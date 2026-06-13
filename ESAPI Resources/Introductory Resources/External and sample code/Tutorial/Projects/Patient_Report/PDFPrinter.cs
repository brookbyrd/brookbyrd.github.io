using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.IO;
using Microsoft.Win32;

namespace Patient_Report
{
    class PDFPrinter
    {
        public void CreatePDF(Patient patient, List<ReportControl.DoseQMetric> dqms)
        {
            //build the pdf
            int Xmargin = 36;
            int Ymargin = 36;
            PdfDocument doc;
            PdfPage page;
            XGraphics gfx;

        }
    }
}
