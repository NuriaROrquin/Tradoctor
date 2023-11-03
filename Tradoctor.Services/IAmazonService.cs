using Amazon.Textract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tradoctor.Data;

namespace Tradoctor.Services
{
    public interface IAmazonService
    {
        public Task<TextractResult> GetTextFromDocument();
    }
}
