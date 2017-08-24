using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace TqCcnetDashboard.Controllers.Api
{
    public class CcnetPluginController : AbstractApiController
    {
        public async Task<HttpResponseMessage> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var imsi_savePath = GetImsi_savePath();
            var real_savePath = GetReal_savePath();

            var streamProvider = new MultipartFormDataStreamProvider(imsi_savePath);
            await Request.Content.ReadAsMultipartAsync(streamProvider);

            foreach (MultipartFileData fileData in streamProvider.FileData)
            {
                if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                {
                    return Request.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted");
                }
                string fileName = fileData.Headers.ContentDisposition.FileName;
                if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                {
                    fileName = fileName.Trim('"');
                }
                if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                {
                    fileName = Path.GetFileName(fileName);
                }

                if (File.Exists(Path.Combine(real_savePath, fileName))) File.Delete(Path.Combine(real_savePath, fileName));
                File.Move(fileData.LocalFileName, Path.Combine(real_savePath, fileName));
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private string GetImsi_savePath()
        {
            var path = MapPathUtil.MapPath("~/pluginTemp");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        private string GetReal_savePath()
        {
            var path = MapPathUtil.MapPath("~/plugin");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }
}