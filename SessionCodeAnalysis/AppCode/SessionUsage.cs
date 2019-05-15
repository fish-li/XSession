using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionCodeAnalysis.AppCode
{
    internal class SessionUsage
    {
        public string SessionKey { get; set; }

        public string FilePath { get; set; }

        public string TitlePath { get; set; }

        public string CodeLine { get; set; }

        public int LineNo { get; set; }
    }
}
