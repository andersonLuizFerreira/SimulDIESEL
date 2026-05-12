using System;
using System.Windows.Forms;
using SimulDIESEL.BLL.Services.Database;

namespace SimulDIESEL
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            new LocalDatabaseService().Initialize();
            Application.Run(new DashBoard());
        }
    }
}
