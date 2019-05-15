using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SessionCodeAnalysis.AppCode
{
    internal class SessionCodeFinder
    {
        private static readonly Regex[] s_regex = new Regex[] { new Regex(@"\bSession\b\s*[\[\(]\s*(?<key>[^\)\]]+)\s*[\]\)]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
                                                                new Regex(@"\bSession\b\s*\.\s*Add\s*\(\s*(?<key>[^,]+)\s*,", RegexOptions.IgnoreCase | RegexOptions.Compiled) };

        private object _lock = new object();
        public List<SessionUsage> Result { get; private set; } = new List<SessionUsage>();
        public List<string> Files { get; private set; } = new List<string>();


        public Dictionary<string, int> GetStatistics()
        {
            Dictionary<string, int> dict = null;

            lock( _lock ) {
                dict = (from r in this.Result
                            // 注意：这里分组时区分大小写，如果用 ToUpper()，结果就是全大写了，也不完美，所以后面再做合并
                        group r by r.SessionKey into g
                        select g
                        ).ToDictionary(g => g.Key, g => g.Count());
            }

            
            // 下面做大小写的合并
            Dictionary<string, int> data = new Dictionary<string, int>(dict.Count, StringComparer.OrdinalIgnoreCase);

            foreach(var x in dict ) {

                int value = 0;
                if( data.TryGetValue(x.Key, out value) )
                    data[x.Key] = value + x.Value;
                else
                    data[x.Key] = x.Value;
            }

            return data;
        }

        public void Execute(string srcPath)
        {
            this.Files.Clear();
            this.Result.Clear();

            string[] dirs = Directory.GetDirectories(srcPath, "*", SearchOption.AllDirectories);

            foreach( string dir in dirs )
                SearchDirectory(dir);


            // 计算用于标题显示的文件路径
            string root = srcPath.EndsWith("\\") ? srcPath : srcPath + "\\";

            lock( _lock ) {
                foreach( var x in this.Result )
                    x.TitlePath = x.FilePath.Substring(root.Length);
            }
        }


        private void SearchDirectory(string path)
        {
            if( Directory.Exists(path) == false )
                return;

            SearchDirectory(path, "*.asax");
            SearchDirectory(path, "*.ascx");
            SearchDirectory(path, "*.ashx");
            SearchDirectory(path, "*.aspx");
            SearchDirectory(path, "*.cs");
            SearchDirectory(path, "*.vb");
            SearchDirectory(path, "*.master");
        }

        private void SearchDirectory(string path, string ext)
        {
            string[] files = Directory.GetFiles(path, ext, SearchOption.TopDirectoryOnly);

            foreach( string file in files )
                SearchFile(file);

        }

        private void SearchFile(string file)
        {
            if( File.Exists(file) == false )
                return;

            this.Files.Add(file);

            List<SessionUsage> list = new List<SessionUsage>();

            // TODO: 这样读文件会有乱码问题！
            var lines = File.ReadAllLines(file);

            for(int i= 0; i<lines.Length;i++ ) {
                string line = lines[i];

                if( string.IsNullOrEmpty(line) )
                    continue;

                foreach( var regex in s_regex ) {
                    var matchs = regex.Matches(line);

                    foreach( Match m in matchs ) {
                        if( m.Success ) {

                            list.Add(new SessionUsage {
                                SessionKey = m.Groups["key"].Value,
                                FilePath = file,
                                CodeLine = line.Trim(),
                                LineNo = i + 1
                            });
                        }
                    }
                }
            }

            if( list.Count > 0 ) {
                lock( _lock ) {
                    this.Result.AddRange(list);
                }
            }
        }



    }
}
