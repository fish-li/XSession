using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SessionCodeAnalysis.AppCode;

namespace SessionCodeAnalysis
{
    public partial class MainForm1 : Form
    {
        private readonly SessionCodeFinder _finder = new SessionCodeFinder();
        private int _gapWidth1 = 0;
        private int _gapWidth2 = 0;

        public MainForm1()
        {
            InitializeComponent();
        }

        private void MainForm1_Shown(object sender, EventArgs e)
        {
            this.txtSrcPath.Text = @"E:\TFS\项目代码\总部招商\ERP256\源代码\分支10";

            string notepadxxPath = NotepadxxHelper.GetInstallPath();
            if( string.IsNullOrEmpty(notepadxxPath) )
                this.labNotepadxxTip.Visible = true;

            _gapWidth1 = this.listView1.Width - this.listView1.Columns[0].Width - this.listView1.Columns[1].Width;
            _gapWidth2 = this.listView2.Width - this.listView2.Columns[0].Width - this.listView2.Columns[1].Width - this.listView2.Columns[2].Width;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            if( this.btnSearch.Enabled == false )
                return;

            if( txtSrcPath.Text.Length == 0 )
                return;

            string path = txtSrcPath.Text.Trim();
            if( Directory.Exists(path) == false ) {
                MessageBox.Show("指定的目录不存在。", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            this.txtSrcPath.Enabled = false;
            this.btnSearch.Enabled = false;
            this.backgroundWorker1.RunWorkerAsync(txtSrcPath.Text.Trim());
            this.toolStripStatusLabel1.Text = "正在搜索文件…………";

            this.listView1.Items.Clear();
            this.listView2.Items.Clear();

            this.timer1.Enabled = true;
        }


        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string srcPath = (string)e.Argument;
            _finder.Execute(srcPath);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            ShowListView1(1);
        }


        private void ShowListView1(int sortIndex)
        {
            Dictionary<string, int> data = _finder.GetStatistics();

            if( sortIndex == 0 ) {
                data = data.OrderBy(x => x.Key).ToDictionary(x=>x.Key, x=>x.Value);
            }
            else if( sortIndex == 1 ) {
                data = data.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }

            this.listView1.BeginUpdate();
            this.listView1.Items.Clear();

            foreach( var x in data ) {
                ListViewItem item = new ListViewItem(x.Key, 0);
                item.SubItems.Add(x.Value.ToString());
                this.listView1.Items.Add(item);
            }
            this.listView1.EndUpdate();
        }

        private void ListView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ShowListView1(e.Column);
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.timer1.Enabled = false;
            this.txtSrcPath.Enabled = true;
            this.btnSearch.Enabled = true;

            this.Timer1_Tick(null, null);

            this.toolStripStatusLabel1.Text = $"共搜索到 {_finder.Result.Count} 个匹配行，{this.listView1.Items.Count}个Session用法。";

            if( e.Error != null ) {
                MessageBox.Show(e.Error.ToString(), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void ListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowListView2(true);
        }


        /// <summary>
        /// 计算分组标题上需要增加几个空格，以便看起来标题与第2列对齐
        /// </summary>
        /// <returns></returns>
        private int GetGroupTitleSpaceCount()
        {
            using( var graph = this.listView2.CreateGraphics() ) {
                SizeF s = graph.MeasureString(" ", this.listView2.Font);
                int width = this.listView2.Columns[0].Width - this.imageList1.ImageSize.Width;
                return (int)(width / s.Width);
            }
        }

        private void ShowListView2(bool limit)
        {
            if( this.backgroundWorker1.IsBusy )
                return;

            this.listView2.Items.Clear();
            this.listView2.Groups.Clear();

            ListViewItem item = this.listView1.GetFirstSelected();
            if( item == null )
                return;


            string key = item.Text;

            this.listView2.BeginUpdate();

            var list = (from x in _finder.Result
                        where string.Equals( x.SessionKey, key, StringComparison.OrdinalIgnoreCase)
                        group x by x.TitlePath into g
                        orderby g.Key
                        select new { File = g.Key, List = g.ToList() }
             ).ToList();



            int i = 0;
            int spaceCount = GetGroupTitleSpaceCount();

            foreach( var x in list ) {

                // TODO: 如果显示之后，再调整列宽，这里就不能及时刷新
                // 在ColumnResize事件中，如果重新指定 group.Header 会引发异常（.NET BUG）
                ListViewGroup group = new ListViewGroup(new string(' ', spaceCount) + x.File);
                group.Name = x.File;
                this.listView2.Groups.Add(group);
                int fileIcon = GetFileIcon(x.File);

                foreach( var c in x.List ) {
                    i++;

                    // 默认只显示前300行，显示太多会造成UI卡顿
                    if( limit && i > 300 ) {

                        ListViewGroup g2 = new ListViewGroup("View more");
                        this.listView2.Groups.Add(g2);

                        ListViewItem lx = new ListViewItem("", 4);
                        lx.SubItems.Add("<<双击查看剩余记录>>");
                        lx.ForeColor = Color.Blue;
                        lx.Group = g2;
                        this.listView2.Items.Add(lx);

                        break;
                    }

                    ListViewItem li = new ListViewItem(i.ToString(), fileIcon);
                    li.SubItems.Add(c.CodeLine);
                    li.SubItems.Add(c.LineNo.ToString());
                    li.Group = group;
                    li.Tag = c;

                    this.listView2.Items.Add(li);
                }

                if( limit && i > 300 )
                    break;
            }

            this.listView2.EndUpdate();
        }

        private int GetFileIcon(string file)
        {
            if( file.EndsWith(".cs") )
                return 1;

            if( file.EndsWith(".vb") )
                return 2;

            return 3;
        }

        private void ListView2_ItemActivate(object sender, EventArgs e)
        {
            ListViewItem item = this.listView2.GetFirstSelected();

            if( item == null )
                return;

            if( item.ImageIndex == 4 ) {
                ShowListView2(false);

                if( listView2.Items.Count > 300 ) {
                    listView2.Items[300].Selected = true;
                    listView2.Items[300].EnsureVisible();
                }
                return;
            }
            else {
                SessionUsage usage = item.Tag as SessionUsage;
                NotepadxxHelper.OpenFile(usage.FilePath, usage.LineNo);
            }

        }

        private void TxtSrcPath_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.KeyCode == Keys.Enter )
                this.BtnSearch_Click(null, null);
        }

        private void ListView1_Resize(object sender, EventArgs e)
        {
            if( _gapWidth1 == 0 )
                return;

            this.listView1.Columns[0].Width = this.listView1.Width - this.listView1.Columns[1].Width - _gapWidth1;
        }

        private void ListView2_Resize(object sender, EventArgs e)
        {
            if( _gapWidth2 == 0 )
                return;

            this.listView2.Columns[1].Width = this.listView2.Width - this.listView2.Columns[0].Width - this.listView2.Columns[2].Width - _gapWidth2;
        }

        private void ListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.Control && e.KeyCode == Keys.C ) {
                ListViewItem item = this.listView1.GetFirstSelected();
                if( item != null ) {
                    Clipboard.SetText(item.Text);
                }
            }
        }

        private void ListView2_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.Control && e.KeyCode == Keys.C ) {
                ListViewItem item = this.listView2.GetFirstSelected();
                if( item != null ) {
                    Clipboard.SetText(item.SubItems[1].Text);
                }
            }
        }
    }
}
