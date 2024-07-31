using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Xceed.Words.NET;
using NPOI.XWPF.UserModel;
using NPOI.XWPF.Extractor;

namespace CvSubmission.Helpers
{
    public static class DocxTextExtractor
    {
        public static string ExtractText(Stream docxStream)
        {
            XWPFDocument doc = new XWPFDocument(docxStream);
            var textExtractor = new XWPFWordExtractor(doc);
            string text = textExtractor.Text;
            doc.Close();
            return text.ToString(); 
        }
    }
}

