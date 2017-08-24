using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace TqCcnetDashboard
{
    public class ModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            bindingContext.ModelMetadata.ConvertEmptyStringToNull = false;
            if (IsPayloadRequest(controllerContext.HttpContext.Request)
                && IsClass(bindingContext.ModelMetadata.ModelType)
                )
            {
                return JsonBindModel(controllerContext.HttpContext.Request.InputStream, bindingContext);
            }

            return base.BindModel(controllerContext, bindingContext);
        }

        private bool IsPayloadRequest(HttpRequestBase Request)
        {
            return Request.ContentType.ToLower().Contains("json");
        }

        private bool IsClass(Type type)
        {
            return type.IsClass
                && !type.Equals(typeof(string))
                && !type.IsValueType
                && !type.IsEnum
                && !type.IsPrimitive;
        }

        private bool IsJObjectReturnType(Type modelType)
        {
            return modelType.Equals(typeof(Object)) || modelType.Equals(typeof(JObject));
        }

        private object JsonBindModel(Stream stream, ModelBindingContext bindingContext)
        {
            stream.Seek(0, SeekOrigin.Begin); // Payload 스트림의 포지션을 최초로 되감는다.

            using (var sr = new StreamReader(stream, System.Text.Encoding.UTF8, false, 1024, leaveOpen: true))
            {
                string data = sr.ReadToEnd();
                try
                {
                    var jObject = JObject.Parse(data);
                    // 파라메터에 해당되는 값
                    var modelValue = jObject.GetValue(bindingContext.ModelName, StringComparison.OrdinalIgnoreCase);
                    // 파라메터에 해당되는 값이 있는경우
                    if (modelValue != null)
                    {
                        // 해당 이름의 값으로 역직렬화 한다.
                        return JsonConvert.DeserializeObject(modelValue.ToString(), bindingContext.ModelType);
                    }
                    else
                    {
                        // 전체 값으로 역직렬화 한다.
                        return JsonConvert.DeserializeObject(jObject.ToString(), bindingContext.ModelType);
                    }
                }
                catch
                {
                    if (IsJObjectReturnType(bindingContext.ModelType)) return new JObject();
                    return Activator.CreateInstance(bindingContext.ModelType);
                }
            }
        }
    }
}