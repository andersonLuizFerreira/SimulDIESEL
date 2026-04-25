using System;
using SimulDIESEL.BLL.Boards.GSA;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards
{
    public interface IBoardDispatcher
    {
        IUceDispatcher Uce { get; }
        IGsaDispatcher Gsa { get; }
        object Resolve(byte boardAddress);
    }

    public sealed class BoardDispatcher : IBoardDispatcher
    {
        public BoardDispatcher(IUceDispatcher uce, IGsaDispatcher gsa)
        {
            Uce = uce ?? throw new ArgumentNullException(nameof(uce));
            Gsa = gsa ?? throw new ArgumentNullException(nameof(gsa));
        }

        public IUceDispatcher Uce { get; private set; }
        public IGsaDispatcher Gsa { get; private set; }

        public object Resolve(byte boardAddress)
        {
            switch (boardAddress)
            {
                case GwProtocol.UceAddress:
                    return Uce;
                case GwProtocol.GsaAddress:
                    return Gsa;
                default:
                    throw new NotSupportedException("Board não suportada pelo dispatcher: 0x" + boardAddress.ToString("X1") + ".");
            }
        }
    }
}
