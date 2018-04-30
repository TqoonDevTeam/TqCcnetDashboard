using Exortech.NetReflector;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Tasks;

namespace TqLib.ccnet.Core.Publishers
{
    [ReflectorType("LineMessage", Description = "LineMessage")]
    public class LineMessagePublisher : TaskBase
    {
        [ReflectorProperty("ApiUrl", Required = false)]
        public string ApiUrl { get; set; } = "https://notify-api.line.me/api/notify";

        [ReflectorProperty("Token")]
        public string Token { get; set; }

        [DataType(DataType.MultilineText)]
        [ReflectorProperty("Message", Required = false)]
        public string Message { get; set; } = string.Empty;

        [ReflectorProperty("ImageThumbnail", Required = false)]
        public string ImageThumbnail { get; set; } = string.Empty;

        [ReflectorProperty("ImageFullsize", Required = false)]
        public string ImageFullsize { get; set; } = string.Empty;

        [ReflectorProperty("ImageFile", Required = false)]
        public string ImageFile { get; set; } = string.Empty;

        [ReflectorProperty("StickerPackageId", Required = false)]
        public int StickerPackageId { get; set; }

        [ReflectorProperty("StickerId", Required = false)]
        public int StickerId { get; set; }

        [ReflectorProperty("MessageTemplate", Required = false)]
        public string MessageTemplate { get; set; } = string.Empty;

        protected override bool Execute(IIntegrationResult result)
        {
            string message = GetMessage(result);

            using (var clinet = new HttpClient())
            using (var content = GetContent(result, message))
            {
                clinet.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
                clinet.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));

                var response = clinet.PostAsync(ApiUrl, content).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                if (!response.IsSuccessStatusCode)
                {
                    result.AddTaskResult($"[LineMessagePublisher] {responseString}");
                    return false;
                }
            }
            return true;
        }

        private HttpContent GetContent(IIntegrationResult result, string message)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(message), "message");
            if (!string.IsNullOrEmpty(ImageThumbnail)) content.Add(new StringContent(ImageThumbnail), "imageThumbnail");
            if (!string.IsNullOrEmpty(ImageFullsize)) content.Add(new StringContent(ImageFullsize), "imageFullsize");
            if (!string.IsNullOrEmpty(ImageFile))
            {
                var file = new FileInfo(ImageFile);
                if (file.Exists)
                {
                    var bytes = File.ReadAllBytes(file.FullName);
                    content.Add(new ByteArrayContent(bytes), "imageFile", file.Name);
                }
            }
            if (StickerPackageId > 0 && StickerId > 0)
            {
                content.Add(new StringContent(StickerPackageId.ToString()), "stickerPackageId");
                content.Add(new StringContent(StickerId.ToString()), "stickerId");
            }
            return content;
        }

        private string GetMessage(IIntegrationResult result)
        {
            StringBuilder msg = new StringBuilder(Message);
            msg.AppendLine();

            switch (MessageTemplate)
            {
                case "TqGitCI":
                    if (result.Succeeded)
                    {
                        msg.AppendLine($"CI Success");
                        return string.Empty;
                    }
                    else
                    {
                        msg.AppendLine($"CI Failure");

                        var errMsg = result.GetParameters("$TqGitCI_mergeResult") ?? string.Empty;
                        if (!string.IsNullOrEmpty(errMsg)) msg.AppendLine(errMsg);
                        errMsg = result.GetParameters("$TqGitCI_mergeExceptionMessage") ?? string.Empty;
                        if (!string.IsNullOrEmpty(errMsg)) msg.AppendLine(errMsg);
                        msg.Append(GetFailureTaskMessage(result));
                        return msg.ToString();
                    }

                case "TqBuild":
                    if (result.Succeeded)
                    {
                        msg.AppendLine($"Build And Deploy Success");
                        return msg.ToString();
                    }
                    else
                    {
                        msg.AppendLine($"Build And Deploy Failure");
                        msg.Append(GetFailureTaskMessage(result));
                        return string.Empty;
                    }

                default:
                    msg.Append(GetFailureTaskMessage(result));
                    return msg.ToString();
            }
        }

        private string GetFailureTaskMessage(IIntegrationResult result)
        {
            if (result.Succeeded) return string.Empty;

            StringBuilder msg = new StringBuilder();
            msg.AppendLine("FailureTasks");
            foreach (var task in result.FailureTasks)
            {
                msg.AppendLine(task.ToString());
            }
            if (result.ExceptionResult != null) msg.AppendLine(result.ExceptionResult.Message);
            return msg.ToString();
        }
    }
}