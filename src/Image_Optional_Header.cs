using System;
using System.Linq;

namespace Linker
{
    internal class Image_Optional_Header
    {
        public ushort Magic = 0x010B;
        public byte MajorLinkerVersion = 0x01;
        public byte MinorLinkerVersion = 0x00;
        public uint SizeOfCode;
        public uint SizeOfInitializedData;
        public uint SizeOfUninitializedData = 0x00;
        public uint AddressOfEntryPoint = 0x1000;
        public uint BaseOfCode = 0x1000;
        public uint BaseOfData = 0x2000;
        public uint ImageBase = 0x00400000;
        public uint SectionAlignment = 0x00001000;
        public uint FileAlignment = 0x00000200;

        public ushort MajorOperatingSystemVersion = 0x0004;
        public ushort MinorOperatingSystemVersion = 0x0000;
        public ushort MajorImageVersion = 0x00;
        public ushort MinorImageVersion = 0x00;
        public ushort MajorSubsystemVersion = 0x0004;
        public ushort MinorSubsystemVersion = 0x0000;
        public uint Win32VersionValue = 0x00;
        public uint SizeOfImage;
        public uint SizeOfHeaders = 0x00000200;
        public uint CheckSum = 0x00;

        public ushort Subsystem = 0x0003;
        public ushort DllCharacteristics = 0x00;
        public uint SizeOfStackReserve = 0x00100000;
        public uint SizeOfStackCommit = 0x00001000;
        public uint SizeOfHeapReserve = 0x00100000;
        public uint SizeOfHeapCommit = 0x00001000;
        public uint LoaderFlags = 0x00000000;
        public uint NumberOfRvaAndSizes = 0x00000010;

        public Image_Data_Directory[] DataDirectory;

        public Image_Optional_Header()
        {
            DataDirectory = new Image_Data_Directory[NumberOfRvaAndSizes];
            for (int i = 0; i < NumberOfRvaAndSizes; i++)
            {
                DataDirectory[i] = new Image_Data_Directory();
            }
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Magic)
                .Concat(new[] { MajorLinkerVersion, MinorLinkerVersion })
                .Concat(BitConverter.GetBytes(SizeOfCode))
                .Concat(BitConverter.GetBytes(SizeOfInitializedData))
                .Concat(BitConverter.GetBytes(SizeOfUninitializedData))
                .Concat(BitConverter.GetBytes(AddressOfEntryPoint))
                .Concat(BitConverter.GetBytes(BaseOfCode))
                .Concat(BitConverter.GetBytes(BaseOfData))
                .Concat(BitConverter.GetBytes(ImageBase))
                .Concat(BitConverter.GetBytes(SectionAlignment))
                .Concat(BitConverter.GetBytes(FileAlignment))
                .Concat(BitConverter.GetBytes(MajorOperatingSystemVersion))
                .Concat(BitConverter.GetBytes(MinorOperatingSystemVersion))
                .Concat(BitConverter.GetBytes(MajorImageVersion))
                .Concat(BitConverter.GetBytes(MinorImageVersion))
                .Concat(BitConverter.GetBytes(MajorSubsystemVersion))
                .Concat(BitConverter.GetBytes(MinorSubsystemVersion))
                .Concat(BitConverter.GetBytes(Win32VersionValue))
                .Concat(BitConverter.GetBytes(SizeOfImage))
                .Concat(BitConverter.GetBytes(SizeOfHeaders))
                .Concat(BitConverter.GetBytes(CheckSum))
                .Concat(BitConverter.GetBytes(Subsystem))
                .Concat(BitConverter.GetBytes(DllCharacteristics))
                .Concat(BitConverter.GetBytes(SizeOfStackReserve))
                .Concat(BitConverter.GetBytes(SizeOfStackCommit))
                .Concat(BitConverter.GetBytes(SizeOfHeapReserve))
                .Concat(BitConverter.GetBytes(SizeOfHeapCommit))
                .Concat(BitConverter.GetBytes(LoaderFlags))
                .Concat(BitConverter.GetBytes(NumberOfRvaAndSizes))
                .Concat(DataDirectory
                    .Select(x => x.ToBytes())
                    .Aggregate((i, j) => i.Concat(j)
                        .ToArray()))
                .ToArray();
        }
    }
}
