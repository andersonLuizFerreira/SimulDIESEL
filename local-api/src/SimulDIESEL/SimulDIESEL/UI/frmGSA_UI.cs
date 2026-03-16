using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimulDIESEL.BLL;

namespace SimulDIESEL.UI
{
    public partial class frmGSA_UI : Form
    {
        private static frmGSA_UI _instance;
        private GsaLedService _ledService;
        private bool _syncingSelection;
        private bool _lastKnownLedOn;

        public frmGSA_UI()
        {
            InitializeComponent();

            rdoLedOn.CheckedChanged += async (_, __) => await HandleLedSelectionChangedAsync(rdoLedOn, true);
            rdoLedOff.CheckedChanged += async (_, __) => await HandleLedSelectionChangedAsync(rdoLedOff, false);
        }

        public static frmGSA_UI Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                {
                    _instance = new frmGSA_UI();
                }

                return _instance;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_ledService == null)
            {
                _ledService = new GsaLedService(
                    SerialLink.Service.Gsa,
                    isLinked: () => SerialLink.IsLinked);
            }

            SyncLedSelection(false);
        }

        private async Task HandleLedSelectionChangedAsync(RadioButton radio, bool ligado)
        {
            if (_syncingSelection || !radio.Checked)
            {
                return;
            }

            await ApplyLedStateAsync(ligado);
        }

        private async Task ApplyLedStateAsync(bool ligado)
        {
            SetLedControlsEnabled(false);

            GsaLedCommandResult result = await _ledService.SetBuiltinLedAsync(ligado);
            if (result.Success)
            {
                _lastKnownLedOn = result.AppliedState ?? ligado;
                SyncLedSelection(_lastKnownLedOn);
            }
            else
            {
                SyncLedSelection(_lastKnownLedOn);
                MessageBox.Show(
                    result.Message,
                    "GSA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            SetLedControlsEnabled(true);
        }

        private void SyncLedSelection(bool ligado)
        {
            _syncingSelection = true;
            try
            {
                rdoLedOn.Checked = ligado;
                rdoLedOff.Checked = !ligado;
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private void SetLedControlsEnabled(bool enabled)
        {
            rdoLedOn.Enabled = enabled;
            rdoLedOff.Enabled = enabled;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_ledService != null)
            {
                _ledService.Dispose();
                _ledService = null;
            }

            base.OnFormClosed(e);
        }
    }
}
