using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linker
{
    internal class Image_Data_Directory
    {
        public uint VirtualAddress;
        public uint Size;

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(VirtualAddress)
                .Concat(BitConverter.GetBytes(Size))
                .ToArray();
        }
    }
}
