using System;
using System.Linq;

namespace Linker
{
    internal class Image_File_Header
    {
        public ushort Machine = 0x014C;
        public ushort NumberOfSections = 0x03;
        public uint TimeDateStamp;
        public uint PointerToSymbolTable = 0x00;
        public uint NumberOfSymbols = 0x00;
        public ushort SizeOfOptionalHeader = 0x00E0;
        public ushort Characteristics = 0x030F;

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Machine)
                .Concat(BitConverter.GetBytes(NumberOfSections))
                .Concat(BitConverter.GetBytes(TimeDateStamp))
                .Concat(BitConverter.GetBytes(PointerToSymbolTable))
                .Concat(BitConverter.GetBytes(NumberOfSymbols))
                .Concat(BitConverter.GetBytes(SizeOfOptionalHeader))
                .Concat(BitConverter.GetBytes(Characteristics))
                .ToArray();
        }
    }
}
