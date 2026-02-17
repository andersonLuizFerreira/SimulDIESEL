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

            SerialLink.Service.ConnectionChanged -= Serial_ConnectionChanged;
            SerialLink.Service.ConnectionChanged += Serial_ConnectionChanged;

            SerialLink.Service.LinkStateChanged -= Link_StateChanged;
            SerialLink.Service.LinkStateChanged += Link_StateChanged;

            SerialLink.Service.Error -= Link_Error;
            SerialLink.Service.Error += Link_Error;

            SerialLink.Service.NomeDaInterfaceChanged -= NomeDaInterfaceChanged_Handler;
            SerialLink.Service.NomeDaInterfaceChanged += NomeDaInterfaceChanged_Handler;

            AtualizarBotaoConectar();
            AtualizarIndicadores(); // (7) garante estado inicial coerente
            NomeDaInterfaceChanged_Handler(); // (7) força refletir "Nenhum" na partida
        }

        private void toolStripConectar_Click(object sender, EventArgs e)
        {
            if (SerialLink.IsConnected)
            {
                SerialLink.Close();
                AtualizarBotaoConectar();
                AtualizarIndicadores(); // (7)
                NomeDaInterfaceChanged_Handler(); // (7)
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
            toolStripConectar.Text = SerialLink.IsConnected ? "Desconectar" : "Conectar";

            toolStripConectar.Image = SerialLink.IsConnected
                ? Properties.Resources.Conectado
                : Properties.Resources.Desconectado;
        }

        private void AtualizarIndicadores()
        {
            bool serialOk = SerialLink.IsConnected;
            var state = SerialLink.Service.State;

            // SERIAL
            tsLedSerial.Image = serialOk
                ? Properties.Resources.LedGreenBright_18x18
                : Properties.Resources.LedRedDark_18x18;

            tsLabelSerial.Text = "Status da Serial: " + (serialOk ? "Conectado" : "Desconectado");

            // LINK
            if (!serialOk)
            {
                // (7) quando a serial cai, garante link em cinza e nome coerente
                tsLedLink.Image = Properties.Resources.LedGrayOff_18x18;
                tsNomeInterface.Text = "Nenhum";
                return;
            }

            switch (state)
            {
                case SerialLinkService.LinkState.Linked:
                    tsLedLink.Image = Properties.Resources.LedGreenBright_18x18;
                    break;

                case SerialLinkService.LinkState.LinkFailed:
                    tsLedLink.Image = Properties.Resources.LedRedBright_18x18;
                    break;

                case SerialLinkService.LinkState.Draining:
                case SerialLinkService.LinkState.BannerSent:
                case SerialLinkService.LinkState.SerialConnected:
                    tsLedLink.Image = Properties.Resources.LedYellowBright_18x18;
                    break;

                default:
                    tsLedLink.Image = Properties.Resources.LedGrayOff_18x18;
                    break;
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
            NomeDaInterfaceChanged_Handler(); // (7) reforça UI em queda/subida
        }

        private void NomeDaInterfaceChanged_Handler()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(NomeDaInterfaceChanged_Handler));
                return;
            }

            tsNomeInterface.Text = SerialLink.Service.IsLinked
                ? SerialLink.Service.NomeDaInterface
                : "Nenhum";
        }

        private void Link_StateChanged(SerialLinkService.LinkState state)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Link_StateChanged(state)));
                return;
            }

            AtualizarIndicadores();
            NomeDaInterfaceChanged_Handler(); // (7) se cair de Linked -> atualiza nome
        }

        private void Link_Error(string[] msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Link_Error(msg)));
                return;
            }

            // opcional: log
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SerialLink.Service.ConnectionChanged -= Serial_ConnectionChanged;
            SerialLink.Service.LinkStateChanged -= Link_StateChanged;
            SerialLink.Service.NomeDaInterfaceChanged -= NomeDaInterfaceChanged_Handler;
            SerialLink.Service.Error -= Link_Error;

            base.OnFormClosing(e);
        }

        private void mnuFerramentasLed_Click(object sender, EventArgs e)
        {
            var frmLED = frmLedGw.Instance;

            if (frmLED.Visible)
            {
                frmLED.BringToFront();
                frmLED.Activate();
                return;
            }

            frmLED.MdiParent = this;
            frmLED.StartPosition = FormStartPosition.CenterParent;
            frmLED.Show();
        }
    }
}
