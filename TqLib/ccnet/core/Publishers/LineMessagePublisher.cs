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

        [DataType("Select")]
        [ReflectorProperty("SendCondition", Required = false)]
        public SendConditions SendCondition { get; set; } = SendConditions.FALSE;

        [ReflectorProperty("AppendTaskResult", Required = false)]
        public bool AppendTaskResult { get; set; } = false;

        [ReflectorProperty("TaskResultFilter", Required = false)]
        public string TaskResultFilter { get; set; } = string.Empty;

        protected override bool Execute(IIntegrationResult result)
        {
            if (SendCondition == SendConditions.ALL || SendCondition.ToString() == result.Succeeded.ToString().ToUpper())
            {
                string message = GetMessage(result);
                if (!string.IsNullOrEmpty(message))
                {
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
            if (AppendTaskResult)
            {
                StringBuilder sb = new StringBuilder(Message);
                string msg;
                foreach (object taskResult in result.TaskResults)
                {
                    if (taskResult is ITaskResult)
                    {
                        msg = (taskResult as ITaskResult).Data;
                    }
                    else
                    {
                        msg = (taskResult as string);
                    }

                    if (msg.StartsWith(TaskResultFilter))
                    {
                        sb.AppendLine(msg);
                    }
                }
                return sb.ToString();
            }
            return Message;
        }

        public enum SendConditions
        {
            ALL, TRUE, FALSE
        }
    }
}