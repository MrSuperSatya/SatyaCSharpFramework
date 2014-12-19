using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Satya
{
    public partial class SatInputBox : Form
    {
		public string LabelText
		{
			set	{
				this.label1.Text = value;
				this.Width = label1.Width + textBox1.Width + 48;
			}
			get { return this.label1.Text; }
		}
		public string InputText {
			set { this.textBox1.Text = value; }
			get { return this.textBox1.Text; }
		}
    	public SatInputBox()
        {
            InitializeComponent();
			this.Text = "Input Info";
			this.label1.Text = "Input: ";
        }
        public SatInputBox(string title, string label) 
        {
			InitializeComponent();
			this.Text = title;
			this.label1.Text = label;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Width = label1.Width + textBox1.Width + 48;
			this.textBox1.Select();
        }
        
        private void buttonOk_Click(object sender, EventArgs e)
        {
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Close();
        }
    }
}
