using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linker
{
    internal class Image_Section_Header
    {
        public byte[] name;
        public uint VirtualSize;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint Characteristics;

        public byte[] ToBytes()
        {
            if (name.Length > 8)
            {
                name = name.Take(8).ToArray();
            }

            return name
                .Concat(CreatePadding(8 - name.Length))
                .Concat(BitConverter.GetBytes(VirtualSize))
                .Concat(BitConverter.GetBytes(VirtualAddress))
                .Concat(BitConverter.GetBytes(SizeOfRawData))
                .Concat(BitConverter.GetBytes(PointerToRawData))
                .Concat(CreatePadding(12))
                .Concat(BitConverter.GetBytes(Characteristics))
                .ToArray();
        }

        private static byte[] CreatePadding(int size)
        {
            return new byte[size];
        }
    }
}
