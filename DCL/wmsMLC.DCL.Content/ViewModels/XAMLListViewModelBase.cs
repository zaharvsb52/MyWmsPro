using System.Linq;
using System.Xml;
using wmsMLC.Business.Managers;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.General;
using wmsMLC.General.DAL;

namespace wmsMLC.DCL.Content.ViewModels
{
    public class XAMLListViewModelBase<T> : ObjectListViewModelBase<T>
    {
        protected string TagName = "XAMLBODY";
        
        protected override object DownloadXml(XmlDocument xmlDoc, IUnitOfWork uow, ref bool? isInsert)
        {
            var mgr = IoC.Instance.Resolve<IXamlManager<T>>();
            mgr.SetUnitOfWork(uow);
            var xaml = string.Empty;
            if (xmlDoc.DocumentElement != null)
            {
                var cn = xmlDoc.DocumentElement.ChildNodes;
                foreach (var n in from XmlNode n in cn where n.Name.Equals(TagName) select n)
                {
                    xaml = n.InnerText;
                    xmlDoc.DocumentElement.RemoveChild(n);
                    break;
                }
            }
            var obj = base.DownloadXml(xmlDoc, uow, ref isInsert);
            var entity = obj as WMSBusinessObject;
            if (entity != null)
                mgr.SetXaml(entity.GetKey().ToString(), xaml);
            return obj;
        }

        protected override XmlDocument GetXmlDocument(WMSBusinessObject item)
        {
            var mgr = IoC.Instance.Resolve<IXamlManager<T>>();
            var xaml = mgr.GetXaml(item.GetKey());
            var xmlDocument = XmlDocumentConverter.ConvertFrom(item);

            if (string.IsNullOrEmpty(xaml)) 
                return xmlDocument;

            var el = xmlDocument.CreateElement(TagName);
            el.InnerText = xaml;
            if (xmlDocument.DocumentElement != null)
                xmlDocument.DocumentElement.AppendChild(el);
            return xmlDocument;
        }

    }
}