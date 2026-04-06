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
        private readonly ToolStripStatusLabel sslabel1 = new ToolStripStatusLabel("Selecionado:");
        private readonly ToolStripStatusLabel ssSelecionado = new ToolStripStatusLabel("");
        private readonly ToolStripStatusLabel sslabel2 = new ToolStripStatusLabel("Porta:");
        private readonly ToolStripStatusLabel ssPorta = new ToolStripStatusLabel("");
        private readonly ToolStripStatusLabel sslabel3 = new ToolStripStatusLabel("Status:");
        private readonly ToolStripStatusLabel ssStatus = new ToolStripStatusLabel("");

        private static frmBluetoothConnect _instance;
        private FrmBpmLogic _logic;
        private BluetoothDeviceDto[] _devices = Array.Empty<BluetoothDeviceDto>();

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
            CarregarDispositivosBluetooth();
            AtualizarEstadoFormulario();
        }

        private void ConfiguraStatusStrip()
        {
            statusStrip1.BackColor = Color.LightGray;

            ToolStripSeparator separator1 = new ToolStripSeparator();
            ToolStripSeparator separator2 = new ToolStripSeparator();

            statusStrip1.Items.Clear();
            statusStrip1.Items.Add(sslabel1);
            statusStrip1.Items.Add(ssSelecionado);
            statusStrip1.Items.Add(separator1);
            statusStrip1.Items.Add(sslabel2);
            statusStrip1.Items.Add(ssPorta);
            statusStrip1.Items.Add(separator2);
            statusStrip1.Items.Add(sslabel3);
            statusStrip1.Items.Add(ssStatus);
        }

        private void CarregarDispositivosBluetooth()
        {
            lvBluetoothDevices.Items.Clear();
            _devices = _logic.ListarBluetoothDispositivos() ?? Array.Empty<BluetoothDeviceDto>();

            foreach (BluetoothDeviceDto device in _devices)
            {
                ListViewItem item = new ListViewItem(device.DisplayName);
                item.SubItems.Add(string.IsNullOrWhiteSpace(device.Address) ? "Nao informado" : device.Address);
                item.SubItems.Add(string.IsNullOrWhiteSpace(device.PortName) ? "-" : device.PortName);
                item.SubItems.Add(device.StatusText);
                item.Tag = device;
                lvBluetoothDevices.Items.Add(item);
            }

            if (lvBluetoothDevices.Items.Count > 0)
                lvBluetoothDevices.Items[0].Selected = true;
        }

        private void AtualizarEstadoFormulario()
        {
            BpmStatusDto status = _logic.GetStatus();
            bool bluetoothAtivo = status.IsConnected && status.TransportKind == TransportKind.Bluetooth;
            bool outraSessaoAtiva = status.IsConnected && status.TransportKind != TransportKind.Bluetooth;
            BluetoothDeviceDto selectedDevice = GetSelectedDevice();

            lvBluetoothDevices.Enabled = !status.IsConnected;
            btnAtualizar.Enabled = !status.IsConnected;

            if (bluetoothAtivo)
            {
                ssSelecionado.Text = status.TransportDisplayName;
                ssPorta.Text = status.TransportDisplayName;
                ssStatus.Text = "Conectado";
                ssStatus.ForeColor = Color.Green;
                btnConectar.Enabled = true;
                btnConectar.Text = "Desconectar";
                return;
            }

            if (outraSessaoAtiva)
            {
                ssPorta.Text = status.TransportDisplayName;
                ssSelecionado.Text = status.TransportKind.ToString();
                ssStatus.Text = "Sessao ativa via " + status.TransportKind;
                ssStatus.ForeColor = Color.DarkOrange;
                btnConectar.Enabled = false;
                btnConectar.Text = "Conectar";
                return;
            }

            bool temSelecionado = selectedDevice != null && selectedDevice.IsAvailable;
            btnConectar.Enabled = temSelecionado;
            btnConectar.Text = "Conectar";
            ssSelecionado.Text = selectedDevice != null ? selectedDevice.DisplayName : "Nenhum";
            ssPorta.Text = selectedDevice != null && !string.IsNullOrWhiteSpace(selectedDevice.PortName)
                ? selectedDevice.PortName
                : "-";
            ssStatus.Text = BuildIdleStatusText(selectedDevice);
            ssStatus.ForeColor = Color.Red;
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            BpmStatusDto status = _logic.GetStatus();
            BluetoothDeviceDto selectedDevice = GetSelectedDevice();

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
                if (selectedDevice == null)
                {
                    MessageBox.Show("Selecione um dispositivo Bluetooth.", "Conexao Bluetooth");
                    return;
                }

                if (!selectedDevice.IsAvailable)
                {
                    MessageBox.Show(
                        "O dispositivo selecionado nao possui uma porta COM SPP utilizavel no Windows. Pareie o dispositivo e crie a porta SPP antes de conectar.",
                        "Conexao Bluetooth",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                bool ok = _logic.ConnectBluetooth(selectedDevice);
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

        private void btnAtualizar_Click(object sender, EventArgs e)
        {
            CarregarDispositivosBluetooth();
            AtualizarEstadoFormulario();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lvBluetoothDevices_SelectedIndexChanged(object sender, EventArgs e)
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

        private BluetoothDeviceDto GetSelectedDevice()
        {
            if (lvBluetoothDevices.SelectedItems.Count == 0)
                return null;

            return lvBluetoothDevices.SelectedItems[0].Tag as BluetoothDeviceDto;
        }

        private static string BuildIdleStatusText(BluetoothDeviceDto device)
        {
            if (device == null)
                return "Nenhum dispositivo encontrado";

            if (device.IsAvailable)
                return "Pronto para conectar";

            if (string.IsNullOrWhiteSpace(device.StatusText))
                return "Dispositivo indisponivel";

            return device.StatusText;
        }
    }
}
