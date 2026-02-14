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

        /// <summary>
        /// Verdadeiro se a porta serial está aberta (transporte ativo).
        /// </summary>
        public static bool IsConnected => Service.IsConnected;

        /// <summary>
        /// Verdadeiro se o handshake foi concluído com sucesso.
        /// </summary>
        public static bool IsLinked => Service.IsLinked;

        public static string NomeDaInterface => Service.NomeDaInterface;
        /// <summary>
        /// Fecha a conexão serial (transporte).
        /// Não destrói o Service.
        /// </summary>
        public static void Close()
        {
            Service.Disconnect();
        }
    }
}
