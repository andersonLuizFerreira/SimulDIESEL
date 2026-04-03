using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimulDIESEL.BLL.Boards.GSA;
using SimulDIESEL.BLL.FormsLogic.GSA;
using SimulDIESEL.DTL.Boards.GSA;
using SimulDIESEL.UI.Controls;

namespace SimulDIESEL.UI
{
    public partial class frmGSA_UI : Form
    {
        private static frmGSA_UI _instance;
        private readonly FrmGsaLogic _logic;
        private readonly ChannelUiState[] _channels = new ChannelUiState[16];
        private bool _builtinLedAppliedState;
        private bool _suppressBuiltinLedEvent;
        private bool _initialRefreshStarted;
        private bool _initialSnapshotLoaded;

        public frmGSA_UI()
        {
            InitializeComponent();

            _logic = FrmGsaLogic.CreateDefault();
            _logic.ChannelFaultEventReceived += Logic_ChannelFaultEventReceived;
            _logic.PhysicalOperationEventReceived += Logic_PhysicalOperationEventReceived;

            _gsaControls.SetpointVoltageChanged += GsaControls_SetpointVoltageChanged;
            _gsaControls.SetpointVoltageCommitted += GsaControls_SetpointVoltageCommitted;
            _gsaControls.OutputEnabledChanged += GsaControls_OutputEnabledChanged;
            _gsaControls.ConfigButtonClick += GsaControls_ConfigButtonClick;
            _builtinLedCheckBox.CheckedChanged += BuiltinLedCheckBox_CheckedChanged;

            Shown += frmGSA_UI_Shown;
            Activated += frmGSA_UI_Activated;

            ApplyInitialVisualState();
        }

        public static frmGSA_UI Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new frmGSA_UI();

                return _instance;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _logic.ChannelFaultEventReceived -= Logic_ChannelFaultEventReceived;
            _logic.PhysicalOperationEventReceived -= Logic_PhysicalOperationEventReceived;
            _gsaControls.SetpointVoltageChanged -= GsaControls_SetpointVoltageChanged;
            _gsaControls.SetpointVoltageCommitted -= GsaControls_SetpointVoltageCommitted;
            _gsaControls.OutputEnabledChanged -= GsaControls_OutputEnabledChanged;
            _gsaControls.ConfigButtonClick -= GsaControls_ConfigButtonClick;
            _builtinLedCheckBox.CheckedChanged -= BuiltinLedCheckBox_CheckedChanged;
            Shown -= frmGSA_UI_Shown;
            Activated -= frmGSA_UI_Activated;
            _logic.Dispose();

            base.OnFormClosed(e);
        }

        private void ApplyInitialVisualState()
        {
            for (int index = 0; index < _channels.Length; index++)
            {
                int channel = index + 1;
                _channels[index] = new ChannelUiState
                {
                    SetpointVoltage = 0d,
                    MeasuredVoltage = 0d,
                    MeasuredCurrent = 0d,
                    OutputEnabled = false,
                    FaultActive = false
                };

                ApplyChannelState(channel);
            }

            SetBuiltinLedCheckboxState(false);
            SetPhysicalResultMessage("Resultado físico: aguardando operação da GSA.", true);
        }

        private void ApplyChannelState(int channel)
        {
            int index = channel - 1;
            ChannelUiState state = _channels[index];
            _gsaControls.SetChannelState(
                channel,
                state.SetpointVoltage,
                state.MeasuredVoltage,
                state.MeasuredCurrent,
                state.OutputEnabled,
                state.FaultActive);
        }

        private async void frmGSA_UI_Shown(object sender, EventArgs e)
        {
            await RefreshAllChannelsIfLinkedAsync().ConfigureAwait(true);
        }

        private async void frmGSA_UI_Activated(object sender, EventArgs e)
        {
            await RefreshAllChannelsIfLinkedAsync().ConfigureAwait(true);
        }

        private async Task RefreshAllChannelsIfLinkedAsync()
        {
            if (_initialRefreshStarted || _initialSnapshotLoaded || !_logic.IsLinked)
                return;

            _initialRefreshStarted = true;

            try
            {
                for (int channel = 1; channel <= _channels.Length; channel++)
                    await RefreshChannelAsync(channel, true).ConfigureAwait(true);

                _initialSnapshotLoaded = true;
            }
            finally
            {
                _initialRefreshStarted = false;
            }
        }

        private void GsaControls_SetpointVoltageChanged(object sender, GsaControls.GsaChannelSetpointChangedEventArgs e)
        {
            _channels[e.ChannelNumber - 1].SetpointVoltage = GsaChannelScaling.ClampVoltage(e.ChannelNumber, e.SetpointVoltage);
        }

        private async void GsaControls_SetpointVoltageCommitted(object sender, GsaControls.GsaChannelSetpointChangedEventArgs e)
        {
            int channel = e.ChannelNumber;
            ChannelUiState state = _channels[channel - 1];
            state.SetpointVoltage = GsaChannelScaling.ClampVoltage(channel, e.SetpointVoltage);

            if (!state.OutputEnabled)
                return;

            await SendChannelSetpointAsync(channel).ConfigureAwait(true);
        }

        private async void GsaControls_OutputEnabledChanged(object sender, GsaControls.GsaChannelOutputEnabledChangedEventArgs e)
        {
            int channel = e.ChannelNumber;
            ChannelUiState state = _channels[channel - 1];
            bool previousState = state.OutputEnabled;

            GsaOperationResult<GsaChannelEnableResponse> result = await _logic
                .SetChannelEnableAsync(channel, e.OutputEnabled)
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                state.OutputEnabled = previousState;
                ApplyChannelState(channel);
                ShowOperationError(result.Message);
                return;
            }

