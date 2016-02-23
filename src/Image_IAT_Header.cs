using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linker
{
    internal class Image_IAT_Header
    {
        public uint Address;

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Address);
        }
    }
}
