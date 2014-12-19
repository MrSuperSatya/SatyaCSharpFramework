using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Satya
{
	public static class StaticMethods : Object
	{
		public static void searchItemInListView(ref ListView lv, string text)
		{
			foreach (ListViewItem lvi in lv.Items)
			{
				if (text == "")
					lvi.BackColor = Color.White;
				else
					for (int i = 0; i < lvi.SubItems.Count; i++)
					{
						if (lvi.SubItems[i].Text.IndexOf(text,
							StringComparison.OrdinalIgnoreCase) >= 0)
						{
							lvi.BackColor = Color.LightBlue;
							break;
						}
						else
							lvi.BackColor = Color.White;
					}
			}
		}
		public static string removeTime(string date) {
            return date.Substring(0, date.IndexOf(' '));
        }
	}
}
