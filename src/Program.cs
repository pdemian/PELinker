using System;
using System.IO;
using Newtonsoft.Json;

namespace Linker
{
    public class LinkMain
    {
        internal class ProgramArgs
        {
            public bool Verbose;
            public uint FileAlignment;
            public uint SectionAlignment;
            public string FileIn;
            public string FileOut;

            public ProgramArgs()
            {
                Verbose = false;
                FileAlignment = 0;
                SectionAlignment = 0;
                FileIn = "";
                FileOut = null;
            }
        }

        public static void Main(string[] args)
        {
            ProgramArgs arguments = new ProgramArgs();
            try
            {
                //parse arguments
                arguments = ParseArguments(args);

                if (string.IsNullOrWhiteSpace(arguments.FileIn))
                {
                    throw new Exception("No input files");
                }
                if (!File.Exists(arguments.FileIn))
                {
                    throw new Exception("Input file not found");
                }

                //parse file
                var codeInfo = ReadCodeInfoFile(arguments.FileIn);

                if (arguments.FileOut == null)
                {
                    arguments.FileOut = Path.GetFileNameWithoutExtension(arguments.FileIn) + ".exe";
                }

                codeInfo.FileAlignment = arguments.FileAlignment;
                codeInfo.SectionAlignment = arguments.SectionAlignment;

                using (FileStream fs = new FileStream(arguments.FileOut, FileMode.Create))
                {
                    Linker.WriteEXE(fs, codeInfo, arguments.Verbose);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal Error: " + ex.Message);
                if (arguments.Verbose)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private static ProgramArgs ParseArguments(string[] args)
        {
            ProgramArgs arguments = new ProgramArgs();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-sectionalign":
                    case "-s":
                        if (i + 1 >= args.Length)
                        {
                            arguments.SectionAlignment = uint.Parse(args[++i]);
                        }
                        break;
                    case "-filealign":
                    case "-f":
                        if (i + 1 >= args.Length)
                        {
                            arguments.FileAlignment = uint.Parse(args[++i]);
                        }
                        break;
                    case "-verbose":
                    case "-v":
                        arguments.Verbose = true;
                        break;
                    case "-output":
                    case "-o":
                        if (i + 1 >= args.Length)
                        {
                            arguments.FileOut = args[++i];
                        }
                        break;
                    default:
                        if (i + 1 >= args.Length)
                        {
                            arguments.FileIn = args[i];
                        }
                        break;
                }
            }

            return arguments;
        }

        private static CodeInformation ReadCodeInfoFile(string file)
        {
            return JsonConvert.DeserializeObject<CodeInformation>(File.ReadAllText(file));
        }
    }
}