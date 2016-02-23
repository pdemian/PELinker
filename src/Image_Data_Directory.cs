using System;
using System.Linq;

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
