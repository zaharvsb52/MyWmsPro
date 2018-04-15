using System;
using System.IO;
using System.Xml;
using wmsMLC.General;
using wmsMLC.General.DAL.WebAPI;

namespace wmsMLC.Business.DAL.WebAPI
{
    public class IntegrationServiceImpl
    {
        private string _url;
        public IntegrationServiceImpl(string url)
        {
            _url = url;
        }

        public string ProcessApi(string action, string controller, object request)
        {
            if (string.IsNullOrEmpty(action))
                throw new ArgumentNullException("action");

            if (string.IsNullOrEmpty(controller))
                throw new ArgumentNullException("controller");

            var helper = new WebAPIHelper(_url);

            switch (action.ToUpper())
            {
                case "POST":
                    using (var ms = helper.Post<Stream>(controller, request))
                    {
                        if (ms != null)
                        {
                            using (var reader = XmlReader.Create(ms))
                            {
                                var responseXmlDoc = XmlDocumentConverter.ConvertTo<XmlDocument>(reader);
                                if (responseXmlDoc != null)
                                    return responseXmlDoc.InnerXml;
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException(string.Format("Action '{0}' is not Implemented", action));
            }

            return null;
        }
    }
}
