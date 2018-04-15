using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;
using log4net;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.DAL;

namespace wmsMLC.APS.wmsWebAPI.Controllers
{
    public class BaseController : ApiController
    {
        protected ILog Log;
        public BaseController()
        {
            Log = LogManager.GetLogger(GetType());
        }

        protected T GetFromRequestBody<T>() where T : class
        {
            // TODO: use binding
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Request.Content.ReadAsStringAsync().Result);
            if (xmlDoc.DocumentElement == null)
                Request.CreateResponse(HttpStatusCode.BadRequest);
            return XmlDocumentConverter.ConvertTo<T>(xmlDoc);
        }

        protected UnitOfWorkContext GetUnitOfWorkContext()
        {
            var userSignature = HttpContext.Current.User.Identity.Name;
            return new UnitOfWorkContext { UserSignature = userSignature };
        }

        protected XmlDocument SingleEntityResult<TEnt>(TEnt obj) 
            where TEnt : WMSBusinessObject, new()
        {
            return Serialize(obj);
        }

        protected XmlDocument Serialize(object obj)
        {
            // TODO: Delegate to serialization to XmlMediaTypeFormatter
            return XmlDocumentConverter.ConvertFrom(obj);
        }

        protected XmlDocument EntityListResult<TEnt>(IEnumerable<TEnt> objList)
            where TEnt : WMSBusinessObject, new()
        {
            // TODO: Delegate to serialization to XmlMediaTypeFormatter
            var xmlList = XmlDocumentConverter.ConvertFromListOf(objList);
            var resultXmlDocList = new XmlDocument();
            resultXmlDocList.LoadXml("<ITEMS></ITEMS>");
            var root = resultXmlDocList.DocumentElement;
            if (root != null)
            {
                foreach (var doc in xmlList)
                {
                    if (doc.DocumentElement != null)
                        root.InnerXml += doc.InnerXml;
                }
            }
            return resultXmlDocList;
        }

        protected object GetTrueKeyValue<TEnt>(string id, IBaseManager<TEnt> mgr)
            where TEnt : WMSBusinessObject, new()
        {
            var tmp = (WMSBusinessObject)mgr.New();
            var keyName = tmp.GetPrimaryKeyPropertyName();
            var descriptor =
                TypeDescriptor.GetProperties(tmp.GetType())
                    .Cast<PropertyDescriptor>()
                    .FirstOrDefault(i => i.Name.EqIgnoreCase(keyName));

            if (descriptor == null)
                return null;
            var type = descriptor.PropertyType;
            return SerializationHelper.ConvertToTrueType(id, type);
        }

        protected XmlDocument GetRequestDocument()
        {
            // TODO: use binding
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(Request.Content.ReadAsStringAsync().Result);
            if (xmlDoc.DocumentElement == null)
                Request.CreateResponse(HttpStatusCode.BadRequest);
            return xmlDoc;
        }
    }
}