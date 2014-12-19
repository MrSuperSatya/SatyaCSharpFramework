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
	public class SatTextBox : TextBox
	{
		private string autoText = "";
        private bool autoCalculate;
        private DBDataType dataType;

        public DBDataType DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }
        public bool AutoCalculate
        {
            get { return autoCalculate; }
            set { autoCalculate = value; }
        }
        public string AutoText
		{
			get { return autoText; }
			set { autoText = this.Text = value; }
		}
		
		public SatTextBox()
		{
			this.autoText = this.Text = "Search...";
			this.ForeColor = Color.Gray;
            this.autoCalculate = false;
            dataType = DBDataType.String;
		}
		public SatTextBox(string autoText)
		{
			this.autoText = autoText;
			this.ForeColor = Color.Gray;
            this.autoCalculate = false;
            dataType = DBDataType.String;
		}

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if(dataType == DBDataType.Num)
                if (! char.IsControl(e.KeyChar) && 
                    ! char.IsDigit(e.KeyChar) && 
                    ! "+-*/.".Contains(e.KeyChar))
                    e.Handled = true;
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
			if (this.Text.Length == 0 && autoText.Length > 0)
			{
				this.Text = autoText;
				this.ForeColor = Color.Gray;
			}
            if (autoCalculate) { 
                if(Text.Contains("+") || Text.Contains("-") || 
                   Text.Contains("*") || Text.Contains("/")) {
                    int signIndex = 0;
                    if(Text.Contains("+"))
                        signIndex = Text.IndexOf("+");
                    else if(Text.Contains("-"))
                        signIndex = Text.IndexOf("-");
                    else if(Text.Contains("*"))
                        signIndex = Text.IndexOf("*");
                    else if(Text.Contains("/"))
                        signIndex = Text.IndexOf("/");
                    
                    double num1 = Convert.ToDouble(Text.Substring(0,signIndex));
                    double num2 = Convert.ToDouble(Text.Substring(signIndex+1));
                    double result = 0;
                    if(Text.Contains("+"))
                        result = num1 + num2;
                    else if(Text.Contains("-"))
                        result = num1 - num2;
                    else if(Text.Contains("*"))
                        result = num1 * num2;
                    else if(Text.Contains("/"))
                        result = num1 / num2;

                    Text = result.ToString();
                }
            }
		}
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                case Keys.Down:
                    return true;
                case Keys.Shift | Keys.Up:
                case Keys.Shift | Keys.Down:
                    return true;
            }
            return base.IsInputKey(keyData);
        }
    }
}
