using SimulDIESEL.BLL;
using System;
using System.Windows.Forms;
using SimulDIESEL.DTL;


namespace SimulDIESEL.UI
{
    public partial class frmLedGw : Form
    {
        private LedGwTest_BLL _bll;
        public static frmLedGw _instance;

        public frmLedGw()
        {
            InitializeComponent();
        }


        public static frmLedGw Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new frmLedGw();
                return _instance;
            }
        }

        private void frmLedGw_Load(object sender, EventArgs e)
        {
            _bll = new LedGwTest_BLL(
                SerialLink.Service.Sggw,
                isLinked: () => SerialLink.IsLinked
            );

            _bll.LedStatusChanged += Bll_LedStatusChanged;

            btnTogle.Text = "LIGA";
            SetLedUi(false);
        }

        private void btnTogle_Click(object sender, EventArgs e)
        {
            if (!SerialLink.IsLinked)
            {
                MessageBox.Show(
                    "Link não está em Linked.",
                    "SGGW",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!_bll.IsRunning)
            {
                _bll.Start();
                btnTogle.Text = "DESLIGA";
            }
            else
            {
                _bll.Stop(true);
                btnTogle.Text = "LIGA";
            }
        }

        private void btnFechar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Bll_LedStatusChanged(bool isOn)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetLedUi(isOn)));
                return;
            }

            SetLedUi(isOn);
        }

        private void SetLedUi(bool on)
        {
            imgLED.Image = on
                ? Properties.Resources.LedRedLight_18x18
                : Properties.Resources.LedRedDark_18x18;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_bll != null)
            {
                _bll.LedStatusChanged -= Bll_LedStatusChanged;
                _bll.Dispose();
                _bll = null;
            }

            base.OnFormClosing(e);
        }
    }
}
