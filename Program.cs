using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Compiler
{
    class LinkMain
    {
        static void Main(string[] args)
        {
            Tuple<bool,int,int,string,string> arguments = new Tuple<bool,int,int,string,string>(false,-1,-1,null,null);
            try
            {
                //parse arguments
                arguments = ParseArguments(args);

                if(arguments.Item4 == null)
                {
                    throw new Exception("No input files");
                }
                if(!File.Exists(arguments.Item4))
                {
                    throw new Exception("Input file not found");
                }

                //parse file
                var codeInfo = ReadCodeInfoFile(arguments.Item4);

                if(arguments.Item5 == null)
                {
                    arguments = new Tuple<bool, int, int, string, string>(
                        arguments.Item1,
                        arguments.Item2,
                        arguments.Item3,
                        arguments.Item4,
                        arguments.Item4 + ".exe");
                }

                using (FileStream fs = new FileStream(arguments.Item5, FileMode.Create))
                {
                    Linker.WriteEXE(fs, codeInfo);
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine("Fatal Error: " + ex.Message);
                if(arguments.Item1)
                {
                    Console.WriteLine(ex);
                }
            }
        }
        private static Tuple<bool,int,int,string,string> ParseArguments(string[] args)
        {
            int SectionAlign = -1;
            int FileAlign = -1;
            string FileNameIn = null;
            string FileNameOut = null;
            bool Verbose = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-sectionalign":
                    case "-s":
                        SectionAlign = int.Parse(args[++i]);
                        break;
                    case "-filealign":
                    case "-f":
                        FileAlign = int.Parse(args[++i]);
                        break;
                    case "-verbose":
                    case "-v":
                        Verbose = true;
                        break;
                    case "-output":
                    case "-o":
                        FileNameOut = args[++i];
                        break;
                    default:
                        FileNameIn = args[i];
                        break;
                }
            }

            return new Tuple<bool, int, int, string,string>(Verbose, SectionAlign, FileAlign, FileNameIn,FileNameOut);
        }
        private static CodeInformation ReadCodeInfoFile(string file)
        {
            CodeInformation ci = new CodeInformation();

            //File Format:
            //Signature "CI"
            //[Header]
            //    [Size of Code: 4]
            //    [Size of String Table: 4]
            //    [Size of Import: 4]
            //    [Subsystem: 2]
            //    [EntryPoint: 4]
            //    [Reserved: 16]
            //[Code: N]
            //[String Table]
            //    [Number of strings: 4]
            //      [Number of replacements : 4]
            //      [replacements: 4N]
            //      [String Length: 4]
            //      [String]
            //[Import]
            //    [Number of Imports: 4]
            //        [Length of name]
            //        [Name of Library]
            //        [Number of Functions: 4]
            //            [Number of replacements: 4]
            //            [replacements: 4N]
            //            [Ordinal: 2]
            //            [Length of name: 4]
            //            [Function Name: N]

            byte[] temp = new byte[36];
            using(FileStream fs = new FileStream(file, FileMode.Open))
            {
                fs.Read(temp, 0, 36);
                if (!BytesToString(temp, 0, 2).Equals("CI"))
                    throw new Exception("Invalid file");

                int SizeOfCode = BitConverter.ToInt32(temp, 2);
                int SizeOfStringTable = BitConverter.ToInt32(temp, 6);
                int SizeOfImportTable = BitConverter.ToInt32(temp, 10);
                ci.Subsystem = BitConverter.ToUInt16(temp, 14);
                ci.EntryPoint = BitConverter.ToUInt32(temp,16);
                ci.Code = new byte[SizeOfCode];
                fs.Read(ci.Code, 0, SizeOfCode);

                fs.Read(temp,0, 4);
                int NumberOfStrings = BitConverter.ToInt32(temp, 0);
                for(int i = 0; i < NumberOfStrings; i++)
                {
                    fs.Read(temp,0, 4);
                    int NumberOfReplacements = BitConverter.ToInt32(temp, 0);
                    fs.Read(temp, 0, 4 * NumberOfReplacements);
                    List<int> replacements = new List<int>();
                    for(int k = 0; k < NumberOfReplacements; k++)
                    {
                        replacements.Add(BitConverter.ToInt32(temp, 4 * k));
                    }
                    fs.Read(temp, 0, 4);
                    int StrLen = BitConverter.ToInt32(temp,0);
                    byte[] str = new byte[StrLen];
                    fs.Read(str,0,StrLen);

                    ci.StringTable.Add(new Tuple<string,List<int>>(BytesToString(str,0,StrLen),replacements));
                }

                fs.Read(temp, 0, 4);
                int NumberOfImports = BitConverter.ToInt32(temp, 0);
                for(int i = 0; i < NumberOfImports; i++)
                {
                    fs.Read(temp, 0, 4);
                    int StrLen = BitConverter.ToInt32(temp, 0);
                    byte[] str = new byte[StrLen];
                    fs.Read(str, 0, StrLen);

                    CodeInformation.SymbolInformation si = new CodeInformation.SymbolInformation();
                    si.Functions = new List<Tuple<int, string, List<int>>>();
                    
                    si.LibraryName = BytesToString(str, 0, StrLen);

                    fs.Read(temp,0,4);
                    int NumberOfFunctions = BitConverter.ToInt32(temp,0);
                    for(int k = 0; k < NumberOfFunctions; k++)
                    {
                        fs.Read(temp, 0, 4);
                        int NumberOfReplacements = BitConverter.ToInt32(temp, 0);
                        List<int> replacements = new List<int>();
                        fs.Read(temp, 0, 4 * NumberOfReplacements);
                        for(int j = 0; j < NumberOfReplacements; j++)
                        {
                            replacements.Add(BitConverter.ToInt32(temp, 4*j));
                        }

                        fs.Read(temp, 0, 6);
                        short ordinal = BitConverter.ToInt16(temp, 0);
                        int LengthOfFunctionName = BitConverter.ToInt32(temp, 0);
                        str = new byte[LengthOfFunctionName];

                        fs.Read(str, 0, LengthOfFunctionName);

                        si.Functions.Add(new Tuple<int, string, List<int>>((int)ordinal, BytesToString(str,0,LengthOfFunctionName), replacements));
                    }
                }
            }
            return ci;
        }

        public static string BytesToString(byte[] bytestr, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < length; i++)
                sb.Append((char)bytestr[offset+i]);
            return sb.ToString();
        }
    }



}
