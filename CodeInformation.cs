using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class CodeInformation
    {
        public byte[] Code;

        public uint EntryPoint;

        public ushort Subsystem;

        public class SymbolInformation
        {
            public string LibraryName;
            public List<Tuple<int, string, List<int>>> Functions;
        }

        public List<SymbolInformation> SymbolInfo;

        public List<Tuple<string, List<int>>> StringTable;

        public CodeInformation()
        {
            StringTable = new List<Tuple<string, List<int>>>();
            SymbolInfo = new List<SymbolInformation>();
        }
    }
}
