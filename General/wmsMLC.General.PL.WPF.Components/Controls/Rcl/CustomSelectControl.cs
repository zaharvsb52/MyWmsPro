using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using log4net;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.General.PL.WPF.Components.Controls.Rcl
{
    public class CustomSelectControl : CustomSelectControlBase
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (CustomSelectControl));

        #region .  Constants & Fields  .
        #endregion .  Constants & Fields  .

        public CustomSelectControl()
        {
            Loaded += OnLoaded;
        }

        #region . Properties .
        public string LookUpCodeEditor { get; set; }
        public string LookUpCodeEditorFilterExt { get; set; }

        /// <summary>
        /// Составной DispalyMember. Формат: 'FORMAT(C# формат,имя свойства1,имя свойства2,...)'. 
        /// </summary>
        public string CustomDisplayMember { get; set; }
        #endregion . Properties .

        #region . Methods .
        private List<SelectListItem> PrepareItemSource()
        {
            string valueMember;
            string displayMember;
            IList itemsSource;

            if (string.IsNullOrEmpty(LookUpCodeEditor))
            {
                valueMember = ValueMember;
                displayMember = DisplayMember;
                itemsSource = ExternalItemsSource;
            }
            else //Определен лукап
            {
                var lookupInfo = LookupHelper.GetLookupInfo(LookUpCodeEditor);
                var filtertxt = lookupInfo.Filter;
                string filter0;
                LookupHelper.InitializeVarFilter(filtertxt, out filter0);
                var filters = new List<string> {filter0, LookUpCodeEditorFilterExt};
                var filter = string.Join(" AND ", filters.Where(p => !string.IsNullOrEmpty(p)).Select(p => p));

                using (var managerInstance = LookupHelper.GetItemSourceManager(lookupInfo))
                    itemsSource = managerInstance.GetFiltered(filter, GetModeEnum.Partial).ToArray();

                valueMember = lookupInfo.ValueMember;
                displayMember = lookupInfo.DisplayMember;
            }

            return PrepareItemSource(valueMember: valueMember, displayMember: displayMember, itemsSource: itemsSource);
        }

        private List<SelectListItem> PrepareItemSource(string valueMember, string displayMember, ICollection itemsSource)
        {
            const string rownumvariable = "rownumvariable";
            var result = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(valueMember) && !string.IsNullOrEmpty(displayMember) && itemsSource != null && itemsSource.Count > 0)
            {
                PropertyDescriptorCollection prdesc = null;
                PropertyDescriptor idProp = null;
                PropertyDescriptor displayMemberProp = null;

                CalcEngine.CalcEngine engine = null;
                var customDisplayMember = CustomDisplayMember;
                if (!string.IsNullOrEmpty(CustomDisplayMember))
                {
                    engine = new CalcEngine.CalcEngine();
                    if (customDisplayMember.Contains(ValueDataFieldConstants.RowNumberFlag))
                    {
                        customDisplayMember = CustomDisplayMember.Replace(ValueDataFieldConstants.RowNumberFlag, rownumvariable);
                        if (!engine.Variables.ContainsKey(rownumvariable))
                            engine.Variables.Add(rownumvariable, 0);
                    }
                }

                var id = 0;
                foreach (var d in itemsSource)
                {
                    if (prdesc == null)
                        prdesc = TypeDescriptor.GetProperties(d);
                    if (idProp == null)
                        idProp = prdesc.Find(valueMember, true);
                    if (displayMemberProp == null)
                        displayMemberProp = prdesc.Find(displayMember, true);

                    var item = new SelectViewItem { Id = idProp.GetValue(d) };

                    if (engine != null)
                    {
                        try
                        {
                            if (engine.Variables.ContainsKey(rownumvariable))
                                engine.Variables[rownumvariable] = id + 1;
                            engine.DataContext = d;
                            item.Name = engine.Evaluate(customDisplayMember).To<string>();
                        }
                        catch (Exception ex)
                        {
                            _log.WarnFormat("При попытке получить описание по CustomDisplayMember='{0}' возникла ошибка: {1}", CustomDisplayMember, ExceptionHelper.ExceptionToString(ex));
                            _log.Debug(ex);
                            // прописываем текст из DisplayMember
                            item.Name = displayMemberProp.GetValue(d).To<string>();
                        }
                    }
                    else
                        item.Name = displayMemberProp.GetValue(d).To<string>();

                    var listItem = new SelectListItem { Id = id++, Value = item };
                    result.Add(listItem);
                }
            }
            return result;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            ItemsSource = PrepareItemSource();
            if (TotalRowCount() == 0)
                UpdateEditValue();
        }
        #endregion . Methods .
    }
}
