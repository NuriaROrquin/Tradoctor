using Amazon.Textract.Model;
using Microsoft.AspNetCore.Mvc;
using Tradoctor.Data;
using Tradoctor.Services;

namespace Tradoctor.API.Controllers
{
    [Route("Textract")]
    [ApiController]
    public class HomeController : Controller
    {
        private IAmazonService _amazonService;

        public HomeController(IAmazonService amazonService)
        {
            _amazonService = amazonService;
        }

        [HttpPost (Name = "GetResultFromDocument")]
        public Task<TextractResult> GetResultFromDocument([FromBody] ImageTextract imageTextract)
        {
            return _amazonService.GetTextFromDocument(imageTextract);
        }
    }
}
