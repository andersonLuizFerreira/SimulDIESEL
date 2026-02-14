using System;
using System.Drawing;
using System.Windows.Forms;
using SimulDIESEL.BLL;

namespace SimulDIESEL.UI
{
    public partial class frmPortaSerial_UI : Form
    {
        // Criando os labels do StatusStrip
        ToolStripStatusLabel sslabel1 = new ToolStripStatusLabel("Porta:");
        ToolStripStatusLabel ssPorta = new ToolStripStatusLabel("");
        ToolStripStatusLabel sslabel2 = new ToolStripStatusLabel("Velocidade:");
        ToolStripStatusLabel ssVel = new ToolStripStatusLabel("");
        ToolStripStatusLabel sslabel3 = new ToolStripStatusLabel("Status:");
        ToolStripStatusLabel ssStatus = new ToolStripStatusLabel("");

        public static frmPortaSerial_UI _instance;
        

        public static frmPortaSerial_UI Instance
        {
            get
            {
                if (_instance == null || _instance.IsDisposed)
                    _instance = new frmPortaSerial_UI();
                return _instance;
            }
        }

        private SerialLinkService _serial;

        private frmPortaSerial_UI()
        {
            InitializeComponent();

            this.Load += frmConexaoSerial_Load;
            this.FormClosing += frmPortaSerial_UI_FormClosing;

            ConfiguraStatusStrip();

            // BLL (conexão apenas)
            _serial = SerialLink.Service;
            _serial.ConnectionChanged += Serial_ConnectionChanged;
            _serial.Error += Serial_Error;
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
            cboPortas.Items.AddRange(SerialLinkService.ListarPortas());

            if (cboPortas.Items.Count > 0)
                cboPortas.SelectedIndex = 0;
        }

        private void cboPortas_SelectedIndexChanged(object sender, EventArgs e) => AtualizarEstadoFormulario();
        private void cboVelocidade_SelectedIndexChanged(object sender, EventArgs e) => AtualizarEstadoFormulario();

        private void AtualizarEstadoFormulario()
        {
            bool conectado = _serial != null && _serial.IsConnected;

            if (conectado)
            {
                // Travar seleção enquanto conectado
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
                // Desconectado: permitir selecionar
                cboPortas.Enabled = true;
                cboVelocidade.Enabled = true;

                // Se não carregou portas ainda, tenta carregar
                if (cboPortas.Items.Count == 0)
                    CarregarPortas();

                ssPorta.Text = "";
                ssVel.Text = cboVelocidade.Text;

                // Só habilita conectar se tiver porta e baud válidos
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
            if (_serial == null) return;

            if (!_serial.IsConnected)
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

                // Conexão (sem tráfego). Para Arduino, você pode depois ativar dtrEnable=true.
                bool ok = _serial.Connect(cboPortas.Text, baud, dtrEnable: false, rtsEnable: false);

                // Atualiza UI mesmo se falhar (o erro vem por evento também)
                AtualizarEstadoFormulario();

                if (!ok)
                    return;
            }
            else
            {
                _serial.Disconnect();
                AtualizarEstadoFormulario();
            }
        }

        private void Serial_ConnectionChanged(bool connected)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Serial_ConnectionChanged(connected)));
                return;
            }

            AtualizarEstadoFormulario();
        }

        private void Serial_Error(string[] msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Serial_Error(msg)));
                return;
            }

            MessageBox.Show(string.Join(Environment.NewLine, msg), "Erro Serial", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void frmPortaSerial_UI_FormClosing(object sender, FormClosingEventArgs e)
        {
            // NÃO fecha a conexão automaticamente ao fechar o form.
            // O form é só um "controle" da conexão.

            if (_serial != null)
            {
                _serial.ConnectionChanged -= Serial_ConnectionChanged;
                _serial.Error -= Serial_Error;
                // _serial.Dispose();  // <-- REMOVER
                _serial = null;
            }
        }

    }
}
