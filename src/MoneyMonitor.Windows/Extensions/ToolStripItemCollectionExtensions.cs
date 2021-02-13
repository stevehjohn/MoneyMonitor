using System;
using System.Windows.Forms;

namespace MoneyMonitor.Windows.Extensions
{
    public static class ToolStripItemCollectionExtensions
    {
        public static ToolStripMenuItem GetItem(this ToolStripItemCollection collection, string title)
        {
            foreach (ToolStripItem item in collection)
            {
                if (item.Text.Equals(title, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (ToolStripMenuItem) item;
                }
            }

            return null;
        }
    }
}