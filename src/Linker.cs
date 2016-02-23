using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Linker
{
    internal class Linker
    {
        public static void WriteEXE(FileStream fs, CodeInformation code_info, bool verbose)
        {
            WriteDOSHeader(fs);
            WritePE(fs, code_info, verbose);
        }


        private static void WriteDOSHeader(FileStream fs)
        {
            //No real point in making a struct just for this, as it is ALWAYS the same
            //In fact, TinyCC,GCC,and Visual Studio all produce the exact same header. 
            //The only difference between them is the DOS code.
            byte[] DOS_Header =
            {
                0x4D, 0x5A, //'MZ'
                0x90, 0x00, //Bytes on last page of file
                0x03, 0x00, //Pages in file
                0x00, 0x00, //Relocations

                0x04, 0x00, //Size of header in paragraphs
                0x00, 0x00, //Minimum extra paragraphs needed
                0xFF, 0xFF, //Maximum extra paragraphs needed

                0x00, 0x00, //Initial SS value
                0xB8, 0x00, //Initial SP value
                0x00, 0x00, //Checksum
                0x00, 0x00, //Initial IP value
                0x00, 0x00, //Initial CS value
                0x40, 0x00, //File Address of relocation table
                0x00, 0x00, //Overlay number

                0, 0, 0, 0, //[Reserved]
                0, 0, 0, 0, //[Reserved]

                0x00, 0x00, //OEM Identifier
                0x00, 0x00, //OEM Information

                0, 0, 0, 0, 0, //[Reserved]
                0, 0, 0, 0, 0, //[Reserved]
                0, 0, 0, 0, 0, //[Reserved]
                0, 0, 0, 0, 0, //[Reserved]

                0x80, 0x00, //File Address of PE header
                0x00, 0x00 //
            };

            //Prints out "This program cannot be run in DOS mode" in DOS
            byte[] DOS_Code =
            {
                0x0e, 0x1f, 0xba, 0x0e, 0x00, 0xb4, 0x09, 0xcd,
                0x21, 0xb8, 0x01, 0x4c, 0xcd, 0x21, 0x54, 0x68,
                0x69, 0x73, 0x20, 0x70, 0x72, 0x6f, 0x67, 0x72,
                0x61, 0x6d, 0x20, 0x63, 0x61, 0x6e, 0x6e, 0x6f,
                0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6e,
                0x20, 0x69, 0x6e, 0x20, 0x44, 0x4f, 0x53, 0x20,
                0x6d, 0x6f, 0x64, 0x65, 0x2e, 0x0d, 0x0d, 0x0a,
                0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };

            //write header
            fs.Write(DOS_Header, 0, DOS_Header.Length);
            //write code
            fs.Write(DOS_Code, 0, DOS_Code.Length);
        }


        private static void WritePE(FileStream fs, CodeInformation code_info, bool verbose)
        {
            //header information
            Image_Header header = new Image_Header();
            List<List<Image_IAT_Header>> IAT_Headers = new List<List<Image_IAT_Header>>();
            List<Image_Import_Header> IIH_Headers = new List<Image_Import_Header>();

            //correct alignment if necessary
            if (code_info.FileAlignment > 0)
            {
                header.OptionalHeader.FileAlignment = code_info.FileAlignment;
            }
            if (code_info.SectionAlignment > 0)
            {
                header.OptionalHeader.SectionAlignment = code_info.SectionAlignment;
            }


            //temporary variables
            byte[] temp = null;
            byte[] temp2 = null;
            uint offset = 0;
            uint offset_calculation = 0;

            //Data Sections
            MemoryStream RData_Section = new MemoryStream();
            MemoryStream Data_Section = new MemoryStream();
            MemoryStream RData_stringTable = new MemoryStream();

            //Virtual Address locations
            uint Code_RVA = header.OptionalHeader.SectionAlignment;
            uint RData_RVA = AlignTo(Code_RVA + (uint) code_info.Code.Length, header.OptionalHeader.SectionAlignment);
            uint Data_RVA = 0;

            //Import header offsets
            uint total_IAT_offset = 0;
            uint import_header_offset = 0;

            #region imports

            if (code_info.SymbolInfo.Count > 0)
            {
                /*
                 * [IAT1]....[IAT_n]
                 * [Import_Header1][Import_Header2]...[Import_Headers_n]
                 * [IAT1]....[IAT_n]
                 * for(0...n) [0000 + function_name_1 + 00]....[0000 + function_name_n + 00]
                 */

                //Add in the placeholder headers
                foreach (var DLL in code_info.SymbolInfo)
                {
                    IIH_Headers.Add(new Image_Import_Header());
                    List<Image_IAT_Header> current_IAT =
                        DLL.Functions.Select(function => new Image_IAT_Header()).ToList();

                    IAT_Headers.Add(current_IAT);
                }

                //handle the first IAT list
                for (int i = 0; i < IIH_Headers.Count; i++)
                {
                    IIH_Headers[i].ImportAddressTableAddress = RData_RVA + total_IAT_offset;

                    for (int j = 0; j < code_info.SymbolInfo[i].Functions.Count; j++)
                    {
                        foreach (var replacement in code_info.SymbolInfo[i].Functions[j].Replacements)
                        {
                            offset_calculation = header.OptionalHeader.ImageBase + RData_RVA + total_IAT_offset;

                            code_info.Code[replacement + 0] = (byte) (offset_calculation >> 0);
                            code_info.Code[replacement + 1] = (byte) (offset_calculation >> 8);
                            code_info.Code[replacement + 2] = (byte) (offset_calculation >> 16);
                            code_info.Code[replacement + 3] = (byte) (offset_calculation >> 24);
                        }
                        total_IAT_offset += 4;
                    }

                    total_IAT_offset += 4;
                }

                header.OptionalHeader.DataDirectory[12].VirtualAddress = RData_RVA;
                header.OptionalHeader.DataDirectory[12].Size = total_IAT_offset;

                import_header_offset += RData_RVA + total_IAT_offset + ((uint) IIH_Headers.Count + 1)*0x14;

                header.OptionalHeader.DataDirectory[1].VirtualAddress = RData_RVA + total_IAT_offset;
                header.OptionalHeader.DataDirectory[1].Size = (((uint) IIH_Headers.Count) + 1)*0x14;

                //handle the second IAT list
                for (int i = 0; i < IIH_Headers.Count; i++)
                {
                    IIH_Headers[i].ImportLookUpTableAddress = import_header_offset;

                    import_header_offset += ((uint) IAT_Headers[i].Count + 1)*4;
                }


                //handle function names and what not
                for (int i = 0; i < IAT_Headers.Count; i++)
                {
                    for (int j = 0; j < IAT_Headers[i].Count; j++)
                    {
                        var func = code_info.SymbolInfo[i].Functions[j];

                        IAT_Headers[i][j].Address = import_header_offset;

                        //write ordinal + function name + null
                        temp = BitConverter.GetBytes((short) func.Ordinal);
                        RData_stringTable.Write(temp, 0, temp.Length);
                        temp = Encoding.UTF8.GetBytes(func.FunctionName);
                        RData_stringTable.Write(temp, 0, temp.Length);
                        RData_stringTable.WriteByte(0);

                        import_header_offset += 2 + (uint) temp.Length + 1;
                    }

                    IIH_Headers[i].NameAddress = import_header_offset;

                    temp = Encoding.UTF8.GetBytes(code_info.SymbolInfo[i].LibraryName);

                    RData_stringTable.Write(temp, 0, temp.Length);
                    RData_stringTable.WriteByte(0);

                    import_header_offset += (uint) temp.Length + 1;
                }

                foreach (var IAT in IAT_Headers)
                {
                    IAT.Add(new Image_IAT_Header());
                }
                //add in the null header to signify the end of the list
                IIH_Headers.Add(new Image_Import_Header());

                //this is awful and it works
                //convert all headers into their byte equivalent, then concatenate them all together
                temp = IAT_Headers
                    .Select(x => x
                        .Select(y => y.ToBytes())
                        .Aggregate((a, b) => a
                            .Concat(b)
                            .ToArray()))
                    .Aggregate((a, b) => a
                        .Concat(b)
                        .ToArray());

                temp2 = IIH_Headers
                    .Select(x => x.ToBytes())
                    .Aggregate((a, b) => a.Concat(b).ToArray());

                RData_Section.Write(temp, 0, temp.Length);
                RData_Section.Write(temp2, 0, temp2.Length);
                RData_Section.Write(temp, 0, temp.Length);

                //CopyTo doesn't seem to work 100% of the time. Maybe a bug with .NET
                //RData_stringTable.CopyTo(RData_Section);
                temp = RData_stringTable.GetBuffer();
                RData_Section.Write(temp, 0, (int) RData_stringTable.Length);
            }

            #endregion

            #region Data Table

            //Correct Data RVA
            Data_RVA = AlignTo((uint)(RData_RVA + RData_Section.Length), header.OptionalHeader.SectionAlignment);

            //string table replacements
            offset = 0;
            for (int i = 0; i < code_info.StringTable.Count; i++)
            {
                temp = Encoding.UTF8.GetBytes(code_info.StringTable[i].Text);

                foreach (int replacement in code_info.StringTable[i].Replacements)
                {
                    offset_calculation = header.OptionalHeader.ImageBase + Data_RVA + offset;

                    code_info.Code[replacement + 0] = (byte) (offset_calculation >> 0);
                    code_info.Code[replacement + 1] = (byte) (offset_calculation >> 8);
                    code_info.Code[replacement + 2] = (byte) (offset_calculation >> 16);
                    code_info.Code[replacement + 3] = (byte) (offset_calculation >> 24);
                }

                Data_Section.Write(temp, 0, temp.Length);

                Data_Section.WriteByte(0);

                offset += (uint) (temp.LongLength + 1);
            }

            #endregion

            #region Header

            //I don't want to hard code this as 0x170 because I might (maybe) add in more sections, and forget to change this
            //though at the moment with what's coded already, we're wasting a huge amount of time calculating this
            temp = header.ToBytes();
            uint raw_header_length = (uint) temp.LongLength;
            uint header_length = AlignTo(raw_header_length, header.OptionalHeader.FileAlignment);


            //.code section
            header.Text_Section.VirtualSize = (uint) code_info.Code.LongLength;
            header.Text_Section.SizeOfRawData = AlignTo((uint) code_info.Code.LongLength,
                header.OptionalHeader.FileAlignment);

            header.Text_Section.VirtualAddress = Code_RVA;
            header.Text_Section.PointerToRawData = header_length;

            //.rdata section
            header.RData_Section.VirtualSize = (uint) RData_Section.Length + 1;
            header.RData_Section.SizeOfRawData = AlignTo((uint) RData_Section.Length,
                header.OptionalHeader.FileAlignment);

            header.RData_Section.VirtualAddress = RData_RVA;
            header.RData_Section.PointerToRawData = header_length + header.Text_Section.SizeOfRawData;

            //.data section
            header.Data_Section.VirtualSize = (uint) Data_Section.Length;
            header.Data_Section.SizeOfRawData = AlignTo((uint) Data_Section.Length, header.OptionalHeader.FileAlignment);

            header.Data_Section.VirtualAddress = Data_RVA;
            header.Data_Section.PointerToRawData = header.RData_Section.PointerToRawData +
                                                   header.RData_Section.SizeOfRawData;


            header.OptionalHeader.Subsystem = code_info.Subsystem;
            header.OptionalHeader.AddressOfEntryPoint = code_info.EntryPoint + Code_RVA;
            header.OptionalHeader.BaseOfCode = Code_RVA;
            header.OptionalHeader.BaseOfData = RData_RVA;

            header.OptionalHeader.SizeOfHeaders =
                AlignTo((uint) RData_Section.Length, header.OptionalHeader.FileAlignment) +
                AlignTo((uint) Data_Section.Length, header.OptionalHeader.FileAlignment);
            header.OptionalHeader.SizeOfInitializedData = header.OptionalHeader.SizeOfHeaders;

            header.OptionalHeader.SizeOfImage = AlignTo(header_length, header.OptionalHeader.SectionAlignment)
                                                +
                                                AlignTo(header.Text_Section.SizeOfRawData,
                                                    header.OptionalHeader.SectionAlignment)
                                                +
                                                AlignTo(header.RData_Section.SizeOfRawData,
                                                    header.OptionalHeader.SectionAlignment)
                                                +
                                                AlignTo(header.Data_Section.SizeOfRawData,
                                                    header.OptionalHeader.SectionAlignment);

            header.OptionalHeader.SizeOfCode = AlignTo((uint) code_info.Code.LongLength,
                header.OptionalHeader.FileAlignment);

            header.FileHeader.TimeDateStamp = (uint) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            #endregion

            #region Write Executable

            //finally write the file

            //write PE header
            temp = header.ToBytes();
            fs.Write(temp, 0, temp.Length);
            //write Header padding
            temp = CreatePadding((uint) fs.Position, header.OptionalHeader.FileAlignment);
            fs.Write(temp, 0, temp.Length);

            //write .code section
            fs.Write(code_info.Code, 0, code_info.Code.Length);
            //write .code padding
            temp = CreatePadding((uint) code_info.Code.Length, header.OptionalHeader.FileAlignment);
            fs.Write(temp, 0, temp.Length);

            //write .rdata section
            temp = RData_Section.GetBuffer();
            fs.Write(temp, 0, (int) RData_Section.Length);
            //write .rdata padding
            temp = CreatePadding((uint) RData_Section.Length, header.OptionalHeader.FileAlignment);
            fs.Write(temp, 0, temp.Length);

            //write .data section 
            temp = Data_Section.GetBuffer();
            fs.Write(temp, 0, (int) Data_Section.Length);
            //write .data padding
            temp = CreatePadding((uint) Data_Section.Length, header.OptionalHeader.FileAlignment);
            fs.Write(temp, 0, temp.Length);

            #endregion
        }

        private static byte[] CreatePadding(uint value, uint alignment)
        {
            return new byte[(alignment - value%alignment)];
        }

        private static uint AlignTo(uint value, uint alignment)
        {
            return value + (alignment - value%alignment);
        }
    }
}