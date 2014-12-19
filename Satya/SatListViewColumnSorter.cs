using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;

namespace Satya
{
	public class SatListViewColumnSorter : IComparer
	{
		private int ColumnToSort;
		private SortOrder OrderOfSort;
		private CaseInsensitiveComparer ObjectCompare;

		public SatListViewColumnSorter()
		{
			ColumnToSort = 0;
			OrderOfSort = SortOrder.None;
			ObjectCompare = new CaseInsensitiveComparer();
		}

		/// <param name="x">First object to be compared</param>
		/// <param name="y">Second object to be compared</param>
		/// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
		public int Compare(object x, object y)
		{
			int compareResult;
			ListViewItem lbi1, lbi2;

			lbi1 = (ListViewItem)x;
			lbi2 = (ListViewItem)y;

			double d1;
			if (double.TryParse(lbi1.SubItems[ColumnToSort].Text, out d1))
			{
				double d2;
				double.TryParse(lbi2.SubItems[ColumnToSort].Text, out d2);
				compareResult = ObjectCompare.Compare(d1, d2);
			}
			else if (lbi1.SubItems[ColumnToSort].Text.Contains("$"))
			{
				double a = Convert.ToDouble(lbi1.SubItems[ColumnToSort].
					Text.Replace("$", "").Replace(" ", ""));
				double b = Convert.ToDouble(lbi2.SubItems[ColumnToSort].
					Text.Replace("$", "").Replace(" ", ""));
				compareResult = ObjectCompare.Compare(a, b);
			}
			else if (lbi1.SubItems[ColumnToSort].Text.Contains("/"))
			{
				DateTime date1, date2;
				DateTime.TryParse(lbi1.SubItems[ColumnToSort].Text, out date1);
				DateTime.TryParse(lbi2.SubItems[ColumnToSort].Text, out date2);
				compareResult = ObjectCompare.Compare(date1, date2);
			}
			else
				compareResult = ObjectCompare.Compare(lbi1.SubItems[ColumnToSort].Text, lbi2.SubItems[ColumnToSort].Text);

			if (OrderOfSort == SortOrder.Ascending)
				return compareResult;
			else if (OrderOfSort == SortOrder.Descending)
				return (-compareResult);
			else
				return 0;
		}
		
		/// <summary>
		/// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
		/// </summary>
		public int SortColumn
		{
			set
			{
				ColumnToSort = value;
			}
			get
			{
				return ColumnToSort;
			}
		}
		
		/// <summary>
		/// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
		/// </summary>
		public SortOrder Order
		{
			set
			{
				OrderOfSort = value;
			}
			get
			{
				return OrderOfSort;
			}
		}
		
		public void sortValidate(ref ColumnClickEventArgs e)
		{
			if (e.Column == SortColumn)
			{
				if (Order == SortOrder.Ascending)
					Order = SortOrder.Descending;
				else
					Order = SortOrder.Ascending;
			}
			else
			{
				SortColumn = e.Column;
				Order = SortOrder.Ascending;
			}
		}
	}
}
