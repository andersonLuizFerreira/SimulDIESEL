using SimulDIESEL.BLL;
using SimulDIESEL.UI;
using System;
using System.Windows.Forms;

namespace SimulDIESEL
{
    public partial class DashBoard : Form
    {
        public DashBoard()
        {
            InitializeComponent();

            // SERIAL
            SerialLink.Service.ConnectionChanged -= Serial_ConnectionChanged;
            SerialLink.Service.ConnectionChanged += Serial_ConnectionChanged;

            // LINK
            SerialLink.Service.LinkStateChanged -= Link_StateChanged;
            SerialLink.Service.LinkStateChanged += Link_StateChanged;

            // ERRO (opcional, mas recomendado)
            SerialLink.Service.Error -= Link_Error;
            SerialLink.Service.Error += Link_Error;


            AtualizarBotaoConectar();
            AtualizarStatusStrip();
        }

        private void toolStripConectar_Click(object sender, EventArgs e)
        {
            // Se já está conectado: fecha a conexão SEM abrir o form
            if (SerialLink.IsConnected)
            {
                SerialLink.Close();
                // Os eventos vão atualizar, mas atualizar aqui ajuda a dar resposta imediata
                AtualizarBotaoConectar();
                AtualizarStatusStrip();
                return;
            }

            // Se não está conectado: abre o form de conexão
            var frmConexaoSerial = frmPortaSerial_UI.Instance;

            // Se já estiver aberto, só traz pra frente
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
            toolStripConectar.Text = SerialLink.IsConnected ? "Desconectar" : "Conectar";

            toolStripConectar.Image = SerialLink.IsConnected
                ? Properties.Resources.Conectado
                : Properties.Resources.Desconectado;
        }

        private void AtualizarIndicadores()
        {
            bool serialOk = SerialLink.IsConnected;
            var state = SerialLink.Service.State;

            // ==========================
            // SERIAL
            // ==========================
            tsLedSerial.Image = serialOk
                ? Properties.Resources.LedGreenBright_18x18
                : Properties.Resources.LedRedDark_18x18;

            tsLabelSerial.Text = "Status da Serial: " + (serialOk ? "Conectado" : "Desconectado");

            // ==========================
            // LINK
            // ==========================
            if (!serialOk)
            {
                tsLedLink.Image = Properties.Resources.LedGrayOff_18x18;
                //tsLabelLink.Text = "Status do Link: Desconectado";
                return;
            }

            switch (state)
            {
                case SerialLinkService.LinkState.Linked:
                    tsLedLink.Image = Properties.Resources.LedGreenBright_18x18;
                    //tsLabelLink.Text = "Status do Link: Linked";
                    break;

                case SerialLinkService.LinkState.LinkFailed:
                    tsLedLink.Image = Properties.Resources.LedRedBright_18x18;
                    //tsLabelLink.Text = "Status do Link: Falhou (retry 3s)";
                    break;

                case SerialLinkService.LinkState.Draining:
                case SerialLinkService.LinkState.BannerSent:
                case SerialLinkService.LinkState.SerialConnected:
                    tsLedLink.Image = Properties.Resources.LedYellowBright_18x18;
                    //tsLabelLink.Text = "Status do Link: Conectando...";
                    break;

                default:
                    tsLedLink.Image = Properties.Resources.LedGrayOff_18x18;
                    //tsLabelLink.Text = "Status do Link: " + state.ToString();
                    break;
            }
        }


        private void AtualizarStatusStrip()
        {
            // IMPORTANTE: no seu Designer:
            // toolStripStatusLINK = valor do Status da Serial
            // toolStripStatusLabel4 = valor do Status do Link

            // Status da Serial (transporte)
            //tsLabelSerialValue.Text = SerialLink.IsConnected ? "Conectado" : "Desconectado";

            // Status do Link (handshake)
            if (!SerialLink.IsConnected)
            {
                //tsLabelLink.Text = "Desconectado";
            }
            else
            {
                // Você pode escolher:
                // 1) mostrar simples:
                //toolStripStatusLabel4.Text = SerialLink.IsLinked ? "Linked" : "Aguardando link";
                //tsLabelLink.Text = SerialLink.Service.State.ToString();


                // 2) OU mostrar estado detalhado:
                // toolStripStatusLabel4.Text = SerialLink.Service.State.ToString();
            }
        }

        private void Serial_ConnectionChanged(bool connected)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Serial_ConnectionChanged(connected)));
                return;
            }

            AtualizarBotaoConectar();
            AtualizarIndicadores();
        }

        private void Link_StateChanged(SerialLinkService.LinkState state)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Link_StateChanged(state)));
                return;
            }

            AtualizarIndicadores();
        }

        private void Link_Error(string[] msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Link_Error(msg)));
                return;
            }

            // Se quiser, pode registrar em console ou colocar em um ToolStripStatusLabel extra
            // Console.WriteLine($"ERROR: {string.Join(" | ", msg ?? Array.Empty<string>())}");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SerialLink.Service.ConnectionChanged -= Serial_ConnectionChanged;
            SerialLink.Service.LinkStateChanged -= Link_StateChanged;
            SerialLink.Service.Error -= Link_Error;

            base.OnFormClosing(e);
        }
    }
}
