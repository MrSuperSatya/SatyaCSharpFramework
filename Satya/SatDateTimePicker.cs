using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace Satya
{
	public class SatDateTimePicker : DateTimePicker
	{
		public SatDateTimePicker()
			: base()
		{
			this.Format = DateTimePickerFormat.Custom;
			this.CustomFormat = "dd/MM/yyyy hh:mm:ss tt";
		}
		//public override string Text
		//{
		//    get
		//    {
		//        //DateTime dateTime = DateTime.ParseExact(base.Text,
		//        //                                "dd-MM-yyyy HH:mm:ss",
		//        //                                System.Globalization.CultureInfo.InvariantCulture);
		//        //return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
		//        return base.Text;
		//    }
		//    set
		//    {
		//        //DateTime dateTime = DateTime.ParseExact(value,
		//        //                                "dd/MM/yyyy hh:mm:ss tt",
		//        //                                new CultureInfo("en-US"),
		//        //                            DateTimeStyles.None);
		//        //base.Text = dateTime.ToString("dd-MM-yyyy HH:mm:ss");
		//        base.Text = value;
		//    }
		//}
	}
}
