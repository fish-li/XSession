using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SessionCodeAnalysis.AppCode
{
    internal static class ListViewExtensions
    {
        public static ListViewItem GetFirstSelected(this ListView listView)
        {
            if( listView == null )
                throw new ArgumentNullException(nameof(listView));

            var items = listView.SelectedItems;
            return items.Count > 0 ? items[0] : null;
        }
    }
}
