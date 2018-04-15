using System;
using System.Linq;
using System.Collections.ObjectModel;
using wmsMLC.Activities.ViewInteraction.Views;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.ViewModels;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.Activities.ViewInteraction.ViewModels
{
    [View(typeof(OwbProxyView))]
    public class OwbProxyViewModel : CustomObjectListViewModelBase<OwbProxy>
    {
        public const string ProxyCodePropertyName = "ProxyCode";
        public const string ProxyDatePropertyName = "ProxyDate";

        public string ProxyCodeCaption { get; set; }
        public string ProxyDateCaption { get; set; }

        private string _proxyCode;
        public string ProxyCode {
            get { return _proxyCode; }
            set
            {
                _proxyCode = value;
                OnPropertyChanged(ProxyCodePropertyName);
            }
        }

        private DateTime? _proxyDate;
        public DateTime? ProxyDate {
            get { return _proxyDate; }
            set
            {
                _proxyDate = value;
                OnPropertyChanged(ProxyDatePropertyName);
            }
        }

        private bool _selectionChange;
        public bool SelectionChange {
            get { return _selectionChange; }
            set
            {
                _selectionChange = value;
                if(Source != null)
                    foreach (var s in Source)
                        s.Checked = _selectionChange;
            }
        }

        public string SelectionChangeCaption { get; set; }

        protected override void CreateMainMenu()
        {
            //STUB
        }

        public OwbProxyViewModel()
        {
            IsMenuEnable = true;
            IsCustomization = true;
            AllowClosePanel = true;
            PanelCaptionImage = ImageResources.DCLDefault16.GetBitmapImage();
            SelectionChangeCaption = "Выбрать все";
            ProxyCodeCaption = "Номер договора";
            ProxyDateCaption = "Дата договора";
        }

        protected override ObservableCollection<DataField> GetDataFields()
        {
            return GetFieldsInternal();
        }

        private ObservableCollection<DataField> GetFieldsInternal()
        {
            var result = new ObservableCollection<DataField>();
            result.Add(new DataField
            {
                Caption = "*",
                Description = "Выбор записи для изменения",
                FieldName = "Checked",
                Name = "Checked",
                SourceName = "Checked",
                FieldType = typeof(bool),
                EnableEdit = true,
                IsEnabled = true,
                Visible = true
            });
            result.Add(new DataField
            {
                Caption = "Наименование",
                Description = "Наименование расходной накладной",
                FieldName = "OwbName",
                Name = "OwbName",
                SourceName = "OwbName",
                FieldType = typeof(string),
                EnableEdit = false,
                Visible = true
            });
            result.Add(new DataField
            {
                Caption = "Рейс",
                Description = "Рейс",
                FieldName = "Traffic",
                Name = "Traffic",
                SourceName = "Traffic",
                FieldType = typeof(string),
                EnableEdit = false,
                Visible = true
            });
            result.Add(new DataField
            {
                Caption = "Водитель",
                Description = "Водитель",
                FieldName = "Driver",
                Name = "Driver",
                SourceName = "Driver",
                FieldType = typeof(string),
                EnableEdit = false,
                Visible = true
            });
            result.Add(new DataField
            {
                Caption = "Номер доверенности",
                Description = "Номер доверенности",
                FieldName = "ProxyCode",
                Name = "ProxyCode",
                SourceName = "ProxyCode",
                FieldType = typeof(string),
                EnableEdit = false,
                Visible = true
            });
            result.Add(new DataField
            {
                Caption = "Дата доверенности",
                Description = "Дата доверенности",
                FieldName = "ProxyDate",
                Name = "ProxyDate",
                SourceName = "ProxyDate",
                FieldType = typeof(DateTime?),
                EnableEdit = false,
                Visible = true
            });
            return result;
        }
    }

    public class OwbProxy : ViewModelBase
    {
        public const string CheckedPropertyName = "Checked";

        private decimal _id;
        private bool _checked;
        
        public bool Checked 
        {
            get { return _checked; }
            set
            {
                _checked = value;
                OnPropertyChanged(CheckedPropertyName);
            }
        }
        public string OwbName { get; set; }
        public string Traffic { get; set; }
        public string Driver { get; set; }
        public string ProxyCode { get; set; }
        public DateTime? ProxyDate { get; set; }

        public OwbProxy(decimal id)
        {
            _id = id;
        }

        public decimal GetId()
        {
            return _id;
        }
    }
}
