namespace SimulDIESEL.BLL
{
    /// <summary>
    /// Adapter global legado e transitório.
    /// Mantido apenas para compatibilidade enquanto a UI migra gradualmente para
    /// FormsLogic/BPM board clients como ponto principal de composição.
    /// </summary>
    public static class SerialLink
    {
        public static SerialLinkService Service { get; } = new SerialLinkService();

        public static bool IsConnected => Service.IsConnected;
        public static bool IsLinked => Service.IsLinked;
        public static string NomeDaInterface => Service.NomeDaInterface;

        public static void Close()
        {
            Service.Disconnect();
        }
    }
}
