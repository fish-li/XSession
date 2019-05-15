using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace SessionCodeAnalysis.AppCode
{
    public static class NotepadxxHelper
    {
        private static string s_installPath = null;

        public static string GetInstallPath()
        {
            if( s_installPath == null ) {
                string directory = Environment.Is64BitOperatingSystem
                                    ? Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Notepad++", null, string.Empty).ToString()
                                    : Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Notepad++", null, string.Empty).ToString();

                if( string.IsNullOrEmpty(directory) == false )
                    s_installPath = Path.Combine(directory, "notepad++.exe");

            }

            return s_installPath;
        }


        /// <summary>
        /// 用 Notepad++ 打开一个文件，并定位到指定的行号
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lineIndex"></param>
        public static void OpenFile(string filePath, int lineIndex)
        {
            string exePath = GetInstallPath();

            if( string.IsNullOrEmpty(exePath) ) {
                MessageBox.Show("当前机器没有安装 Notepad++", nameof(NotepadxxHelper), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }


            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = exePath;
            info.Arguments = $" -n{lineIndex}  \"{filePath}\"";

            try {
                Process.Start(info);
            }
            catch( Exception ex ) {
                MessageBox.Show(ex.Message, nameof(NotepadxxHelper), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


    }
}
