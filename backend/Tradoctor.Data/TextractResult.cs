using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tradoctor.Data
{
    public class TextractResult
    {
        public string PrescriptionText { get; set; }

        public float Confidence { get; set; }
    }
}
