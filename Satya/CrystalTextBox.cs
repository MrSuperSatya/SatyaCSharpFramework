using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Satya
{
	/// <summary>
	/// An implementation of TextBox with auto display text when there is input from user
	/// </summary>
	class CrystalTextBox : TextBox
	{
		private string autoText;

		public CrystalTextBox()
		{
			this.autoText = this.Text = "Input here...";
			this.ForeColor = Color.Gray;
		}
		public CrystalTextBox(string autoText)
		{

			this.autoText = this.Text = autoText;
			this.ForeColor = Color.Gray;
		}
		public string AutoText
		{
			get { return autoText; }
			set { autoText = this.Text = value; }
		}
		protected override void OnEnter(EventArgs e)
		{
			base.OnEnter(e);
			if (this.Text.Equals(this.autoText))
			{
				this.ForeColor = SystemColors.WindowText;
				this.Text = "";
			}
		}
		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);
			if (this.Text.Length == 0)
			{
				this.Text = autoText;
				this.ForeColor = Color.Gray;
			}
		}
	}
}
