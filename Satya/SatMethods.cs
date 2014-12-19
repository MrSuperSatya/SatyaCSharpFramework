using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Satya
{
	public static class SatMethods : Object
	{
		private static Random random = new Random();
        // Make it likely to return high number. ex: return 8 or 9 or 10 if max = 10
        private static int highNumberPercentage = 10;

        public static int HighNumberPercentage
        {
            get { return SatMethods.highNumberPercentage; }
            set { SatMethods.highNumberPercentage = value; }
        }

		public static void searchItemInListView(ListView lv, string text)
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
						{
							lvi.BackColor = Color.White;
						}
					}
			}
		}
		public static string removeTimeFromDate(string date) {
            return date.Substring(0, date.IndexOf(' '));
        }


        public static int getRandomNum(int minNum, int maxNum, bool likelyToGetHighNumber=false) //ex: minNum=1, max=4 -> return 1 or 2 or 3     
        {
            if (likelyToGetHighNumber && trueOrFalse(90))//sometimes it can return low number even if likelyToGetHighNumber = true
            {
                int newMinNum = maxNum - (maxNum - minNum) * highNumberPercentage / 100;
                if (newMinNum < minNum)
                    newMinNum = minNum;
                return random.Next(newMinNum, maxNum);
            }
            return random.Next(minNum, maxNum); 
        }
        public static int[] getRandomNums(int minNum, int maxNum, int count) {
			int[] randomNums = new int[count];
			string allChoosenNum = "";
			while (count > 0)
			{
				int r = random.Next(minNum, maxNum);
				if (!allChoosenNum.Contains(r.ToString())) {
					allChoosenNum += r.ToString();
					randomNums[count-1] = r;
					count--;
				}
			}
			return randomNums;
		}
        public static int[] getRandomNums(int maxNum, int count, bool likelyToGetHighNumber = false)
		{
			int[] randomNums = new int[count];
			string allChoosenNum = "";
			while (count > 0)
			{
                int r = getRandomNum(0,maxNum,likelyToGetHighNumber);

				if (!allChoosenNum.Contains(r.ToString()))
				{
					allChoosenNum += r.ToString();
					randomNums[count - 1] = r;
					count--;
				}
			}
			return randomNums;
		}
		public static int[] getRandomNumsExcept(int minNum, int maxNum, int count, int exceptNum)
		{
			int[] randomNums = new int[count];
			string allChoosenNum = exceptNum.ToString();
			while (count > 0)
			{
				int r = random.Next(minNum, maxNum);
				if (!allChoosenNum.Contains(r.ToString()))
				{
					allChoosenNum += r.ToString();
					randomNums[count - 1] = r;
					count--;
				}
			}
			return randomNums;
		}
        public static bool trueOrFalse(int probablilityPercentageToBeTrue) {
            return random.Next(1, 101) <= probablilityPercentageToBeTrue;                
        }
        // --------------   PictureBox Methods  ----------------------------
        public static byte[] convertFromPicturePathToByte(string path)
        {
            FileInfo fi = new FileInfo(path);
            long fileSize = fi.Length;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            return br.ReadBytes((int)fileSize);
        }
        public static byte[] imageToByteArray(System.Drawing.Image image)
        {
            if (image == null) return null;
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }
        public static Image byteArrayToImage(byte[] byteArray)
        {
            if (byteArray == null) return null;
            MemoryStream ms = new MemoryStream(byteArray);
            Image returnImage = Image.FromStream(ms);
            return returnImage;
        }

        public static void clearTextBox(Form f) {
            foreach (Control c in f.Controls)
                if (c is TextBox)
                    c.Text = "";
        }
    }
}
