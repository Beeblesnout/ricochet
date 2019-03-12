using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Popcron.Networking
{
    public class NoReadAccessException : Exception
    {
        public NoReadAccessException(string message) : base(message)
        {
        }
    }
}
