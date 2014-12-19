using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Satya
{
	public class SatListBox : ListBox
	{
		private SatIDBConnector connector;
		private string sql = "";
		private List<string> listId;
		public List<string> ListId
		{
			get { return listId; }
			set { listId = value; }
		}
        private TextBox searchTextBox;

        public TextBox SearchTextBox
        {
            get { return searchTextBox; }
            set
            {
                searchTextBox = value;
                if (value != null)
                    searchTextBox.TextChanged += searchTextBox_TextChanged;
            }
        }

		public SatListBox()
		{
			listId = new List<string>();
		}

		public string getSelectedId()
		{
			return listId.ElementAt(SelectedIndex);
		}
		public string selectedId()
		{
			return listId.ElementAt(SelectedIndex);
		}
		public string getIdAt(int index)
		{
			return listId.ElementAt(index);
		}
		public string idAt(int index)
		{
			return listId.ElementAt(index);
		}
		public void setData(SatIDBConnector connector, string sql)
		{
			connector.setRowSource(this, sql);
			this.sql = sql;
			this.connector = connector;
		}
		public void refreshData()
		{
			connector.setRowSource(this, sql);
		}
        void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (this.Text == "")
                this.SelectedItems.Clear();
            else
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].ToString().IndexOf(this.Text,
                                StringComparison.OrdinalIgnoreCase) >= 0)
                        this.SelectedItems.Add(this.Items[i]);
                    else
                        this.SelectedItems.Remove(this.Items[i]);
                }
        }
    }
}
