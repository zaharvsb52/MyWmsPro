using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using DevExpress.Xpf.LayoutControl;
using wmsMLC.DCL.Main.Helpers;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Main.Views.Controls
{
    public class CustomDataLayoutControl : LayoutControl, ISaveRestore, IDisposable
    {
        private bool _isfocusset;
        private bool _isLoaded;

        public bool DoNotSetFocusOnLoad { get; set; }
        public bool IsXmlLoading { get; private set; }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (_isLoaded)
                return;
            _isLoaded = true;

            if (DoNotSetFocusOnLoad || _isfocusset)
                return;

            if (Children == null || Children.Count <= 0)
                return;

            _isfocusset = true;
            var customDataLayoutItems = Children.OfType<CustomDataLayoutItem>().ToArray();
            if (customDataLayoutItems.Length > 0)
            {
                if (customDataLayoutItems.FirstOrDefault(p => p.SetFocus) != null) 
                    return;
                var element =
                    customDataLayoutItems.FirstOrDefault(p => !p.IsReadOnly && p.Visibility == Visibility.Visible) ??
                    customDataLayoutItems[0];
                if (element != null)
                {
                    if (element.Content == null)
                    {
                        element.BackgroundFocus();
                    }
                    else
                    {
                        element.Content.BackgroundFocus();
                    }
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (IsXmlLoading)
                return new Size();
            return base.MeasureOverride(availableSize);
        }

        protected override Size OnMeasure(Size availableSize)
        {
             if (IsXmlLoading)
                return new Size();
             return base.OnMeasure(availableSize);
        }

        public override void ReadFromXML(XmlReader xml)
        {
            try
            {
                IsXmlLoading = true;
                base.ReadFromXML(xml);
            }
            finally
            {
                IsXmlLoading = false;
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

        //public override void ReadFromXML(XmlReader xml)
        //{
        //    //base.ReadFromXML(xml);
        //    if (xml.IsStartElement(GetType().Name))
        //    {
        //        ReadFromXMLCore(xml);
        //    }
        //}

        //protected override FrameworkElement ReadChildFromXML(XmlReader xml, IList children, int index)
        //{
        //    var element = base.ReadChildFromXML(xml, children, index);

        //    //Минимизируем построение visual tree для LayoutGroup
        //    //https://www.devexpress.com/Support/Center/Question/Details/Q493185
        //    var group = element as LayoutGroup;
        //    if (group != null && !group.MeasureSelectedTabChildOnly)
        //        group.MeasureSelectedTabChildOnly = true;

        //    return element;
        //}

        protected override void AddChildFromXML(IList children, FrameworkElement element, int index)
        {
            //Минимизируем построение visual tree для LayoutGroup
            //https://www.devexpress.com/Support/Center/Question/Details/Q493185
            var group = element as LayoutGroup;
            if (group != null && !group.MeasureSelectedTabChildOnly)
                group.MeasureSelectedTabChildOnly = true;
            base.AddChildFromXML(children, element, index);
        }

        public void Dispose()
        {
            // прибиваем вложенные контролы
            var children = Children.OfType<IDisposable>();
            foreach (var child in children)
                child.Dispose();
        }
    }
}
