using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace XSession.Modules
{
    internal static class WinLogger
    {
        public static void WriteError(string message)
        {
            if( string.IsNullOrEmpty(message) )
                return;

            EventLog.WriteEntry("Application Error", message);
        }


        public static void WriteInfo(string message)
        {
            if( string.IsNullOrEmpty(message) )
                return;

            EventLog.WriteEntry("Application", message);
        }
    }
}
