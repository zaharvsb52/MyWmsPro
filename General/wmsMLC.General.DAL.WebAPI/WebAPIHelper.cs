using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Xml;
using Microsoft.Practices.TransientFaultHandling;
using wmsMLC.Business.Objects;
using wmsMLC.General.DAL.WebAPI.Properties;

namespace wmsMLC.General.DAL.WebAPI
{
    public class WebAPIHelper
    {
        private class CustomWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest w = base.GetWebRequest(address);
                w.Timeout = 1000 * Settings.Default.RequestTimeOutInSeconds;
                return w;
            }
        }

        private class ErrorDetectionStrategy : ITransientErrorDetectionStrategy
        {
            public bool IsTransient(Exception ex)
            {
                return ex is WebException;
            }
        }

        private readonly string _baseUri;

        public WebAPIHelper() : this(Settings.Default.APIEndPoint)
        {
        }

        public WebAPIHelper(string baseUri)
        {
            if (string.IsNullOrWhiteSpace(baseUri))
                throw new ArgumentException("baseUri");
            _baseUri = baseUri;
        }

        public T Post<T>(string controller, object request)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(controller))
                throw new ArgumentException("controller");

            try
            {
                var policy = new RetryPolicy(new ErrorDetectionStrategy(),
                    new FixedInterval(Settings.Default.RequestRetryCount, TimeSpan.FromSeconds(Settings.Default.RequestRetryIntervalInSeconds)));
                return policy.ExecuteAction(() =>
                {
                    using (var wc = new CustomWebClient())
                    {
                        wc.BaseAddress = _baseUri;
                        wc.Headers[HttpRequestHeader.ContentType] = "text/xml";
                        wc.Credentials = WMSEnvironment.Instance.AuthenticatedUser.GetCredentials();
                        var buffer = Encoding.UTF8.GetBytes(XmlDocumentConverter.ConvertFrom(request).InnerXml);

                        var resp = wc.UploadData(controller, "POST", buffer);
                        if (typeof(WMSBusinessObject).IsAssignableFrom(typeof(T)))
                        {
                            using (var ms = new MemoryStream(resp))
                            using (var reader = XmlReader.Create(ms))
                            {
                                return XmlDocumentConverter.ConvertTo<T>(reader);
                            }
                        }
                        else if (typeof(T) == typeof(Stream))
                        {
                            return new MemoryStream(resp) as T;
                        }
                        else
                        {
                            return (T)Convert.ChangeType(resp, typeof(T));
                        }
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError) 
                    throw;

                var response = ex.Response as HttpWebResponse;
                if (response == null) 
                    throw;

                if (response.StatusCode != HttpStatusCode.InternalServerError &&
                    response.StatusCode != HttpStatusCode.Unauthorized) 
                    throw;

                using (var responseStream = response.GetResponseStream())
                using (var reader = XmlReader.Create(responseStream))
                {
                    try
                    {
                        var err = XmlDocumentConverter.ConvertTo<HttpError>(reader);
                        throw new Exception(err.ContainsKey("ExceptionMessage")
                            ? string.Format("{0}: {1}", err["ExceptionType"], err["ExceptionMessage"])
                            : err.Message);
                    }
                    catch (XmlException)
                    {
                        throw new Exception("Could not deserialize error response.");
                    }
                }
            }
        }

        public T Get<T>(string controller, string action, NameValueCollection parameters)
            where T : class
        {
            if (string.IsNullOrWhiteSpace(controller))
                throw new ArgumentException("controller");

            try
            {
                var policy = new RetryPolicy(new ErrorDetectionStrategy(),
                    new FixedInterval(Settings.Default.RequestRetryCount, TimeSpan.FromSeconds(Settings.Default.RequestRetryIntervalInSeconds)));

                return policy.ExecuteAction(() =>
                {
                    using (var wc = new CustomWebClient())
                    {
                        wc.BaseAddress = _baseUri;
                        wc.Headers[HttpRequestHeader.ContentType] = "text/xml";

                        // если пользователь не авторизован - пробуем под анонимусом
                        // такое возможно, если мы отрпавляем запросы еще до авторизации
                        wc.Credentials = WMSEnvironment.Instance.AuthenticatedUser == null
                            ? null
                            : WMSEnvironment.Instance.AuthenticatedUser.GetCredentials();

                        wc.QueryString = parameters;

                        //var buffer = Encoding.UTF8.GetBytes(XmlDocumentConverter.ConvertFrom(request).InnerXml);

                        var url = string.IsNullOrEmpty(action)
                            ? controller
                            : string.Format("{0}/{1}", controller, action);

                        var resp = wc.DownloadData(url);

                        if (typeof(T) == typeof(Stream))
                            return new MemoryStream(resp) as T;

                        using (var ms = new MemoryStream(resp))
                        using (var reader = XmlReader.Create(ms))
                        {
                            return XmlDocumentConverter.ConvertTo<T>(reader);
                        }
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError)
                    throw;

                var response = ex.Response as HttpWebResponse;
                if (response == null)
                    throw;

                if (response.StatusCode != HttpStatusCode.InternalServerError)
                    throw;

                using (var responseStream = response.GetResponseStream())
                using (var reader = XmlReader.Create(responseStream))
                {
                    try
                    {
                        var err = XmlDocumentConverter.ConvertTo<HttpError>(reader);
                        throw new Exception(err.ContainsKey("ExceptionMessage")
                            ? string.Format("{0}: {1}", err["ExceptionType"], err["ExceptionMessage"])
                            : err.Message);
                    }
                    catch (XmlException)
                    {
                        throw new Exception("Could not deserialize error response.");
                    }
                }
            }
        }
    }
}
