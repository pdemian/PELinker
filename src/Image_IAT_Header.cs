using System;

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
