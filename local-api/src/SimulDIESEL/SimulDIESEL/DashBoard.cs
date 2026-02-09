using SimulDIESEL.BLL;
using SimulDIESEL.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimulDIESEL.BLL;

namespace SimulDIESEL
{
    public partial class DashBoard : Form
    {
        public DashBoard()
        {
            InitializeComponent();

            SerialLink.Service.ConnectionChanged -= Serial_ConnectionChanged; // garante que não tem mais de um handler (se o form for reaberto)
            SerialLink.Service.ConnectionChanged += Serial_ConnectionChanged; // se a conexão mudar, atualiza o botão e status
            AtualizarBotaoConectar(); // estado inicial
        }



        private void toolStripConectar_Click(object sender, EventArgs e)
        {
            // Se já está conectado: fecha a conexão SEM abrir o form
            if (SerialLink.IsConnected)
            {
                SerialLink.Close();
                AtualizarBotaoConectar(); // atualiza texto/ícone
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
            toolStripStatusLINK.Text = SerialLink.IsConnected ? "Conectado" : "Desconectado";

            toolStripConectar.Image = SerialLink.IsConnected
            ? Properties.Resources.Conectado
            : Properties.Resources.Desconectado;
        }

        private void Serial_ConnectionChanged(bool connected)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => Serial_ConnectionChanged(connected)));
                return;
            }

            Console.WriteLine($" DashBoard. Serial_ConnectionChanged Invoked. Connected={connected}");
            // Aqui já está na thread da UI
            AtualizarBotaoConectar();

            // Se você tiver StatusStrip/Label/ícone:
            // lblStatus.Text = connected ? "Conectado" : "Desconectado";
            // lblStatus.ForeColor = connected ? Color.Green : Color.Red;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SerialLink.Service.ConnectionChanged -= Serial_ConnectionChanged;
            base.OnFormClosing(e);
        }

    }
}
