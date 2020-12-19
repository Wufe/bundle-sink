using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BundleSink.Models;
using BundleSink.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BundleSink.ActionFilters {
    public class BodyRewriteResultFilter : ResultFilterAttribute {
        private readonly BundleSinkSettings _settings;
        private readonly SinkService _sinkService;

        private readonly Regex _replaceRegex = new Regex(@"<sink name=""(.+?)"" temp />");

        public BodyRewriteResultFilter(
            BundleSinkSettings settings,
            SinkService sinkService
        )
        {
            _settings = settings;
            _sinkService = sinkService;
        }

        public async override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (_settings.RewriteOutput) {
                using (var bufferStream = new MemoryStream())
                using (var streamReader = new StreamReader(bufferStream))
                using (var writeStream = new MemoryStream())
                using (var streamWriter = new StreamWriter(writeStream))
                {
                    var originalBodyStream = context.HttpContext.Response.Body;
                    context.HttpContext.Response.Body = bufferStream;

                    await next();

                    bufferStream.Seek(0, SeekOrigin.Begin);
                    var content = streamReader.ReadToEnd();

                    content = ParseBody(content);

                    streamWriter.Write(content);
                    streamWriter.Flush();

                    writeStream.Seek(0, SeekOrigin.Begin);

                    await writeStream.CopyToAsync(originalBodyStream);
                    context.HttpContext.Response.Body = originalBodyStream;
                }
            }
        }

        private string ParseBody(string body) {
            return _replaceRegex.Replace(body, match => {
                var sinkName = match.Groups[1].Value;
                return _sinkService.SerializeSink(sinkName);
            });
        }
    }
}