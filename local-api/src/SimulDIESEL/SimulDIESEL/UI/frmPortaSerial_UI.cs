using System;
using System.Drawing;
using System.Windows.Forms;
using SimulDIESEL.BLL.FormsLogic.BPM;
using SimulDIESEL.DTL.Boards.BPM;

namespace SimulDIESEL.UI
{
    public partial class frmPortaSerial_UI : Form
    {
        private readonly ToolStripStatusLabel sslabel1 = new ToolStripStatusLabel("Porta:");
        private readonly ToolStripStatusLabel ssPorta = new ToolStripStatusLabel("");
        private readonly ToolStripStatusLabel sslabel2 = new ToolStripStatusLabel("Velocidade:");
        private readonly ToolStripStatusLabel ssVel = new ToolStripStatusLabel("");
        private readonly ToolStripStatusLabel sslabel3 = new ToolStripStatusLabel("Status:");
        private readonly ToolStripStatusLabel ssStatus = new ToolStripStatusLabel("");

        public static frmPortaSerial_UI _instance;
        private FrmBpmLogic _logic;

        public static frmPortaSerial_UI Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new frmPortaSerial_UI();
                return _instance;
            }
        }

        private frmPortaSerial_UI()
        {
            InitializeComponent();

            Load += frmConexaoSerial_Load;
            FormClosing += frmPortaSerial_UI_FormClosing;

            ConfiguraStatusStrip();

            _logic = FrmBpmLogic.CreateFromLegacyAdapter();
            _logic.StatusChanged += Logic_StatusChanged;
            _logic.Error += Logic_Error;
        }

        private void frmConexaoSerial_Load(object sender, EventArgs e)
        {
            CarregarPortas();
            AtualizarEstadoFormulario();
        }

        public void ConfiguraStatusStrip()
        {
            statusStrip1.BackColor = Color.LightGray;

            ToolStripSeparator separator1 = new ToolStripSeparator();
            ToolStripSeparator separator2 = new ToolStripSeparator();

            statusStrip1.Items.Clear();
            statusStrip1.Items.Add(sslabel1);
            statusStrip1.Items.Add(ssPorta);
            statusStrip1.Items.Add(separator1);
            statusStrip1.Items.Add(sslabel2);
            statusStrip1.Items.Add(ssVel);
            statusStrip1.Items.Add(separator2);
            statusStrip1.Items.Add(sslabel3);
            statusStrip1.Items.Add(ssStatus);
        }

        private void CarregarPortas()
        {
            cboPortas.Items.Clear();
            cboPortas.Items.AddRange(_logic.ListarPortas());

            if (cboPortas.Items.Count > 0)
                cboPortas.SelectedIndex = 0;
        }

        private void cboPortas_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarEstadoFormulario();
        }

        private void cboVelocidade_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarEstadoFormulario();
        }

        private void AtualizarEstadoFormulario()
        {
            BpmStatusDto status = _logic.GetStatus();

            if (status.IsConnected)
            {
                cboPortas.Enabled = false;
                cboVelocidade.Enabled = false;

                ssPorta.Text = cboPortas.Text;
                ssVel.Text = cboVelocidade.Text;

                btnConectar.Enabled = true;
                btnConectar.Text = "Desconectar";

                ssStatus.Text = "Conectado";
                ssStatus.ForeColor = Color.Green;
            }
            else
            {
                cboPortas.Enabled = true;
                cboVelocidade.Enabled = true;

                if (cboPortas.Items.Count == 0)
                    CarregarPortas();

                ssPorta.Text = "";
                ssVel.Text = cboVelocidade.Text;

                bool temPorta = !string.IsNullOrWhiteSpace(cboPortas.Text);
                bool baudOk = int.TryParse(cboVelocidade.Text, out _);

                btnConectar.Enabled = temPorta && baudOk;
                btnConectar.Text = "Conectar";

                ssStatus.Text = "Desconectado";
                ssStatus.ForeColor = Color.Red;
            }
        }

        private void btnConectar_Click(object sender, EventArgs e)
        {
            BpmStatusDto status = _logic.GetStatus();

            if (!status.IsConnected)
            {
                if (string.IsNullOrWhiteSpace(cboPortas.Text))
                {
                    MessageBox.Show("Selecione uma porta COM.", "Conexão Serial");
                    return;
                }

                if (!int.TryParse(cboVelocidade.Text, out int baud))
                {
                    MessageBox.Show("Velocidade (baud rate) inválida.", "Conexão Serial");
                    return;
                }

                bool ok = _logic.Connect(cboPortas.Text, baud);
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

            MessageBox.Show(string.Join(Environment.NewLine, msg), "Erro Serial", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void frmPortaSerial_UI_FormClosing(object sender, FormClosingEventArgs e)
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
