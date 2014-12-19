using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Satya
{
	public class SatCommandLink : Button
	{
		const int BS_COMMANDLINK = 0x0000000E;

		public SatCommandLink()
		{
			this.FlatStyle = FlatStyle.System;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cParams = base.CreateParams;
				cParams.Style |= BS_COMMANDLINK;
				return cParams;
			}
		}
	}
}
