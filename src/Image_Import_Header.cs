using System;
using System.Linq;

namespace Linker
{
    internal class Image_Import_Header
    {
        public uint ImportLookUpTableAddress;
        public uint TimeDateStamp = 0;
        public uint ForwarderChain = 0;
        public uint NameAddress;
        public uint ImportAddressTableAddress;

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(ImportLookUpTableAddress)
                .Concat(BitConverter.GetBytes(TimeDateStamp))
                .Concat(BitConverter.GetBytes(ForwarderChain))
                .Concat(BitConverter.GetBytes(NameAddress))
                .Concat(BitConverter.GetBytes(ImportAddressTableAddress))
                .ToArray();
        }
    }
}
