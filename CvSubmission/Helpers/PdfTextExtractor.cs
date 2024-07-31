using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.Content;
using PdfSharpCore.Pdf.Content.Objects;
using PdfSharpCore.Pdf.IO;

namespace CvSubmission.Helpers
{
    public static class PdfTextExtractor
    {
        public static string ExtractText(Stream pdfStream)





        {
            StringBuilder text = new StringBuilder();
            using (PdfDocument pdf = PdfReader.Open(pdfStream, PdfDocumentOpenMode.ReadOnly))
            {
                foreach (PdfPage page in pdf.Pages)
                {
                    var content = ContentReader.ReadContent(page);
                    text.Append(ProcessContent(content));
                }
            }
            return text.ToString();
        }

        private static string ProcessContent(CObject content)
        {
            StringBuilder text = new StringBuilder();
            if (content is COperator)
            {
                var cOperator = content as COperator;
                if (cOperator.OpCode.Name == OpCodeName.Tj.ToString() || cOperator.OpCode.Name == OpCodeName.TJ.ToString())
                {
                    foreach (var operand in cOperator.Operands)
                    {
                        text.Append(ProcessContent(operand));
                    }
                }
            }
            else if (content is CSequence)
            {
                var sequence = content as CSequence;
                foreach (var element in sequence)
                {
                    text.Append(ProcessContent(element));
                }
            }
            else if (content is CString)
            {
                var cString = content as CString;
                text.Append(cString.Value);
            }
            return text.ToString();
        }
    }
}
