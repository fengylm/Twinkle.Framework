using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Twinkle.Framework.Extensions
{
    public sealed class TwinkleModelBinderProvider : IModelBinderProvider
    {
        internal TwinkleModelBinderProvider()
        {
        }
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (new Type[] { typeof(UploadModel), typeof(ClientModel) }.Contains(context.Metadata.ModelType))
            {
                return new ParameterModelBinder(context.Metadata.ModelType);
            }
            return null;
        }

        private class ParameterModelBinder : IModelBinder
        {
            public ParameterModelBinder(Type type)
            {
                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }
            }
            public Task BindModelAsync(ModelBindingContext bindingContext)
            {
                if (bindingContext == null) throw new ArgumentNullException("bindingContext");

                try
                {
                    if (bindingContext.ModelType == typeof(ClientModel))
                    {
                        return BuilderClientModel(bindingContext);
                    }
                    else if (bindingContext.ModelType == typeof(UploadModel))
                    {
                        return BuilderUploadModel(bindingContext);
                    }
                    return Task.CompletedTask;
                }
                catch (Exception exception)
                {
                    if (!(exception is FormatException) && (exception.InnerException != null))
                    {
                        exception = ExceptionDispatchInfo.Capture(exception.InnerException).SourceException;
                    }
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, exception, bindingContext.ModelMetadata);
                    return Task.CompletedTask;
                }
            }
            public Task BuilderClientModel(ModelBindingContext bindingContext)
            {
                bindingContext.Result = (ModelBindingResult.Success(new ClientModel(bindingContext)));
                return Task.CompletedTask;
            }
            public Task BuilderUploadModel(ModelBindingContext bindingContext)
            {
                ValueProviderResult result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (bindingContext.HttpContext.Request.Form.Files.Count > 0)
                {
                    JObject cusobj = new JObject();
                    foreach (var key in bindingContext.HttpContext.Request.Form.Keys)
                    {
                        cusobj.Add(key, bindingContext.HttpContext.Request.Form[key].ToString());
                    }
                    var file = bindingContext.HttpContext.Request.Form.Files[0];
                    var fileName = file.FileName.Substring(file.FileName.LastIndexOf(@"\") + 1);
                    bindingContext.Result = (ModelBindingResult.Success(new UploadModel
                    {
                        FileName = fileName.Substring(0, fileName.LastIndexOf(".")),
                        Length = file.Length,
                        FileStream = file.OpenReadStream(),
                        CustomData = cusobj,
                        Extension = fileName?.Substring(fileName.LastIndexOf("."))
                    }));
                    return Task.CompletedTask;
                }
                else
                {
                    bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, bindingContext.ModelMetadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(result.ToString()));
                    return Task.CompletedTask;
                }
            }
        }
    }
}
