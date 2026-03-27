using System;
using System.Drawing;
using System.Windows.Forms;
using SimulDIESEL.BLL.FormsLogic.BPM;
using SimulDIESEL.DAL.Transport;
using SimulDIESEL.DTL.Boards.BPM;

namespace SimulDIESEL.UI
{
    public partial class frmBluetoothConnect : Form
    {
        private readonly ToolStripStatusLabel sslabel1 = new ToolStripStatusLabel("Porta BT:");
        private readonly ToolStripStatusLabel ssPorta = new ToolStripStatusLabel("");
        private readonly ToolStripStatusLabel sslabel2 = new ToolStripStatusLabel("Dispositivo:");
        private readonly ToolStripStatusLabel ssDispositivo = new ToolStripStatusLabel("");
        private readonly ToolStripStatusLabel sslabel3 = new ToolStripStatusLabel("Status:");
        private readonly ToolStripStatusLabel ssStatus = new ToolStripStatusLabel("");

        private static frmBluetoothConnect _instance;
        private FrmBpmLogic _logic;

        public static frmBluetoothConnect Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new frmBluetoothConnect();
                return _instance;
            }
        }

        private frmBluetoothConnect()
        {
            InitializeComponent();

            Load += frmBluetoothConnect_Load;
            FormClosing += frmBluetoothConnect_FormClosing;

            ConfiguraStatusStrip();

            _logic = FrmBpmLogic.CreateDefault();
            _logic.StatusChanged += Logic_StatusChanged;
            _logic.Error += Logic_Error;
        }

        private void frmBluetoothConnect_Load(object sender, EventArgs e)
        {
            CarregarPortasBluetooth();
            AtualizarEstadoFormulario();
        }

        private void ConfiguraStatusStrip()
        {
            statusStrip1.BackColor = Color.LightGray;

            ToolStripSeparator separator1 = new ToolStripSeparator();
            ToolStripSeparator separator2 = new ToolStripSeparator();

            statusStrip1.Items.Clear();
            statusStrip1.Items.Add(sslabel1);
            statusStrip1.Items.Add(ssPorta);
            statusStrip1.Items.Add(separator1);
            statusStrip1.Items.Add(sslabel2);
            statusStrip1.Items.Add(ssDispositivo);
            statusStrip1.Items.Add(separator2);
            statusStrip1.Items.Add(sslabel3);
            statusStrip1.Items.Add(ssStatus);
        }

        private void CarregarPortasBluetooth()
        {
            cboBluetoothPortas.Items.Clear();
            cboBluetoothPortas.Items.AddRange(_logic.ListarBluetoothPortas());

            if (cboBluetoothPortas.Items.Count > 0)
                cboBluetoothPortas.SelectedIndex = 0;
        }

        private void AtualizarEstadoFormulario()
        {
            BpmStatusDto status = _logic.GetStatus();
            bool bluetoothAtivo = status.IsConnected && status.TransportKind == TransportKind.Bluetooth;
            bool outraSessaoAtiva = status.IsConnected && status.TransportKind != TransportKind.Bluetooth;

            txtDeviceName.Enabled = !status.IsConnected;
            cboBluetoothPortas.Enabled = !status.IsConnected;

            if (bluetoothAtivo)
            {
                ssPorta.Text = cboBluetoothPortas.Text;
                ssDispositivo.Text = txtDeviceName.Text;
                ssStatus.Text = "Conectado";
                ssStatus.ForeColor = Color.Green;
                btnBluetoothConnect.Enabled = true;
                btnBluetoothConnect.Text = "Desconectar";
                return;
            }

            if (outraSessaoAtiva)
            {
                ssPorta.Text = status.TransportDisplayName;
                ssDispositivo.Text = status.TransportKind.ToString();
                ssStatus.Text = "Sessao ativa via " + status.TransportKind;
                ssStatus.ForeColor = Color.DarkOrange;
                btnBluetoothConnect.Enabled = false;
                btnBluetoothConnect.Text = "Conectar";
                return;
            }

            bool temPorta = !string.IsNullOrWhiteSpace(cboBluetoothPortas.Text);
            btnBluetoothConnect.Enabled = temPorta;
            btnBluetoothConnect.Text = "Conectar";
            ssPorta.Text = "";
            ssDispositivo.Text = txtDeviceName.Text;
            ssStatus.Text = "Desconectado";
            ssStatus.ForeColor = Color.Red;
        }

        private void btnBluetoothConnect_Click(object sender, EventArgs e)
        {
            BpmStatusDto status = _logic.GetStatus();

            if (status.IsConnected && status.TransportKind != TransportKind.Bluetooth)
            {
                MessageBox.Show(
                    "Ja existe uma sessao ativa. Desconecte a interface atual antes de conectar via Bluetooth.",
                    "Conexao Bluetooth",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!status.IsConnected)
            {
                if (string.IsNullOrWhiteSpace(cboBluetoothPortas.Text))
                {
                    MessageBox.Show("Selecione a porta COM atribuida ao Bluetooth SPP.", "Conexao Bluetooth");
                    return;
                }

                bool ok = _logic.ConnectBluetooth(cboBluetoothPortas.Text, txtDeviceName.Text.Trim());
                AtualizarEstadoFormulario();

                if (!ok)
                    return;
            }
            else
            {
                _logic.Disconnect();
                AtualizarEstadoFormulario();
            }
        }

        private void cboBluetoothPortas_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarEstadoFormulario();
        }

        private void txtDeviceName_TextChanged(object sender, EventArgs e)
        {
            AtualizarEstadoFormulario();
        }

        private void Logic_StatusChanged()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(Logic_StatusChanged));
                return;
            }

            AtualizarEstadoFormulario();
        }

        private void Logic_Error(string[] msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Logic_Error(msg)));
                return;
            }

            MessageBox.Show(string.Join(Environment.NewLine, msg), "Erro Bluetooth", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void frmBluetoothConnect_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_logic != null)
            {
                _logic.StatusChanged -= Logic_StatusChanged;
                _logic.Error -= Logic_Error;
                _logic.Dispose();
                _logic = null;
            }
        }
    }
}
