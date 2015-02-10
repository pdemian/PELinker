using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Compiler
{
    class Linker
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
        internal class Image_File_Header
        {
            public ushort Machine = 0x014C;
            public ushort NumberOfSections = 0x03;
            public uint TimeDateStamp = 0x00;
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
        internal class Image_Optional_Header
        {
            public ushort Magic = 0x010B;
            public byte MajorLinkerVersion = 0x01;
            public byte MinorLinkerVersion = 0x00;
            public uint SizeOfCode = 0x00;
            public uint SizeOfInitializedData = 0x00;
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
            public uint SizeOfImage = 0x00;
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
                    DataDirectory[i] = new Image_Data_Directory();
            }

            public byte[] ToBytes()
            {
                DataDirectory.Select(x => x.ToBytes()).Aggregate((i, j) => i.Concat(j).ToArray());


                return BitConverter.GetBytes(Magic)
                    .Concat(new byte[] { MajorLinkerVersion, MinorLinkerVersion })
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
        internal class Image_Data_Directory
        {
            public uint VirtualAddress = 0;
            public uint Size = 0;

            public byte[] ToBytes()
            {
                return BitConverter.GetBytes(VirtualAddress)
                    .Concat(BitConverter.GetBytes(Size))
                    .ToArray();
            }
        }
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
                if (name.Length > 8) name = name.Take(8).ToArray();

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
        }
        internal class Image_IAT_Header
        {
            public uint Address;

            public byte[] ToBytes()
            {
                return BitConverter.GetBytes(Address);
            }
        }
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

        public static void WriteEXE(FileStream fs, CodeInformation code_info)
        {
            WriteDOSHeader(fs);
            WritePE(fs, code_info);
        }


        private static void WriteDOSHeader(FileStream fs)
        {
            //No real point in making a struct just for this, as it is ALWAYS the same
            //In fact, TinyCC,GCC,and Visual Studio all produce the exact same header. 
            //The only difference between them is the DOS code.
            byte[] DOS_Header = 
            {
                0x4D,0x5A, //'MZ'
                0x90,0x00, //Bytes on last page of file
                0x03,0x00, //Pages in file
                0x00,0x00, //Relocations

                0x04,0x00, //Size of header in paragraphs
                0x00,0x00, //Minimum extra paragraphs needed
                0xFF,0xFF, //Maximum extra paragraphs needed

                0x00,0x00, //Initial SS value
                0xB8,0x00, //Initial SP value
                0x00,0x00, //Checksum
                0x00,0x00, //Initial IP value
                0x00,0x00, //Initial CS value
                0x40,0x00, //File Address of relocation table
                0x00,0x00, //Overlay number

                0,0,0,0,   //[Reserved]
                0,0,0,0,   //[Reserved]

                0x00,0x00, //OEM Identifier
                0x00,0x00, //OEM Information

                0,0,0,0,0, //[Reserved]
                0,0,0,0,0, //[Reserved]
                0,0,0,0,0, //[Reserved]
                0,0,0,0,0, //[Reserved]

                0x80,0x00, //File Address of PE header
                0x00,0x00  //
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

        private static void WritePE(FileStream fs, CodeInformation code_info)
        {
            //header information
            Image_Header header = new Image_Header();
            List<Image_IAT_Header> IAT_Headers = new List<Image_IAT_Header>();

            //temporary variables
            byte[] temp = null;
            uint offset = 0;
            uint offset_calculation = 0;

            //Data Sections
            MemoryStream RData_Section = new MemoryStream();
            MemoryStream Data_Section = new MemoryStream();

            //Virtual Address locations
            uint Code_RVA = header.OptionalHeader.SectionAlignment;
            uint RData_RVA = AlignTo(Code_RVA + (uint)code_info.Code.Length, header.OptionalHeader.SectionAlignment);
            uint Data_RVA = 0;

            //Import header offsets
            uint DLL_offset = 0;
            uint function_offset = 0;
            uint IAT_offset = 0;
            uint IAT_size = 0;

            #region imports
            //TODO: Major bug when including multiple libraries
            //For some reason this code is not taking into account multiple IA tables 
            //(Well, I know why: because I'm NOT taking into account multiple IA tables)


            /*
             * Offset:         Contents:
             * 0               [Import_Header_1]...[Import_Header_N],[Null_Header]
             * +IAT_offset     [IAT_1]...[IAT_N],[Null_IAT]
             * (+IAT_size)^    [IAT_1]...[IAT_N],[Null_IAT]
             * +func_offset    [0000 + Function_Name_1 + 00]...[0000 + Function_Name_N + 00]
             * +DLL_offset     [Dll_Name_1 + 00],...[Dll_Name_N + 00]
             * 
             * ^(+IAT_size) ==> Not to be included in final calculation. Only used to access the second IAT.
             */

            IAT_offset = (uint)((1 + code_info.SymbolInfo.Count) * 20); //number of (# of DLLs + NULL) * 20 bytes each

            IAT_size += 4; //4 byte NULL IAT
            foreach (var DLL in code_info.SymbolInfo)
            {
                //4 byte IAT
                IAT_size += 4 * (uint)DLL.Functions.Count;

                foreach (var function in DLL.Functions)
                {
                    //2 byte ordinal + name + null
                    DLL_offset += (uint)(2 + function.Item2.Length + 1);
                }
            }

            function_offset = IAT_size * 2;


            foreach (var DLL in code_info.SymbolInfo)
            {
                temp = new Image_Import_Header
                {
                    NameAddress = RData_RVA + IAT_offset + function_offset + DLL_offset,
                    ImportAddressTableAddress = RData_RVA + IAT_offset,
                    ImportLookUpTableAddress = RData_RVA + IAT_offset + IAT_size
                }.ToBytes();

                RData_Section.Write(temp, 0, temp.Length);

                foreach (var function in DLL.Functions)
                {
                    IAT_Headers.Add(new Image_IAT_Header
                    {
                        Address = RData_RVA + IAT_offset + function_offset
                    });
                    //adjust code to work with new functions
                    foreach (var replacement in function.Item3)
                    {
                        offset_calculation = header.OptionalHeader.ImageBase + RData_RVA + IAT_offset + function_offset;

                        code_info.Code[replacement + 0] = (byte)(offset_calculation >> 0);
                        code_info.Code[replacement + 1] = (byte)(offset_calculation >> 8);
                        code_info.Code[replacement + 2] = (byte)(offset_calculation >> 16);
                        code_info.Code[replacement + 3] = (byte)(offset_calculation >> 24);
                    }
                    function_offset += (uint)(2 + function.Item2.Length + 1);
                }
                DLL_offset += (uint)(DLL.LibraryName.Length + 1);
            }

            temp = new Image_Import_Header().ToBytes();
            RData_Section.Write(temp, 0, temp.Length);

            IAT_Headers.Add(new Image_IAT_Header { Address = 0 });

            for (int i = 0; i < 2; i++)
            {
                foreach (var IAT in IAT_Headers)
                {
                    temp = IAT.ToBytes();
                    RData_Section.Write(temp, 0, temp.Length);
                }
            }

            foreach (var DLL in code_info.SymbolInfo)
            {
                foreach (var functions in DLL.Functions)
                {
                    temp = BitConverter.GetBytes((short)functions.Item1);
                    RData_Section.Write(temp, 0, temp.Length);

                    temp = Encoding.UTF8.GetBytes(functions.Item2);

                    RData_Section.Write(temp, 0, temp.Length);
                    RData_Section.WriteByte(0);
                }
            }

            foreach (var DLL in code_info.SymbolInfo)
            {
                temp = Encoding.UTF8.GetBytes(DLL.LibraryName);
                RData_Section.Write(temp, 0, temp.Length);
                RData_Section.WriteByte(0);
            }


            header.OptionalHeader.DataDirectory[12].VirtualAddress = RData_RVA + IAT_offset;
            header.OptionalHeader.DataDirectory[12].Size = IAT_size;

            header.OptionalHeader.DataDirectory[1].VirtualAddress = RData_RVA;
            header.OptionalHeader.DataDirectory[1].Size = IAT_offset;


            //Correct Data RVA
            Data_RVA = AlignTo((uint)(RData_RVA + RData_Section.Length), header.OptionalHeader.SectionAlignment);
            #endregion

            #region Data Table
            Data_RVA = AlignTo(RData_RVA + offset, header.OptionalHeader.SectionAlignment);

            //string table replacements
            offset = 0;
            for (int i = 0; i < code_info.StringTable.Count; i++)
            {
                temp = Encoding.UTF8.GetBytes(code_info.StringTable[i].Item1);

                foreach (int replacement in code_info.StringTable[i].Item2)
                {
                    offset_calculation = header.OptionalHeader.ImageBase + Data_RVA + offset;

                    code_info.Code[replacement + 0] = (byte)(offset_calculation >> 0);
                    code_info.Code[replacement + 1] = (byte)(offset_calculation >> 8);
                    code_info.Code[replacement + 2] = (byte)(offset_calculation >> 16);
                    code_info.Code[replacement + 3] = (byte)(offset_calculation >> 24);
                }

                Data_Section.Write(temp, 0, temp.Length);

                Data_Section.WriteByte(0);

                offset += (uint)(temp.LongLength + 1);
            }
            #endregion

            #region Header
            //I don't want to hard code this as 0x170 because I might (maybe) add in more sections, and forget to change this
            //though at the moment with what's coded already, we're wasting a huge amount of time calculating this
            temp = header.ToBytes();
            uint raw_header_length = (uint)temp.LongLength;
            uint header_length = AlignTo(raw_header_length, header.OptionalHeader.FileAlignment);


            //.code section
            header.Text_Section.VirtualSize = (uint)code_info.Code.LongLength;
            header.Text_Section.SizeOfRawData = AlignTo((uint)code_info.Code.LongLength, header.OptionalHeader.FileAlignment);

            header.Text_Section.VirtualAddress = Code_RVA;
            header.Text_Section.PointerToRawData = header_length;

            //.rdata section
            header.RData_Section.VirtualSize = (uint)RData_Section.Length;
            header.RData_Section.SizeOfRawData = AlignTo((uint)RData_Section.Length, header.OptionalHeader.FileAlignment);

            header.RData_Section.VirtualAddress = RData_RVA;
            header.RData_Section.PointerToRawData = header_length + header.Text_Section.SizeOfRawData;

            //.data section
            header.Data_Section.VirtualSize = (uint)Data_Section.Length;
            header.Data_Section.SizeOfRawData = AlignTo((uint)Data_Section.Length, header.OptionalHeader.FileAlignment);

            header.Data_Section.VirtualAddress = Data_RVA;
            header.Data_Section.PointerToRawData = header.RData_Section.PointerToRawData + header.RData_Section.SizeOfRawData;


            header.OptionalHeader.Subsystem = code_info.Subsystem;
            header.OptionalHeader.AddressOfEntryPoint = code_info.EntryPoint + Code_RVA;
            header.OptionalHeader.BaseOfCode = Code_RVA;
            header.OptionalHeader.BaseOfData = RData_RVA;


            header.OptionalHeader.SizeOfHeaders = AlignTo((uint)RData_Section.Length, header.OptionalHeader.FileAlignment) + AlignTo((uint)Data_Section.Length, header.OptionalHeader.FileAlignment);
            header.OptionalHeader.SizeOfInitializedData = header.OptionalHeader.SizeOfHeaders;

            header.OptionalHeader.SizeOfImage = AlignTo(header_length, header.OptionalHeader.SectionAlignment)
                + AlignTo(header.Text_Section.SizeOfRawData, header.OptionalHeader.SectionAlignment)
                + AlignTo(header.RData_Section.SizeOfRawData, header.OptionalHeader.SectionAlignment)
                + AlignTo(header.Data_Section.SizeOfRawData, header.OptionalHeader.SectionAlignment);

            header.OptionalHeader.SizeOfCode = AlignTo((uint)code_info.Code.LongLength, header.OptionalHeader.FileAlignment);

            header.FileHeader.TimeDateStamp = (uint)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            #endregion

            #region Write Executable
            //finally write the file

            //write PE header
            temp = header.ToBytes();
            fs.Write(temp, 0, temp.Length);
            //write Header padding
            temp = CreatePadding((uint)fs.Position, header.OptionalHeader.FileAlignment);
            fs.Write(temp, 0, temp.Length);

            //write .code section
            fs.Write(code_info.Code, 0, code_info.Code.Length);
            //write .code padding
            temp = CreatePadding((uint)code_info.Code.Length, header.OptionalHeader.FileAlignment);
            fs.Write(temp, 0, temp.Length);

            //write .rdata section
            temp = RData_Section.GetBuffer();
            fs.Write(temp, 0, (int)RData_Section.Length);
            //write .rdata padding
            temp = CreatePadding((uint)RData_Section.Length, header.OptionalHeader.FileAlignment);
            fs.Write(temp, 0, temp.Length);

            //write .data section 
            temp = Data_Section.GetBuffer();
            fs.Write(temp, 0, (int)Data_Section.Length);
            //write .data padding
            temp = CreatePadding((uint)Data_Section.Length, header.OptionalHeader.FileAlignment);
            fs.Write(temp, 0, temp.Length);
            #endregion
        }


        private static byte[] CreatePadding(int size)
        {
            return new byte[size];
        }

        private static byte[] CreatePadding(uint value, uint alignment)
        {
            return new byte[(alignment - value % alignment)];
        }

        private static uint AlignTo(uint value, uint alignment)
        {
            return value + (alignment - value % alignment);
        }
    }
}
