using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.FormsLogic.UCE;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.UI
{
    public partial class frmUCE_UI : Form
    {
        private static frmUCE_UI _instance;

        private readonly FrmUceLogic _logic;
        private bool _acceptedLedState;
        private bool _suppressLedEvent;
        private bool _suppressCanEvents;
        private readonly Timer _canDriverLogTimer;
        private readonly Timer _canRxGridRefreshTimer;
        private bool _canRxGridRefreshPending;
        private bool _canDriverLogPolling;
        private bool _canPeriodicTxActive;
        private bool _dispatcherOverflowDialogShown;
        private UceCanStatusResponse _lastCanStatus;

        public frmUCE_UI()
        {
            InitializeComponent();

            _logic = FrmUceLogic.CreateDefault();
            _logic.LedEventReceived += Logic_LedEventReceived;
            _logic.CanRxEventReceived += Logic_CanRxEventReceived;
            _logic.CanRxTableChanged += Logic_CanRxTableChanged;
            _logic.DispatcherOverflowDiagnosticReceived += Logic_DispatcherOverflowDiagnosticReceived;
            _logic.CanDiagnosticStateChanged += Logic_CanDiagnosticStateChanged;

            chkLed.CheckedChanged += ChkLed_CheckedChanged;
            chkCanEnabled.CheckedChanged += CanControl_Changed;
            cmbCanSpeed.SelectedIndexChanged += CanControl_Changed;
            cmbCanMode.SelectedIndexChanged += CanControl_Changed;
            btnEnable.Click += BtnEnable_Click;
            Load += FrmUCE_UI_Load;
            tabUCE.SelectedIndexChanged += TabUCE_SelectedIndexChanged;

            _canDriverLogTimer = new Timer();
            _canDriverLogTimer.Interval = 500;
            _canDriverLogTimer.Tick += CanDriverLogTimer_Tick;

            _canRxGridRefreshTimer = new Timer();
            _canRxGridRefreshTimer.Interval = 500;
            _canRxGridRefreshTimer.Tick += CanRxGridRefreshTimer_Tick;

            ApplyInitialCanUiState();
            ConfigureCanRxGrid();
            RefreshCanRxGrid();
            UpdateCanDiagnosticIndicators();
        }

        public static frmUCE_UI Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new frmUCE_UI();

                return _instance;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            chkLed.CheckedChanged -= ChkLed_CheckedChanged;
            _logic.LedEventReceived -= Logic_LedEventReceived;
            _logic.CanRxEventReceived -= Logic_CanRxEventReceived;
            _logic.CanRxTableChanged -= Logic_CanRxTableChanged;
            _logic.DispatcherOverflowDiagnosticReceived -= Logic_DispatcherOverflowDiagnosticReceived;
            _logic.CanDiagnosticStateChanged -= Logic_CanDiagnosticStateChanged;
            chkCanEnabled.CheckedChanged -= CanControl_Changed;
            cmbCanSpeed.SelectedIndexChanged -= CanControl_Changed;
            cmbCanMode.SelectedIndexChanged -= CanControl_Changed;
            btnEnable.Click -= BtnEnable_Click;
            Load -= FrmUCE_UI_Load;
            tabUCE.SelectedIndexChanged -= TabUCE_SelectedIndexChanged;
            _canDriverLogTimer.Stop();
            _canDriverLogTimer.Tick -= CanDriverLogTimer_Tick;
            _canDriverLogTimer.Dispose();
            _canRxGridRefreshTimer.Stop();
            _canRxGridRefreshTimer.Tick -= CanRxGridRefreshTimer_Tick;
            _canRxGridRefreshTimer.Dispose();
            _logic.Dispose();

            base.OnFormClosed(e);
        }

        private void ApplyInitialCanUiState()
        {
            _suppressCanEvents = true;
            try
            {
                if (cmbCanSpeed.Items.Count > 5)
                    cmbCanSpeed.SelectedIndex = 5;
                else if (cmbCanSpeed.Items.Count > 0)
                    cmbCanSpeed.SelectedIndex = 0;

                if (cmbCanMode.Items.Count > 0)
                    cmbCanMode.SelectedIndex = 0;

                chkCanEnabled.Checked = false;
            }
            finally
            {
                _suppressCanEvents = false;
            }

            lblCanStatus.Text = "Status CAN: carregando estado da UCE...";
        }

        private async void FrmUCE_UI_Load(object sender, EventArgs e)
        {
            await RefreshCanStatusAsync(false).ConfigureAwait(true);
            // CAN_RX_EVENT assíncrono é o caminho primário; o poll fica preservado como fallback/diagnóstico.
            _canDriverLogTimer.Start();
            RefreshCanRxGrid();
            UpdateCanDiagnosticIndicators();
        }

        private async void TabUCE_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ReferenceEquals(tabUCE.SelectedTab, tabDados))
            {
                await RefreshCanStatusAsync(false).ConfigureAwait(true);
                RefreshCanRxGrid();
                UpdateCanDiagnosticIndicators();
            }
        }

        private async void ChkLed_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressLedEvent)
                return;

            await ApplyLedStateAsync(chkLed.Checked).ConfigureAwait(true);
        }

        private async void CanControl_Changed(object sender, EventArgs e)
        {
            if (_suppressCanEvents)
                return;

            if (ReferenceEquals(sender, chkCanEnabled))
            {
                await ApplyCanEnabledAsync(chkCanEnabled.Checked).ConfigureAwait(true);
                return;
            }

            await ApplyCanConfigAsync().ConfigureAwait(true);
        }

        private void CanRxGridRefreshTimer_Tick(object sender, EventArgs e)
        {
            _canRxGridRefreshTimer.Stop();

            if (!_canRxGridRefreshPending || IsDisposed || Disposing)
                return;

            _canRxGridRefreshPending = false;
            RefreshCanRxGrid();
        }

        private async void CanDriverLogTimer_Tick(object sender, EventArgs e)
        {
            if (_canDriverLogPolling)
                return;

            _canDriverLogPolling = true;
            try
            {
                await PollCanDriverLogAsync().ConfigureAwait(true);
            }
            finally
            {
                _canDriverLogPolling = false;
            }
        }

        private async void BtnEnable_Click(object sender, EventArgs e)
        {
            btnEnable.Enabled = false;
            try
            {
                if (_canPeriodicTxActive)
                {
                    await StopCanTxAsync().ConfigureAwait(true);
                    return;
                }

                CanTxInput input;
                string validationError;
                if (!TryReadCanTxInput(out input, out validationError))
                {
                    ShowOperationError(validationError);
                    return;
                }

                UceOperationResult<UceCanTxResponse> result = await _logic
                    .SendCanAsync(input.Extended, input.Id, input.Dlc, input.Data, input.PeriodMs)
                    .ConfigureAwait(true);

                if (!result.Success || result.Response == null)
                {
                    ShowOperationError(result.Message);
                    return;
                }

                if (result.Response.TxStatus == SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanTxStatusAcceptedSent)
                {
                    lstMensagens.Items.Add("CAN_TX one-shot enviado.");
                    return;
                }

                if (result.Response.TxStatus == SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanTxStatusPeriodicStarted)
                {
                    _canPeriodicTxActive = true;
                    btnEnable.Text = "Parar";
                    lstMensagens.Items.Add("CAN_TX periódico iniciado. Slot " + result.Response.SequenceOrSlot.ToString(CultureInfo.InvariantCulture) + ".");
                    return;
                }

                ShowOperationError("UCE retornou CAN_TX: " + UceCanProtocol.ToDisplayTxStatus(result.Response.TxStatus) + ".");
            }
            finally
            {
                btnEnable.Enabled = true;
            }
        }

        private async Task ApplyLedStateAsync(bool desiredState)
        {
            UceCommandResult result = await _logic
                .SetBuiltinLedAsync(desiredState)
                .ConfigureAwait(true);

            if (!result.Success || !result.AcceptedState.HasValue)
            {
                SetLedCheckboxState(_acceptedLedState);
                ShowOperationError(result.Message);
                return;
            }

            _acceptedLedState = result.AcceptedState.Value;
            SetLedCheckboxState(_acceptedLedState);
        }

        private void Logic_LedEventReceived(UceLedEvent ledEvent)
        {
            if (ledEvent == null)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Logic_LedEventReceived(ledEvent)));
                return;
            }

            _acceptedLedState = ledEvent.LedState;
            SetLedCheckboxState(ledEvent.LedState);
            lstMensagens.Items.Add(
                "UCE LED EVENT state=" +
                (ledEvent.LedState ? "ON" : "OFF") +
                " code=0x" +
                ledEvent.EventCode.ToString("X2", CultureInfo.InvariantCulture) +
                " counter=" +
                ledEvent.Counter.ToString(CultureInfo.InvariantCulture));

            while (lstMensagens.Items.Count > 200)
                lstMensagens.Items.RemoveAt(0);

            if (lstMensagens.Items.Count > 0)
                lstMensagens.TopIndex = lstMensagens.Items.Count - 1;
        }

        private void Logic_CanRxEventReceived(UceCanRxEvent canRxEvent)
        {
            if (canRxEvent == null)
                return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Logic_CanRxEventReceived(canRxEvent)));
                return;
            }

        }

        private void Logic_CanRxTableChanged(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                if (!IsDisposed && !Disposing)
                    BeginInvoke(new Action(() => Logic_CanRxTableChanged(sender, e)));
                return;
            }

            ScheduleCanRxGridRefresh();
        }

        private void Logic_CanDiagnosticStateChanged(object sender, EventArgs e)
        {
            UpdateCanDiagnosticIndicators();
        }

        private void Logic_DispatcherOverflowDiagnosticReceived(UceDispatcherOverflowDiagnostic diagnostic)
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(() => Logic_DispatcherOverflowDiagnosticReceived(diagnostic)));
                }
                catch (InvalidOperationException)
                {
                }

                return;
            }

            UpdateCanDiagnosticIndicators();

            if (_dispatcherOverflowDialogShown)
                return;

            _dispatcherOverflowDialogShown = true;
            ShowOperationError(UceGatewayDiagnosticLog.BuildDispatcherFifoOverflowMessage(diagnostic));
        }

        private void ScheduleCanRxGridRefresh()
        {
            if (IsDisposed || Disposing)
                return;

            _canRxGridRefreshPending = true;
            if (!_canRxGridRefreshTimer.Enabled)
                _canRxGridRefreshTimer.Start();
        }

        private void SetLedCheckboxState(bool value)
        {
            _suppressLedEvent = true;
            try
            {
                chkLed.Checked = value;
            }
            finally
            {
                _suppressLedEvent = false;
            }
        }

        private async Task ApplyCanConfigAsync()
        {
            int bitrateKbps;
            string mode;
            if (!TryGetSelectedCanConfig(out bitrateKbps, out mode))
                return;

            SetCanStatusText("Status CAN: aplicando configuração...");

            UceOperationResult<UceCanConfigResponse> result = await _logic
                .SetCanConfigAsync(bitrateKbps, mode)
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                await RefreshCanStatusAsync(false).ConfigureAwait(true);
                SetCanStatusError(result.Message);
                ShowOperationError(result.Message);
                return;
            }

            await RefreshCanStatusAsync(false).ConfigureAwait(true);
        }

        private async Task ApplyCanEnabledAsync(bool enabled)
        {
            SetCanStatusText(enabled
                ? "Status CAN: habilitando porta..."
                : "Status CAN: desabilitando porta...");

            UceOperationResult<UceCanEnableResponse> result = await _logic
                .SetCanEnabledAsync(enabled)
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                await RefreshCanStatusAsync(false).ConfigureAwait(true);
                SetCanStatusError(result.Message);
                ShowOperationError(result.Message);
                return;
            }

            await RefreshCanStatusAsync(false).ConfigureAwait(true);
        }

        private async Task RefreshCanStatusAsync(bool showErrorDialog)
        {
            UceOperationResult<UceCanStatusResponse> result = await _logic
                .GetCanStatusAsync()
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                _lastCanStatus = null;
                SetCanStatusError(result.Message);
                UpdateCanDiagnosticIndicators();
                if (showErrorDialog)
                    ShowOperationError(result.Message);
                return;
            }

            _lastCanStatus = result.Response;
            ApplyCanStatus(result.Response);
            UpdateCanDiagnosticIndicators();
        }

        private void ConfigureCanRxGrid()
        {
            if (dgCanRx.Columns.Count > 0)
                return;

            dgCanRx.AutoGenerateColumns = false;
            dgCanRx.Columns.Add("INDEX", "INDEX");
            dgCanRx.Columns.Add("VALID", "VALID");
            dgCanRx.Columns.Add("TIPO", "TIPO");
            dgCanRx.Columns.Add("RTR", "RTR");
            dgCanRx.Columns.Add("CAN_ID", "CAN_ID");
            dgCanRx.Columns.Add("DLC", "DLC");
            dgCanRx.Columns.Add("D0", "D0");
            dgCanRx.Columns.Add("D1", "D1");
            dgCanRx.Columns.Add("D2", "D2");
            dgCanRx.Columns.Add("D3", "D3");
            dgCanRx.Columns.Add("D4", "D4");
            dgCanRx.Columns.Add("D5", "D5");
            dgCanRx.Columns.Add("D6", "D6");
            dgCanRx.Columns.Add("D7", "D7");
            dgCanRx.Columns.Add("CYCLE_TIME_MS", "CYCLE_TIME_MS");
            dgCanRx.Columns.Add("MESSAGE_ORDER", "MESSAGE_ORDER");

            dgCanRx.Rows.Clear();
            for (int index = 0; index < GwProtocol.UceCanRxMirrorCapacity; ++index)
                dgCanRx.Rows.Add();
        }

        private void RefreshCanRxGrid()
        {
            IReadOnlyList<CanRowDto> rows = _logic.GetCanRxMirrorRows();
            if (rows == null)
                return;

            while (dgCanRx.Rows.Count < GwProtocol.UceCanRxMirrorCapacity)
                dgCanRx.Rows.Add();

            for (int index = 0; index < GwProtocol.UceCanRxMirrorCapacity; ++index)
            {
                CanRowDto row = index < rows.Count ? rows[index] : null;
                PopulateCanRxGridRow(dgCanRx.Rows[index], index, row);
            }
        }

        private static void PopulateCanRxGridRow(DataGridViewRow gridRow, int index, CanRowDto row)
        {
            bool valid = row != null && row.Valid;
            gridRow.Cells[0].Value = index.ToString(CultureInfo.InvariantCulture);
            gridRow.Cells[1].Value = valid ? "True" : "False";
            gridRow.Cells[2].Value = valid ? (row.IsExtended ? "EXT" : "STD") : string.Empty;
            gridRow.Cells[3].Value = valid ? (row.IsRemoteRequest ? "True" : "False") : string.Empty;
            gridRow.Cells[4].Value = valid ? "0x" + row.CanId.ToString(row.IsExtended ? "X8" : "X3", CultureInfo.InvariantCulture) : string.Empty;
            gridRow.Cells[5].Value = valid ? row.Dlc.ToString(CultureInfo.InvariantCulture) : string.Empty;

            for (int dataIndex = 0; dataIndex < 8; ++dataIndex)
            {
                string cellValue = string.Empty;
                if (valid && row.Data != null && dataIndex < row.Data.Length)
                    cellValue = row.Data[dataIndex].ToString("X2", CultureInfo.InvariantCulture);

                gridRow.Cells[6 + dataIndex].Value = cellValue;
            }

            gridRow.Cells[14].Value = valid ? row.CycleTime.ToString(CultureInfo.InvariantCulture) : string.Empty;
            gridRow.Cells[15].Value = valid ? row.MessageOrder.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        private async Task PollCanDriverLogAsync()
        {
            UceOperationResult<UceCanDriverLogPollResponse> result = await _logic
                .PollCanDriverLogAsync()
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
                return;

            foreach (UceCanDriverLogEntry entry in result.Response.Entries)
            {
                lstMensagens.Items.Add(FormatDriverLogEntry(result.Response.Controller, entry));
            }

            while (lstMensagens.Items.Count > 200)
                lstMensagens.Items.RemoveAt(0);

            if (lstMensagens.Items.Count > 0)
                lstMensagens.TopIndex = lstMensagens.Items.Count - 1;
        }

        private void ApplyCanStatus(UceCanStatusResponse status)
        {
            _suppressCanEvents = true;
            try
            {
                chkCanEnabled.Checked = UceCanProtocol.IsEnabled(status.State);
                SetCanBitrateSelection(status.BitrateKbps);
                SetCanModeSelection(status.Mode);
            }
            finally
            {
                _suppressCanEvents = false;
            }

            SetCanStatusText(
                "Status CAN: " +
                UceCanProtocol.ToDisplayState(status.State) +
                ", " +
                status.BitrateKbps +
                " kbps, modo " +
                UceCanProtocol.ToDisplayMode(status.Mode));
            UpdateCanDiagnosticIndicators();
        }

        private void UpdateCanDiagnosticIndicators()
        {
            if (IsDisposed || Disposing)
                return;

            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new Action(UpdateCanDiagnosticIndicators));
                }
                catch (InvalidOperationException)
                {
                }

                return;
            }

            CanDiagnosticStatusDto status = _logic.GetCanDiagnosticStatus(_lastCanStatus);
            lblCanDiagMirror.Text = "Mirror: " + status.MirrorStatusText;
            lblCanDiagSync.Text = "Sync: " + status.SyncStatusText;
            lblCanDiagDispatcher.Text = "Dispatcher FIFO: " + status.DispatcherStatusText;
            lblCanDiagTable.Text = "Tabela: " + status.TableStatusText;
            lblCanDiagCan.Text = "CAN: " + status.CanStatusText;
            lblCanDiagLastError.Text = "Último erro: " + status.LastErrorText;

            SetDiagnosticLabelColor(lblCanDiagMirror, status.MirrorStatusText == "OK");
            SetDiagnosticLabelColor(lblCanDiagSync, status.SyncStatusText == "Estável");
            SetDiagnosticLabelColor(lblCanDiagDispatcher, status.DispatcherStatusText == "OK");
            SetDiagnosticLabelColor(lblCanDiagTable, true);
            SetDiagnosticLabelColor(lblCanDiagCan, status.CanStatusText != "ERRO");
            SetDiagnosticLabelColor(lblCanDiagLastError, status.LastErrorText == "-");
        }

        private static void SetDiagnosticLabelColor(Label label, bool normal)
        {
            label.ForeColor = normal
                ? System.Drawing.SystemColors.ControlText
                : System.Drawing.Color.DarkOrange;
        }

        private void SetCanBitrateSelection(int bitrateKbps)
        {
            switch (bitrateKbps)
            {
                case 5:
                    cmbCanSpeed.SelectedIndex = 0;
                    break;
                case 10:
                    cmbCanSpeed.SelectedIndex = 1;
                    break;
                case 25:
                    cmbCanSpeed.SelectedIndex = 2;
                    break;
                case 50:
                    cmbCanSpeed.SelectedIndex = 3;
                    break;
                case 125:
                    cmbCanSpeed.SelectedIndex = 4;
                    break;
                case 250:
                    cmbCanSpeed.SelectedIndex = 5;
                    break;
                case 500:
                    cmbCanSpeed.SelectedIndex = 6;
                    break;
                case 800:
                    cmbCanSpeed.SelectedIndex = 7;
                    break;
                case 1000:
                    cmbCanSpeed.SelectedIndex = 8;
                    break;
            }
        }

        private void SetCanModeSelection(UceCanMode mode)
        {
            cmbCanMode.SelectedIndex = mode == UceCanMode.Listen ? 1 : 0;
        }

        private bool TryGetSelectedCanConfig(out int bitrateKbps, out string mode)
        {
            bitrateKbps = 250;
            mode = "normal";

            switch (cmbCanSpeed.SelectedIndex)
            {
                case 0:
                    bitrateKbps = 5;
                    break;
                case 1:
                    bitrateKbps = 10;
                    break;
                case 2:
                    bitrateKbps = 25;
                    break;
                case 3:
                    bitrateKbps = 50;
                    break;
                case 4:
                    bitrateKbps = 125;
                    break;
                case 5:
                    bitrateKbps = 250;
                    break;
                case 6:
                    bitrateKbps = 500;
                    break;
                case 7:
                    bitrateKbps = 800;
                    break;
                case 8:
                    bitrateKbps = 1000;
                    break;
                default:
                    return false;
            }

            switch (cmbCanMode.SelectedIndex)
            {
                case 0:
                    mode = "normal";
                    return true;
                case 1:
                    mode = "listen";
                    return true;
                default:
                    return false;
            }
        }

        private async Task StopCanTxAsync()
        {
            UceOperationResult<UceCanTxStopResponse> result = await _logic
                .StopCanTxAsync()
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                ShowOperationError(result.Message);
                return;
            }

            if (result.Response.TxStatus != SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanTxStatusPeriodicStopped)
            {
                ShowOperationError("UCE retornou CAN_TX_STOP: " + UceCanProtocol.ToDisplayTxStatus(result.Response.TxStatus) + ".");
                return;
            }

            _canPeriodicTxActive = false;
            btnEnable.Text = "Iniciar";
            lstMensagens.Items.Add("CAN_TX periódico parado.");
        }

        private struct CanTxInput
        {
            public bool Extended;
            public uint Id;
            public byte Dlc;
            public byte[] Data;
            public ushort PeriodMs;
        }

        private bool TryReadCanTxInput(out CanTxInput input, out string error)
        {
            input = new CanTxInput { Data = new byte[8] };
            error = null;

            bool extended;
            if (!TryGetCanTxIdKind(out extended))
            {
                error = "Selecione o tipo de mensagem CAN: 1.0/STD ou 2.0/EXT.";
                return false;
            }

            uint id;
            if (!TryParseUInt32Flexible(txtID.Text, out id))
            {
                error = "Informe um ID CAN válido.";
                return false;
            }

            if (!extended && id > 0x7FFU)
            {
                error = "ID inválido para CAN 1.0 / STD. Valor máximo: 0x7FF.";
                return false;
            }

            if (extended && id > 0x1FFFFFFFU)
            {
                error = "ID inválido para CAN 2.0 / EXT. Valor máximo: 0x1FFFFFFF.";
                return false;
            }

            int len;
            if (!int.TryParse(txtLEN.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out len) ||
                len < 0 ||
                len > 8)
            {
                error = "LEN deve estar entre 0 e 8.";
                return false;
            }

            TextBox[] dataFields = { txtD0, txtD1, txtD2, txtD3, txtD4, txtD5, txtD6, txtD7 };
            for (int i = 0; i < len; ++i)
            {
                byte dataByte;
                if (!TryParseByteFlexible(dataFields[i].Text, out dataByte))
                {
                    error = "D" + i.ToString(CultureInfo.InvariantCulture) + " deve ser hexadecimal entre 00 e FF.";
                    return false;
                }

                input.Data[i] = dataByte;
            }

            int repeatMs;
            if (!int.TryParse(txtTime.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out repeatMs) ||
                repeatMs < 0)
            {
                error = "Tempo de repetição deve ser maior ou igual a 0 ms.";
                return false;
            }

            if (repeatMs > ushort.MaxValue)
            {
                error = "Tempo de repetição deve estar entre 0 e 65535 ms.";
                return false;
            }

            input.Extended = extended;
            input.Id = id;
            input.Dlc = (byte)len;
            input.PeriodMs = (ushort)repeatMs;
            return true;
        }

        private bool TryGetCanTxIdKind(out bool extended)
        {
            extended = true;
            string value = cboCANTYPE.Text != null ? cboCANTYPE.Text.Trim() : string.Empty;
            if (string.Equals(value, "1.0", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "STD", StringComparison.OrdinalIgnoreCase) ||
                value.IndexOf("STD", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                extended = false;
                return true;
            }

            if (string.Equals(value, "2.0", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "EXT", StringComparison.OrdinalIgnoreCase) ||
                value.IndexOf("EXT", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                extended = true;
                return true;
            }

            return false;
        }

        private static bool TryParseUInt32Flexible(string text, out uint value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            string trimmed = text.Trim();
            if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return uint.TryParse(
                    trimmed.Substring(2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            return uint.TryParse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryParseByteFlexible(string text, out byte value)
        {
            value = 0;
            uint parsed;
            if (!TryParseUInt32Flexible(text, out parsed) || parsed > byte.MaxValue)
                return false;

            value = (byte)parsed;
            return true;
        }

        private static string FormatCanFrame(UceCanController controller, UceCanFrame frame)
        {
            var builder = new StringBuilder();
            builder.Append(controller == UceCanController.Can1 ? "CAN1 " : "CAN0 ");
            builder.Append(frame.Extended ? "EXT " : "STD ");
            builder.Append("ID=0x");
            builder.Append(frame.Id.ToString(frame.Extended ? "X8" : "X3", CultureInfo.InvariantCulture));
            builder.Append(" LEN=");
            builder.Append(frame.Dlc.ToString(CultureInfo.InvariantCulture));

            if (frame.RemoteRequest)
                builder.Append(" RTR");

            for (int i = 0; i < 8; ++i)
            {
                builder.Append(" D");
                builder.Append(i.ToString(CultureInfo.InvariantCulture));
                builder.Append("=");
                builder.Append(frame.Data[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        private static string FormatDriverLogEntry(UceCanController controller, UceCanDriverLogEntry entry)
        {
            var builder = new StringBuilder();
            builder.Append("[");
            builder.Append(controller == UceCanController.Can1 ? "CAN1" : "CAN0");
            builder.Append("] ");
            builder.Append(DescribeDriverEvent(entry.EventCode));
            builder.Append(" - ");
            builder.Append(entry.BitrateKbps.ToString(CultureInfo.InvariantCulture));
            builder.Append(" kbps - ");
            builder.Append(UceCanProtocol.ToDisplayMode(entry.Mode));
            builder.Append(" - state=");
            builder.Append(UceCanProtocol.ToDisplayState(entry.InterfaceState));

            string detail = FormatDriverEventDetail(entry);
            if (!string.IsNullOrEmpty(detail))
            {
                builder.Append(" - ");
                builder.Append(detail);
            }

            return builder.ToString();
        }

        private static string DescribeDriverEvent(byte eventCode)
        {
            switch (eventCode)
            {
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventBegin:
                    return "DRIVER BEGIN";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigRequested:
                    return "CONFIG REQUESTED";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigOk:
                    return "CONFIG OK";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigFault:
                    return "CONFIG FAULT";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventOpenRequested:
                    return "OPEN REQUESTED";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventOpenOk:
                    return "OPEN OK";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventOpenFault:
                    return "OPEN FAULT";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventCloseRequested:
                    return "CLOSE REQUESTED";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventCloseOk:
                    return "CLOSE OK";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventResetRequested:
                    return "RESET REQUESTED";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventResetOk:
                    return "RESET OK";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventStatusSnapshot:
                    return "STATUS SNAPSHOT";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventRxPoll:
                    return "RX POLL";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventRxFrameRead:
                    return "RX FRAME READ";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventUnsupportedController:
                    return "UNSUPPORTED CONTROLLER";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventInvalidBitrate:
                    return "INVALID BITRATE";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventInvalidMode:
                    return "INVALID MODE";
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventCanPhysicalError:
                    return "CAN PHYSICAL ERROR";
                default:
                    return "EVENT 0x" + eventCode.ToString("X2", CultureInfo.InvariantCulture);
            }
        }

        private static string FormatDriverEventDetail(UceCanDriverLogEntry entry)
        {
            switch (entry.EventCode)
            {
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigRequested:
                    return "bitrateCode=0x" + entry.Detail0.ToString("X2", CultureInfo.InvariantCulture) +
                           ", modeCode=0x" + entry.Detail1.ToString("X2", CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventConfigOk:
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventOpenOk:
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventStatusSnapshot:
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventCanPhysicalError:
                    return "status=0x" + entry.Detail0.ToString("X2", CultureInfo.InvariantCulture) +
                           ", txErr=" + entry.Detail1.ToString(CultureInfo.InvariantCulture) +
                           ", rxErr=" + entry.Detail2.ToString(CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventRxPoll:
                    return "maxFrames=" + entry.Detail0.ToString(CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventRxFrameRead:
                    return "ext=" + entry.Detail0.ToString(CultureInfo.InvariantCulture) +
                           ", rtr=" + entry.Detail1.ToString(CultureInfo.InvariantCulture) +
                           ", dlc=" + entry.Detail2.ToString(CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventInvalidBitrate:
                    return "bitrateCode=0x" + entry.Detail0.ToString("X2", CultureInfo.InvariantCulture);
                case SimulDIESEL.DTL.Protocols.SDGW.GwProtocol.UceCanDriverEventInvalidMode:
                    return "modeCode=0x" + entry.Detail0.ToString("X2", CultureInfo.InvariantCulture);
                default:
                    return string.Empty;
            }
        }

        private void SetCanStatusText(string message)
        {
            lblCanStatus.Text = message;
        }

        private void SetCanStatusError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "falha desconhecida";

            lblCanStatus.Text = "Status CAN: erro - " + message;
        }

        private void ShowOperationError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "A operacao da UCE falhou.";

            MessageBox.Show(
                this,
                message,
                "UCE",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
