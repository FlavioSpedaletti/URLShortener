using LiteDB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using URLShortener.Models;

namespace URLShortener.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShortController : ControllerBase
    {
        private readonly ILogger<ShortController> _logger;
        private readonly IWebHostEnvironment _env;

        public ShortController(ILogger<ShortController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        [HttpGet]
        [Produces("text/html")]
        public ContentResult Get()
        {
            var webRoot = _env.ContentRootPath;

            var fileContent = System.IO.File.ReadAllText(webRoot + "/index.html");

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = fileContent
            };
        }

        [HttpPost]
        [Route("/url")]
        public IActionResult Post(UrlDto url)
        {
            if (!Uri.TryCreate(url.Url, UriKind.Absolute, out var inputUri))
            {
                return BadRequest("URL is invalid.");
            }

            var liteDb = HttpContext.RequestServices.GetRequiredService<ILiteDatabase>();
            var links = liteDb.GetCollection<ShortUrl>(BsonAutoId.Int32);
            var entry = new ShortUrl(inputUri);
            links.Insert(entry);

            var result = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{entry.UrlChunk}";
            return new JsonResult(new { url = result });
        }

        [HttpGet]
        [Route("/getAll")]
        public IActionResult GetAll()
        {
            var liteDb = HttpContext.RequestServices.GetRequiredService<ILiteDatabase>();
            var collection = liteDb.GetCollection<ShortUrl>();

            var entries = collection.FindAll();
            return new JsonResult(new { entries });
        }
    }
}
