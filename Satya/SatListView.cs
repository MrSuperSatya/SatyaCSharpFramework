using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Satya
{    
	public class SatListView : ListView
	{
		private SatListViewColumnSorter listViewColumnSorter;
		private SatIDBConnector connector;
        private TextBox searchTextBox;
		private string sql = "";
        private int searchColumnIndex = -1;
        private ListViewSearchMode searchMode = ListViewSearchMode.Hide;
        private List<ListViewItem> dataItems;

        public ListViewSearchMode SearchMode
        {
            get { return searchMode; }
            set { searchMode = value; }
        }
        public int SearchColumnIndex
        {
            get { return searchColumnIndex; }
            set { searchColumnIndex = value; }
        }
        public TextBox SearchTextBox
        {
            get { return searchTextBox; }
            set { 
                searchTextBox = value;
                if (searchTextBox != null)
                    searchTextBox.TextChanged += searchTextBox_TextChanged;
            }
        }

        public SatListView() {
            dataItems = new List<ListViewItem>();
            listViewColumnSorter = new SatListViewColumnSorter();
            this.ListViewItemSorter = listViewColumnSorter;
            this.FullRowSelect = true;
            this.View = System.Windows.Forms.View.Details;
            searchMode = ListViewSearchMode.Hide;
        }

        void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (searchMode == ListViewSearchMode.Hightlight)
                searchByHighlight();
            else
                searchByHide();
        }
        public void searchByHighlight() {
            string searchText = searchTextBox.Text;
            foreach (ListViewItem lvi in this.Items)
            {
                if (searchText == "")
                    lvi.BackColor = Color.White;
                else
                {
                    if (searchColumnIndex == -1)
                        for (int i = 0; i < lvi.SubItems.Count; i++)
                        {
                            if (lvi.SubItems[i].Text.IndexOf(searchText,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                lvi.BackColor = Color.LightBlue;
                                break;
                            }
                            else
                                lvi.BackColor = Color.White;
                        }
                    else
                    {
                        if (lvi.SubItems[searchColumnIndex].Text.IndexOf(searchText,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                            lvi.BackColor = Color.LightBlue;
                        else
                            lvi.BackColor = Color.White;
                    }
                }
            }         
        }
        public void searchByHide(){
            this.Items.Clear();
            string searchText = searchTextBox.Text;
            foreach (ListViewItem lvi in dataItems)
            {               
                if (searchText == "")
                    this.Items.Add(lvi);
                else
                {
                    if (searchColumnIndex == -1)
                        for (int i = 0; i < lvi.SubItems.Count; i++)
                        {
                            if (lvi.SubItems[i].Text.IndexOf(searchText,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                this.Items.Add(lvi);
                                break;
                            }
                        }
                    else
                        if (lvi.SubItems[searchColumnIndex].Text.IndexOf(searchText,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                            this.Items.Add(lvi);
                }
            }         
        }
		protected override void OnColumnClick(ColumnClickEventArgs e)
		{
			base.OnColumnClick(e);
			listViewColumnSorter.sortValidate(ref e);
			this.Sort();
		}
		public void setData(SatIDBConnector connector, string sql) {
			connector.setRowSource(this, sql);
			this.sql = sql;
			this.connector = connector;
		}
		public void refreshData() {
			if(connector != null && sql != null)
				connector.setRowSource(this, sql);
		}
        public void copyListViewItemsToList() {
            dataItems.Clear();
            foreach(ListViewItem lvi in Items)
                dataItems.Add(lvi);
        }    
    }

    public enum ListViewSearchMode{
        Hightlight = 1,
        Hide = 2
    }
}
