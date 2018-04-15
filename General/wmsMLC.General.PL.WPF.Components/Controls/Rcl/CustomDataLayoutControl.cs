using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomDataLayoutControl : LayoutControl
    {
        private string _layout;

        public bool DoNotUseChildrenLayout { get; set; }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            if (_layout != null)
            {
                RestoreLayoutBase(_layout);
                _layout = null;
            }
        }

        protected override FrameworkElement FindByXMLID(string id)
        {
            var result = base.FindByXMLID(id);
            return result ?? Children.Cast<FrameworkElement>().FirstOrDefault(p => p.Name == id);
        }

        public void RestoreLayout(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                return;

            if (IsLoaded)
            {
                RestoreLayoutBase(layout);
            }
            else
            {
                _layout = layout;
            }
        }

        private void RestoreLayoutBase(string layout)
        {
            if (string.IsNullOrEmpty(layout))
                return;

            using (var sr = new StringReader(layout))
            {
                using (var wr = XmlReader.Create(sr))
                {
                    ReadFromXML(wr);
                }
            }
        }

        public string SaveLayout()
        {
            var sb = new StringBuilder();
            using (var wr = XmlWriter.Create(sb))
            {
                WriteToXML(wr);
            }
            
            return sb.ToString();
        }

        protected override void OnReadElementFromXML(XmlReader xml, FrameworkElement element)
        {
            base.OnReadElementFromXML(xml, element);

            if (DoNotUseChildrenLayout)
                return;

            var layoutItem = element as LayoutItem;
            if (layoutItem == null || layoutItem.Content == null)
                return;

            foreach (var p in VisualTreeHelperExt.FindChildsByType<RclGridControl>(layoutItem.Content))
            {
                var key = CreateCustomizablePropertyName(layoutItem, p);
                var xmlcontent = xml.GetAttribute(key);
                if (string.IsNullOrEmpty(xmlcontent))
                    continue;

                p.RestoreLayoutFromString(xmlcontent);
            }

            foreach (var p in VisualTreeHelperExt.FindChildsByType<CustomComboBoxEditRcl>(layoutItem.Content))
            {
                var key = CreateCustomizablePropertyName(layoutItem, p);
                var xmlcontent = xml.GetAttribute(key);
                p.LayoutValue = xmlcontent;
            }
        }

        protected override void OnWriteElementToXML(XmlWriter xml, FrameworkElement element)
        {
            base.OnWriteElementToXML(xml, element);

            if (DoNotUseChildrenLayout)
                return;

            var layoutItem = element as LayoutItem;
            if (layoutItem == null || layoutItem.Content == null)
                return;

            foreach (var p in VisualTreeHelperExt.FindChildsByType<RclGridControl>(layoutItem.Content))
            {
                var xmlcontent = p.SaveLayoutToString();
                var key = CreateCustomizablePropertyName(layoutItem, p);
                xml.WriteAttributeString(key, xmlcontent);
            }

            foreach (var p in VisualTreeHelperExt.FindChildsByType<CustomComboBoxEditRcl>(layoutItem.Content))
            {
                if (!string.IsNullOrEmpty(p.LayoutValue))
                {
                    var key = CreateCustomizablePropertyName(layoutItem, p);
                    xml.WriteAttributeString(key, p.LayoutValue);
                }
            }
        }

        private static string CreateCustomizablePropertyName(IFrameworkInputElement child,  IFrameworkInputElement childElementContent)
        {
            if (child == null)
                throw new ArgumentNullException("child");
            if (childElementContent == null)
                throw new ArgumentNullException("childElementContent");

            var name = childElementContent.Name;
            if (string.IsNullOrEmpty(name))
                throw new Exception("Property Name of FrameworkElement childElementContent should be defined when used method CreateCustomizablePropertyName.");

            return string.Format("{0}_{1}_{2}", child.Name, childElementContent.GetType().Name, name);
        }
    }
}
