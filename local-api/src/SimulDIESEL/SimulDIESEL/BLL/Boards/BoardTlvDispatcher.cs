using System;
using System.Threading;
using System.Threading.Tasks;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards
{
    internal sealed class BoardTlvDispatcher : IDisposable
    {
        private sealed class PendingBoardRequest
        {
            public TaskCompletionSource<SdgwFrame> ResponseSource { get; set; }
            public Func<SdgwFrame, bool> MatchFrame { get; set; }
        }

        private const int ResponseTimeoutMs = 2000;

        private readonly SdhClient _sdh;
        private readonly SdgwSession _sdgwSession;
        private readonly ushort _compactCommand;
        private readonly SemaphoreSlim _requestGate = new SemaphoreSlim(1, 1);

        private PendingBoardRequest _pendingRequest;
        private bool _disposed;

        public BoardTlvDispatcher(SdhClient sdh, SdgwSession sdgwSession, byte boardAddress, byte transactOp)
        {
            _sdh = sdh ?? throw new ArgumentNullException(nameof(sdh));
            _sdgwSession = sdgwSession ?? throw new ArgumentNullException(nameof(sdgwSession));
            _compactCommand = GwProtocol.MakeCompactCommand(boardAddress, transactOp);

            _sdgwSession.FrameReceived += OnFrameReceived;
        }

        public async Task<BoardTlvDispatchResult> TransactAsync(
            SdhCommand command,
            Func<SdgwFrame, bool> matchFrame,
            string operationName)
        {
            ThrowIfDisposed();

            await _requestGate.WaitAsync().ConfigureAwait(false);
            try
            {
                _pendingRequest = new PendingBoardRequest
                {
                    ResponseSource = new TaskCompletionSource<SdgwFrame>(TaskCreationOptions.RunContinuationsAsynchronously),
                    MatchFrame = matchFrame
                };

                SdGwLinkEngine.SendOutcome outcome = await _sdh.SendAsync(
                    command,
                    SdGwTxPriority.High,
                    operationName).ConfigureAwait(false);

                if (outcome != SdGwLinkEngine.SendOutcome.Acked)
                    return BoardTlvDispatchResult.Fail(TranslateOutcome(outcome, operationName), outcome);

                SdgwFrame responseFrame = await WaitForResponseAsync(_pendingRequest).ConfigureAwait(false);
                return BoardTlvDispatchResult.Succeeded(responseFrame, outcome);
            }
            catch (OperationCanceledException)
            {
                return BoardTlvDispatchResult.Fail("Timeout aguardando a resposta da board para " + operationName + ".");
            }
            catch (Exception ex)
            {
                return BoardTlvDispatchResult.Fail("Falha ao processar a resposta da board para " + operationName + ": " + ex.Message);
            }
            finally
            {
                _pendingRequest = null;
                _requestGate.Release();
            }
        }

        private void OnFrameReceived(SdgwFrame frame)
        {
            PendingBoardRequest pending = _pendingRequest;
            if (pending == null || frame == null)
                return;

            if ((frame.Flags & 0x02) != 0)
                return;

            if (frame.Cmd != _compactCommand)
                return;

            Func<SdgwFrame, bool> matcher = pending.MatchFrame;
            if (matcher == null || !matcher(frame))
                return;

            pending.ResponseSource.TrySetResult(frame);
        }

        private static async Task<SdgwFrame> WaitForResponseAsync(PendingBoardRequest pendingRequest)
        {
            if (pendingRequest == null)
                throw new OperationCanceledException();

            Task finished = await Task.WhenAny(
                pendingRequest.ResponseSource.Task,
                Task.Delay(ResponseTimeoutMs)).ConfigureAwait(false);

            if (finished != pendingRequest.ResponseSource.Task)
                throw new OperationCanceledException();

            return await pendingRequest.ResponseSource.Task.ConfigureAwait(false);
        }

        private static string TranslateOutcome(SdGwLinkEngine.SendOutcome outcome, string operationName)
        {
            switch (outcome)
            {
                case SdGwLinkEngine.SendOutcome.Nacked:
                    return "A BPM rejeitou o comando para " + operationName + ".";
                case SdGwLinkEngine.SendOutcome.Timeout:
                    return "Timeout aguardando ACK do gateway para " + operationName + ".";
                case SdGwLinkEngine.SendOutcome.TransportDown:
                    return "O transporte ativo da BPM está indisponível no momento.";
                case SdGwLinkEngine.SendOutcome.Busy:
                    return "O link estava temporariamente ocupado. Tente novamente.";
                case SdGwLinkEngine.SendOutcome.Enqueued:
                    return "O comando foi enfileirado, mas não houve confirmação do gateway.";
                default:
                    return "Falha ao enviar comando para " + operationName + ".";
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BoardTlvDispatcher));
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _sdgwSession.FrameReceived -= OnFrameReceived;
            _requestGate.Dispose();
            _disposed = true;
        }
    }

    internal sealed class BoardTlvDispatchResult
    {
        private BoardTlvDispatchResult(bool success, SdgwFrame frame, string message, SdGwLinkEngine.SendOutcome? sendOutcome)
        {
            Success = success;
            Frame = frame;
            Message = message;
            SendOutcome = sendOutcome;
        }

        public bool Success { get; }
        public SdgwFrame Frame { get; }
        public string Message { get; }
        public SdGwLinkEngine.SendOutcome? SendOutcome { get; }

        public static BoardTlvDispatchResult Succeeded(SdgwFrame frame, SdGwLinkEngine.SendOutcome sendOutcome)
        {
            return new BoardTlvDispatchResult(true, frame, null, sendOutcome);
        }

        public static BoardTlvDispatchResult Fail(string message, SdGwLinkEngine.SendOutcome? sendOutcome = null)
        {
            return new BoardTlvDispatchResult(false, null, message, sendOutcome);
        }
    }
}
