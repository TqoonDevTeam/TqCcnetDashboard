using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace TqCcnetDashboard.Controllers.Api
{
    public class CcnetPluginController : ApiController
    {
        public async Task<HttpResponseMessage> Post()
        {
            if (!Request.Content.IsMimeMultipartContent() || CurrentSite.Updator.NowBusy)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            try
            {
                CurrentSite.Updator.CleanUpPluginUpdateDownloadFolder();

                var provider = new MultipartFormDataStreamProvider(CurrentSite.Updator.GetPluginUpdateDownloadFolder());
                await Request.Content.ReadAsMultipartAsync(provider);

                MultipartFileData fileData = provider.FileData.Single();
                string fileName = fileData.Headers.ContentDisposition.FileName.Replace("\"", "");
                string newFIleName = Path.Combine(Path.GetDirectoryName(fileData.LocalFileName), fileName);
                File.Move(fileData.LocalFileName, newFIleName);

                CurrentSite.Updator.UpdatePluginOnly(newFIleName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}