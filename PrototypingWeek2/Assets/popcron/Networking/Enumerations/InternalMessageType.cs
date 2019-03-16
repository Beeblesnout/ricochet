using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popcron.Networking
{
    public enum InternalMessageType : ushort
    {
        AcknowledgeRequest = 32767,
        AcknowledgeResponse = 32768,
        ConnectionRemoveRequest = 32769,
        ConnectionAddRequest = 32770
    }
}
