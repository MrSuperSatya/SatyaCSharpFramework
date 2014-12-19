using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Printing;

namespace Satya
{
    class PrintReportWithoutPrintDialog : IDisposable
    {
        private int currentPageIndex;
        private IList<Stream> streams;
        private DataTable dataTable;
        private string dataSetName;
        private string reportName;

        public PrintReportWithoutPrintDialog(DataTable dataTable, string dataSetName,string reportName) {
            this.dataTable = dataTable;
            this.dataSetName = dataSetName;
            this.reportName = reportName;
        }

        private Stream createStream(string name,
                                   string fileNameExtension, Encoding encoding,
                                   string mimeType, bool willSeek)
        {
            Stream stream = new FileStream(@"..\..\" + name +
               "." + fileNameExtension, FileMode.Create);
            streams.Add(stream);
            return stream;
        }
        private void export(LocalReport report)
        {
            string deviceInfo =
              "<DeviceInfo>" +
              "  <OutputFormat>EMF</OutputFormat>" +
              "  <PageWidth>8.5in</PageWidth>" +
              "  <PageHeight>11in</PageHeight>" +
              "  <MarginTop>0.25in</MarginTop>" +
              "  <MarginLeft>0.25in</MarginLeft>" +
              "  <MarginRight>0.25in</MarginRight>" +
              "  <MarginBottom>0.25in</MarginBottom>" +
              "</DeviceInfo>";
            Warning[] warnings;
            streams = new List<Stream>();
            try {
                report.Render("Image", deviceInfo, createStream,
                   out warnings);
            } catch (Exception e) {
                MessageBox.Show(e.Message);
            }
            foreach (Stream stream in streams)
                stream.Position = 0;
        }
        private void printPage(object sender, PrintPageEventArgs ev)
        {
            Metafile pageImage = new
               Metafile(streams[currentPageIndex]);
            ev.Graphics.DrawImage(pageImage, ev.PageBounds);
            currentPageIndex++;
            ev.HasMorePages = (currentPageIndex < streams.Count);
        }
        public void print()
        {
            PrinterSettings settings = new PrinterSettings();
            string printerName = settings.PrinterName;
            if (streams == null || streams.Count == 0)
                return;
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = printerName;
            if (!printDoc.PrinterSettings.IsValid)
            {
                string msg = String.Format(
                   "Can't find printer \"{0}\".", printerName);
                MessageBox.Show(msg, "Print Error");
                return;
            }
            printDoc.PrintPage += new PrintPageEventHandler(printPage);
            printDoc.Print();
        }
        private void run()
        {
            LocalReport report = new LocalReport();
            report.ReportPath = @"..\..\" + reportName;  //@"..\..\Report1.rdlc"
            report.DataSources.Add(new ReportDataSource(dataSetName, dataTable));
            export(report);
            currentPageIndex = 0;
            print();
        }
        public void Dispose()
        {
            if (streams != null)
            {
                foreach (Stream stream in streams)
                    stream.Close();
                streams = null;
            }
        }
    }
}