            state.OutputEnabled = result.Response.AppliedState;
            ApplyChannelState(channel);
            await RefreshChannelAsync(channel, false).ConfigureAwait(true);
        }

        private void GsaControls_ConfigButtonClick(object sender, GsaControls.GsaChannelConfigClickEventArgs e)
        {
            MessageBox.Show(
                string.Format("Abrir configuracoes de calibracao do Canal {0}.", e.ChannelNumber),
                "GSA",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private async void BuiltinLedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (_suppressBuiltinLedEvent)
                return;

            GsaCommandResult result = await _logic
                .SetBuiltinLedAsync(_builtinLedCheckBox.Checked)
                .ConfigureAwait(true);

            if (!result.Success || !result.AppliedState.HasValue)
            {
                SetBuiltinLedCheckboxState(_builtinLedAppliedState);
                ShowOperationError(result.Message);
                return;
            }

            _builtinLedAppliedState = result.AppliedState.Value;
            SetBuiltinLedCheckboxState(_builtinLedAppliedState);
        }

        private void Logic_ChannelFaultEventReceived(GsaChannelFaultEvent faultEvent)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Logic_ChannelFaultEventReceived(faultEvent)));
                return;
            }

            ApplySnapshot(faultEvent, true);
        }

        private void Logic_PhysicalOperationEventReceived(GsaPhysicalOperationEvent physicalEvent)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Logic_PhysicalOperationEventReceived(physicalEvent)));
                return;
            }

            if (physicalEvent == null)
                return;

            SetPhysicalResultMessage(BuildPhysicalResultText(physicalEvent), physicalEvent.IsSuccess);

            if (physicalEvent.Channel >= 1 && physicalEvent.Channel <= _channels.Length)
            {
                _ = RefreshChannelAsync(physicalEvent.Channel, false);
            }
        }

        private async Task SendChannelSetpointAsync(int channel)
        {
            ChannelUiState state = _channels[channel - 1];
            byte rawValue = GsaChannelScaling.VoltsToRaw(channel, state.SetpointVoltage);

            GsaOperationResult<GsaChannelSetpointResponse> result = await _logic
                .SetChannelSetpointAsync(channel, rawValue)
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
            {
                ShowOperationError(result.Message);
                return;
            }

            state.SetpointVoltage = GsaChannelScaling.RawToVolts(channel, result.Response.AppliedValue);
            ApplyChannelState(channel);
            await RefreshChannelAsync(channel, false).ConfigureAwait(true);
        }

        private async Task RefreshChannelAsync(int channel, bool updateLocalSetpoint)
        {
            GsaOperationResult<GsaChannelStatusResponse> result = await _logic
                .GetChannelStatusAsync(channel)
                .ConfigureAwait(true);

            if (!result.Success || result.Response == null)
                return;

            ApplySnapshot(result.Response, updateLocalSetpoint);
        }

        private void ApplySnapshot(GsaChannelSnapshot snapshot, bool updateLocalSetpoint)
        {
            if (snapshot == null || snapshot.Channel < 1 || snapshot.Channel > _channels.Length)
                return;

            int index = snapshot.Channel - 1;
            ChannelUiState state = _channels[index];

            if (updateLocalSetpoint)
                state.SetpointVoltage = GsaChannelScaling.RawToVolts(snapshot.Channel, snapshot.Setpoint);

            state.MeasuredVoltage = GsaChannelScaling.RawToVolts(snapshot.Channel, snapshot.VoltageRead);
            state.MeasuredCurrent = GsaChannelScaling.RawToMilliamps(snapshot.CurrentRead);
            state.OutputEnabled = snapshot.Enabled;
            state.FaultActive = snapshot.Fault;
            ApplyChannelState(snapshot.Channel);
        }

        private void SetBuiltinLedCheckboxState(bool value)
        {
            _suppressBuiltinLedEvent = true;
            try
            {
                _builtinLedCheckBox.Checked = value;
            }
            finally
            {
                _suppressBuiltinLedEvent = false;
            }
        }

        private void ShowOperationError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                message = "A operacao da GSA falhou.";

            MessageBox.Show(
                this,
                message,
                "GSA",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private void SetPhysicalResultMessage(string text, bool success)
        {
            _physicalResultLabel.Text = string.IsNullOrWhiteSpace(text)
                ? "Resultado físico: aguardando operação da GSA."
                : text;
            _physicalResultLabel.ForeColor = success
                ? System.Drawing.Color.FromArgb(96, 215, 173)
                : System.Drawing.Color.FromArgb(239, 125, 125);
        }

        private static string BuildPhysicalResultText(GsaPhysicalOperationEvent physicalEvent)
        {
            string operationName = DescribeOriginType(physicalEvent.OriginType);
            string statusText = physicalEvent.IsSuccess
                ? "OK"
                : (physicalEvent.Status == GsaPhysicalOperationStatus.TcaNoAck ? "falha TCA9548" : "falha MCP4725");

            return string.Format(
                "Resultado físico: {0} no canal {1} -> {2}.",
                operationName,
                physicalEvent.Channel,
                statusText);
        }

        private static string DescribeOriginType(byte originType)
        {
            switch (originType)
            {
                case 0x10:
                    return "setpoint";
                case 0x11:
                    return "enable";
                case 0x14:
                    return "enable global";
                case 0x15:
                    return "fault reset";
                default:
                    return "TLV 0x" + originType.ToString("X2");
            }
        }

        private sealed class ChannelUiState
        {
            public double SetpointVoltage;
            public double MeasuredVoltage;
            public double MeasuredCurrent;
            public bool OutputEnabled;
            public bool FaultActive;
        }

        private void _gsaControls_Click(object sender, EventArgs e)
        {

        }
    }
}
