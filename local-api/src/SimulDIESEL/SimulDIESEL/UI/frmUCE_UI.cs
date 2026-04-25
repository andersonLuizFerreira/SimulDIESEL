using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.FormsLogic.UCE;
using SimulDIESEL.DTL.Boards.UCE;

namespace SimulDIESEL.UI
{
    public partial class frmUCE_UI : Form
    {
        private static frmUCE_UI _instance;

        private readonly FrmUceLogic _logic;
        private bool _acceptedLedState;
        private bool _suppressLedEvent;
        private bool _suppressCanEvents;

        public frmUCE_UI()
        {
            InitializeComponent();

            _logic = FrmUceLogic.CreateDefault();

            chkLed.CheckedChanged += ChkLed_CheckedChanged;
            chkCanEnabled.CheckedChanged += CanControl_Changed;
            cmbCanSpeed.SelectedIndexChanged += CanControl_Changed;
            cmbCanMode.SelectedIndexChanged += CanControl_Changed;
            Load += FrmUCE_UI_Load;

            ApplyInitialCanUiState();
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
            chkCanEnabled.CheckedChanged -= CanControl_Changed;
            cmbCanSpeed.SelectedIndexChanged -= CanControl_Changed;
            cmbCanMode.SelectedIndexChanged -= CanControl_Changed;
            Load -= FrmUCE_UI_Load;

            base.OnFormClosed(e);
        }

        private void ApplyInitialCanUiState()
        {
            _suppressCanEvents = true;
            try
            {
                if (cmbCanSpeed.Items.Count > 1)
                    cmbCanSpeed.SelectedIndex = 1;
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
                SetCanStatusError(result.Message);
                if (showErrorDialog)
                    ShowOperationError(result.Message);
                return;
            }

            ApplyCanStatus(result.Response);
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
        }

        private void SetCanBitrateSelection(int bitrateKbps)
        {
            switch (bitrateKbps)
            {
                case 125:
                    cmbCanSpeed.SelectedIndex = 0;
                    break;
                case 250:
                    cmbCanSpeed.SelectedIndex = 1;
                    break;
                case 500:
                    cmbCanSpeed.SelectedIndex = 2;
                    break;
                case 1000:
                    cmbCanSpeed.SelectedIndex = 3;
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
                    bitrateKbps = 125;
                    break;
                case 1:
                    bitrateKbps = 250;
                    break;
                case 2:
                    bitrateKbps = 500;
                    break;
                case 3:
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
