using SimulDIESEL.BLL.FormsLogic.BPM;
using SimulDIESEL.DTL.Boards.BPM;
using SimulDIESEL.UI;
using System;
using System.Windows.Forms;

namespace SimulDIESEL
{
    public partial class DashBoard : Form
    {
        private readonly FrmBpmLogic _bpmLogic;

        public DashBoard()
        {
            InitializeComponent();

            _bpmLogic = FrmBpmLogic.CreateDefault();
            _bpmLogic.StatusChanged += BpmLogic_StatusChanged;
            _bpmLogic.Error += BpmLogic_Error;

            AtualizarBotaoConectar();
            AtualizarIndicadores();
            AtualizarNomeDaInterface();
        }

        private void toolStripConectar_Click(object sender, EventArgs e)
        {
            BpmStatusDto status = _bpmLogic.GetStatus();
            if (status.IsConnected)
            {
                _bpmLogic.Disconnect();
                AtualizarBotaoConectar();
                AtualizarIndicadores();
                AtualizarNomeDaInterface();
                return;
            }

            var frmConexaoSerial = frmPortaSerial_UI.Instance;

            if (frmConexaoSerial.Visible)
            {
                frmConexaoSerial.BringToFront();
                frmConexaoSerial.Activate();
                return;
            }

            frmConexaoSerial.MdiParent = this;
            frmConexaoSerial.StartPosition = FormStartPosition.CenterParent;
            frmConexaoSerial.Show();
        }

        private void AtualizarBotaoConectar()
        {
            BpmStatusDto status = _bpmLogic.GetStatus();

            toolStripConectar.Text = status.IsConnected ? "Desconectar" : "Conectar";
            toolStripConectar.Image = status.IsConnected
                ? Properties.Resources.Conectado
                : Properties.Resources.Desconectado;
        }

        private void AtualizarIndicadores()
        {
            BpmStatusDto status = _bpmLogic.GetStatus();

            tsLedSerial.Image = status.IsConnected
                ? Properties.Resources.LedGreenBright_18x18
                : Properties.Resources.LedRedDark_18x18;

            tsLabelSerial.Text = "Status da Serial: " + (status.IsConnected ? "Conectado" : "Desconectado");

            if (!status.IsConnected)
            {
                tsLedLink.Image = Properties.Resources.LedGrayOff_18x18;
                tsNomeInterface.Text = "Nenhum";
                return;
            }

            switch (status.LinkState)
            {
                case "Linked":
                    tsLedLink.Image = Properties.Resources.LedGreenBright_18x18;
                    break;
                case "LinkFailed":
                    tsLedLink.Image = Properties.Resources.LedRedBright_18x18;
                    break;
                case "Draining":
                case "BannerSent":
                case "SerialConnected":
                    tsLedLink.Image = Properties.Resources.LedYellowBright_18x18;
                    break;
                default:
                    tsLedLink.Image = Properties.Resources.LedGrayOff_18x18;
                    break;
            }
        }

        private void AtualizarNomeDaInterface()
        {
            tsNomeInterface.Text = _bpmLogic.GetInterfaceDisplayName();
        }

        private void BpmLogic_StatusChanged()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(BpmLogic_StatusChanged));
                return;
            }

            AtualizarBotaoConectar();
            AtualizarIndicadores();
            AtualizarNomeDaInterface();
        }

        private void BpmLogic_Error(string[] msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => BpmLogic_Error(msg)));
                return;
            }

            // opcional: log
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _bpmLogic.StatusChanged -= BpmLogic_StatusChanged;
            _bpmLogic.Error -= BpmLogic_Error;
            _bpmLogic.Dispose();

            base.OnFormClosing(e);
        }

        private void DashBoard_Load(object sender, EventArgs e)
        {
        }

        private void gSAGERADORDENIVEISANALOGICOSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frmGsa = frmGSA_UI.Instance;

            if (frmGsa.Visible)
            {
                frmGsa.BringToFront();
                frmGsa.Activate();
                return;
            }

            frmGsa.MdiParent = this;
            frmGsa.StartPosition = FormStartPosition.CenterParent;
            frmGsa.Show();
        }
    }
}
