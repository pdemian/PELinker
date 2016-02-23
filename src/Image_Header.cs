using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linker
{
    internal class Image_Header
    {
        public uint Signature = 0x004550;
        public Image_File_Header FileHeader;
        public Image_Optional_Header OptionalHeader;

        //Hard coded at the moment, but should be a list of sections
        public Image_Section_Header Text_Section;
        public Image_Section_Header RData_Section;
        public Image_Section_Header Data_Section;

        public Image_Header()
        {
            FileHeader = new Image_File_Header();
            OptionalHeader = new Image_Optional_Header();


            Text_Section = new Image_Section_Header
            {
                name = Encoding.ASCII.GetBytes(".text"),
                VirtualSize = OptionalHeader.SectionAlignment,
                SizeOfRawData = OptionalHeader.FileAlignment,
                Characteristics = 0x60000020,
                PointerToRawData = 1 * OptionalHeader.FileAlignment,
                VirtualAddress = 1 * OptionalHeader.SectionAlignment
            };

            RData_Section = new Image_Section_Header
            {
                name = Encoding.ASCII.GetBytes(".rdata"),
                VirtualSize = OptionalHeader.SectionAlignment,
                SizeOfRawData = OptionalHeader.FileAlignment,
                Characteristics = 0x40000040,
                PointerToRawData = 2 * OptionalHeader.FileAlignment,
                VirtualAddress = 2 * OptionalHeader.SectionAlignment
            };

            Data_Section = new Image_Section_Header
            {
                name = Encoding.ASCII.GetBytes(".data"),
                VirtualSize = OptionalHeader.SectionAlignment,
                SizeOfRawData = OptionalHeader.FileAlignment,
                Characteristics = 0xC0000040,
                PointerToRawData = 3 * OptionalHeader.FileAlignment,
                VirtualAddress = 3 * OptionalHeader.SectionAlignment
            };
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Signature)
                .Concat(FileHeader.ToBytes())
                .Concat(OptionalHeader.ToBytes())
                .Concat(Text_Section.ToBytes())
                .Concat(RData_Section.ToBytes())
                .Concat(Data_Section.ToBytes())
                .ToArray();
        }
    }
}
