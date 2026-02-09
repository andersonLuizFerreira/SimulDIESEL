using System;

namespace SimulDIESEL.BLL
{
    /// <summary>
    /// Ponto único de acesso ao link serial da aplicação.
    /// Mantém a conexão viva independente de forms.
    /// </summary>
    public static class SerialLink
    {
        public static SerialLinkService Service { get; } = new SerialLinkService();

        public static bool IsConnected => Service.IsConnected;

        public static void Close()
        {
            // Fecha a conexão, mas mantém o objeto vivo
            Service.Disconnect();
        }
    }
}
