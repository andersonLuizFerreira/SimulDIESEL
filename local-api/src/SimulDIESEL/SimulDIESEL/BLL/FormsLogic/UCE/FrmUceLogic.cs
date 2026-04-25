using System;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE;

namespace SimulDIESEL.BLL.FormsLogic.UCE
{
    public sealed class FrmUceLogic
    {
        private const string DefaultCanController = "can0";

        private readonly IUceDispatcher _uceDispatcher;
        private readonly Func<bool> _isLinked;

        public FrmUceLogic(IUceDispatcher uceDispatcher, Func<bool> isLinked)
        {
            _uceDispatcher = uceDispatcher ?? throw new ArgumentNullException(nameof(uceDispatcher));
            _isLinked = isLinked ?? throw new ArgumentNullException(nameof(isLinked));
        }

        public bool IsLinked
        {
            get { return _isLinked(); }
        }

        public static FrmUceLogic CreateDefault()
        {
            BpmSerialService service = BpmSerialService.Shared;
            return new FrmUceLogic(service.BoardDispatcher.Uce, () => service.IsLinked);
        }

        public Task<UceCommandResult> SetBuiltinLedAsync(bool ligado)
        {
            if (!_isLinked())
                return Task.FromResult(UceCommandResult.Fail("Link serial não está em estado Linked."));

            return _uceDispatcher.SetBuiltinLedAsync(ligado);
        }

        public Task<UceOperationResult<UceCanConfigResponse>> SetCanConfigAsync(int bitrateKbps, string mode)
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanConfigResponse>();

            return _uceDispatcher.SetCanConfigAsync(DefaultCanController, bitrateKbps, mode);
        }

        public Task<UceOperationResult<UceCanEnableResponse>> SetCanEnabledAsync(bool enabled)
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanEnableResponse>();

            return _uceDispatcher.SetCanEnabledAsync(DefaultCanController, enabled);
        }

        public Task<UceOperationResult<UceCanStatusResponse>> GetCanStatusAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanStatusResponse>();

            return _uceDispatcher.GetCanStatusAsync(DefaultCanController);
        }

        public Task<UceOperationResult<UceCanResetResponse>> ResetCanAsync()
        {
            if (!_isLinked())
                return FailWhenNotLinked<UceCanResetResponse>();

            return _uceDispatcher.ResetCanAsync(DefaultCanController);
        }

        private static Task<UceOperationResult<T>> FailWhenNotLinked<T>()
            where T : class
        {
            return Task.FromResult(UceOperationResult<T>.Fail("Link serial não está em estado Linked."));
        }
    }
}
