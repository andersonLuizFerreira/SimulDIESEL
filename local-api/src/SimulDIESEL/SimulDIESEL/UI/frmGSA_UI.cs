using System;
using System.Windows.Forms;

namespace SimulDIESEL.UI
{
    public partial class frmGSA_UI : Form
    {
        private static frmGSA_UI _instance;

        public frmGSA_UI()
        {
            InitializeComponent();
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
    }
}
