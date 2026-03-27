using SimulDIESEL.BLL.FormsLogic.BPM;
using SimulDIESEL.DAL.Transport;
using SimulDIESEL.DTL.Boards.BPM;
using SimulDIESEL.UI;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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

            AtualizarBotoesConexao();
            AtualizarIndicadores();
            AtualizarNomeDaInterface();
        }

        private void toolStripConectar_Click(object sender, EventArgs e)
        {
            BpmStatusDto status = _bpmLogic.GetStatus();
            if (status.IsConnected && status.TransportKind == TransportKind.Serial)
            {
                _bpmLogic.Disconnect();
                AtualizarBotoesConexao();
                AtualizarIndicadores();
                AtualizarNomeDaInterface();
                return;
            }

            if (status.IsConnected)
            {
                MessageBox.Show(
                    "Ja existe uma sessao ativa via " + status.TransportKind + ". Desconecte antes de iniciar a Serial.",
                    "Conexao Serial",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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

        private void toolStripBluetooth_Click(object sender, EventArgs e)
        {
            BpmStatusDto status = _bpmLogic.GetStatus();

            if (status.IsConnected && status.TransportKind == TransportKind.Bluetooth)
            {
                _bpmLogic.Disconnect();
                AtualizarBotoesConexao();
                AtualizarIndicadores();
                AtualizarNomeDaInterface();
                return;
            }

            if (status.IsConnected)
            {
                MessageBox.Show(
                    "Ja existe uma sessao ativa via " + status.TransportKind + ". Desconecte antes de iniciar o Bluetooth.",
                    "Conexao Bluetooth",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var frmBluetooth = frmBluetoothConnect.Instance;

            if (frmBluetooth.Visible)
            {
                frmBluetooth.BringToFront();
                frmBluetooth.Activate();
                return;
            }

            frmBluetooth.MdiParent = this;
            frmBluetooth.StartPosition = FormStartPosition.CenterParent;
            frmBluetooth.Show();
        }

        private void AtualizarBotoesConexao()
        {
            BpmStatusDto status = _bpmLogic.GetStatus();

            bool serialAtivo = status.IsConnected && status.TransportKind == TransportKind.Serial;
            bool bluetoothAtivo = status.IsConnected && status.TransportKind == TransportKind.Bluetooth;

            toolStripConectar.Text = serialAtivo ? "Desconectar" : "Serial";
            toolStripConectar.Image = serialAtivo
                ? Properties.Resources.Conectado
                : Properties.Resources.Desconectado;
            toolStripConectar.ToolTipText = "Conectar via Serial";

            toolStripBluetooth.Text = bluetoothAtivo ? "Desconectar BT" : "Bluetooth";
            toolStripBluetooth.Image = CreateBluetoothImage(bluetoothAtivo);
            toolStripBluetooth.ToolTipText = "Conectar via Bluetooth";
        }

        private void AtualizarIndicadores()
        {
            BpmStatusDto status = _bpmLogic.GetStatus();
            bool bluetoothAtivo = status.IsConnected && status.TransportKind == TransportKind.Bluetooth;

            tsLedSerial.Image = status.IsConnected
                ? Properties.Resources.LedGreenBright_18x18
                : Properties.Resources.LedRedDark_18x18;

            tsLabelSerial.Text = bluetoothAtivo
                ? "Status do Bluetooth: " + (status.IsConnected ? "Conectado" : "Desconectado")
                : "Status da Serial: " + (status.IsConnected ? "Conectado" : "Desconectado");

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
                case "BluetoothConnected":
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

            AtualizarBotoesConexao();
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

        private static Bitmap CreateBluetoothImage(bool connected)
        {
            Bitmap bmp = new Bitmap(20, 20);

            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen pen = new Pen(connected ? Color.ForestGreen : Color.DodgerBlue, 2f))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                Point top = new Point(10, 1);
                Point mid = new Point(10, 10);
                Point bottom = new Point(10, 19);
                Point leftTop = new Point(5, 5);
                Point rightTop = new Point(15, 8);
                Point leftBottom = new Point(5, 15);
                Point rightBottom = new Point(15, 12);

                g.DrawLine(pen, top, bottom);
                g.DrawLine(pen, top, rightTop);
                g.DrawLine(pen, rightTop, mid);
                g.DrawLine(pen, mid, leftTop);
                g.DrawLine(pen, mid, rightBottom);
                g.DrawLine(pen, rightBottom, bottom);
                g.DrawLine(pen, leftBottom, mid);
            }

            return bmp;
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

        private void demoGaugesVerticaisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var formGaugeDemo = FormGaugeDemo.Instance;

            if (formGaugeDemo.Visible)
            {
                formGaugeDemo.BringToFront();
                formGaugeDemo.Activate();
                return;
            }

            formGaugeDemo.MdiParent = this;
            formGaugeDemo.StartPosition = FormStartPosition.CenterParent;
            formGaugeDemo.Show();
        }
    }
}
