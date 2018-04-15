using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.General.ViewModels.Menu;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Commands;
using wmsMLC.General.PL.WPF.Services;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View((typeof (CustomObjectListView)))]
    public class IndicateReasonForCorrectionViewModel : CustomObjectListViewModelBase<IwbPosWithCpvGrd>
    {
        public ICommand EditReasonCommand { get; set; }

        public IndicateReasonForCorrectionViewModel()
        {
            AllowEditing = true;
            AllowMultipleSelect = false;
            EditReasonCommand = new DelegateCustomCommand(this, OnEditReason, CustomCanEdit);
        }

        private void OnEditReason()
        {
            var model = new ExpandoObjectViewModelBase();
            var iwbPosWithCpvGrd = SelectedItems.FirstOrDefault();
            var viewService = IoC.Instance.Resolve<IViewService>();

            if (iwbPosWithCpvGrd == null)
            {
                viewService.ShowDialog("Ошибка", "Запись не выбрана", MessageBoxButton.OK, MessageBoxImage.Information,
                    MessageBoxResult.OK);
                return;
            }

            model.Fields = new ObservableCollection<ValueDataField>(new List<ValueDataField>()
            {
                new ValueDataField()
                {
                    Name = "MANDANTID",
                    SourceName = "MANDANTID",
                    FieldName = "MANDANTID",
                    FieldType = typeof (decimal),
                    Visible = false,
                    Value = iwbPosWithCpvGrd.MandantId
                },
                new ValueDataField()
                {
                    Name = "IWBPosAdjustmentReason",
                    SourceName = "IWBPosAdjustmentReason",
                    Caption = "Причина корректировки",
                    LookupCode = "ADJUSTMENTREASON_MANDANT",
                    LookupButtonEnabled = false,
                    FieldName = "IWBPosAdjustmentReason",
                    FieldType = typeof (string),
                    Value = iwbPosWithCpvGrd.IWBPosAdjustmentReason
                },
                new ValueDataField()
                {
                    Name = "IWBPosAdjustmentReasonDesc",
                    SourceName = "IWBPosAdjustmentReasonDesc",
                    Caption = "Описание причины",
                    FieldName = "IWBPosAdjustmentReasonDesc",
                    FieldType = typeof (string),
                    Value = iwbPosWithCpvGrd.IWBPosAdjustmentReasonDesc
                }
            });

            model.PanelCaption = "Причина корректировки";

            if (viewService.ShowDialogWindow(model, true, false, "40%", "15%") != true) return;

            if (iwbPosWithCpvGrd.IWBPosAdjustmentReason != model.Get<decimal?>("IWBPosAdjustmentReason"))
                iwbPosWithCpvGrd.IWBPosAdjustmentReason = model.Get<decimal?>("IWBPosAdjustmentReason");

            if (iwbPosWithCpvGrd.IWBPosAdjustmentReasonDesc != model.Get<string>("IWBPosAdjustmentReasonDesc"))
                iwbPosWithCpvGrd.IWBPosAdjustmentReasonDesc = model.Get<string>("IWBPosAdjustmentReasonDesc");
        }

        protected bool CustomCanEdit()
        {
            return true;
        }

        protected override void InitializeSettings()
        {
            MenuSuffix = GetType().GetFullNameWithoutVersion();
            base.InitializeSettings();
        }

        //protected override bool CanEdit()
        //{
        //    // не даем сработать по двойному клику
        //    return false;
        //}

        protected override ObservableCollection<DataField> GetDataFields()
        {
            var fields = new[]
            {
                new DataField
                {
                    FieldName = "IWBPosNum",
                    Caption = "Номер позиции прихода",
                    Description = "Номер позиции прихода",
                    FieldType = typeof (int),
                    Name = "IWBPosNum",
                    SourceName = "IWBPosNum",
                    IsEnabled = false,
                    Visible = true
                },
                new DataField
                {
                    FieldName = "SKUName",
                    Caption = "наименование SKU",
                    Description = "наименование SKU",
                    FieldType = typeof (string),
                    Name = "SKUName",
                    SourceName = "SKUName",
                    IsEnabled = false,
                    Visible = true
                },
                new DataField
                {
                    FieldName = "IWBPosCount",
                    Caption = "кол-во заявлено",
                    Description = "кол-во заявлено",
                    FieldType = typeof (decimal),
                    Name = "IWBPosCount",
                    SourceName = "IWBPosCount",
                    Visible = true,
                    IsEnabled = true
                },
                new DataField
                {
                    FieldName = "IWBPosProductCount",
                    Caption = "кол-во принято",
                    Description = "кол-во принято",
                    FieldType = typeof (double),
                    Name = "IWBPosProductCount",
                    SourceName = "IWBPosProductCount",
                    IsEnabled = false,
                    Visible = true
                },
                new DataField
                {
                    FieldName = "IWBPosAdjustmentReason",
                    Caption = "Причина корректировки",
                    Description = "Причина корректировки",
                    LookupCode = "ADJUSTMENTREASON_MANDANT",
                    LookupButtonEnabled = false,
                    FieldType = typeof (decimal),
                    Name = "IWBPosAdjustmentReason",
                    SourceName = "IWBPosAdjustmentReason",
                    IsEnabled = false,
                    Visible = true
                },
                new DataField
                {
                    FieldName = "IWBPosAdjustmentReasonDesc",
                    Caption = "Описание причины",
                    Description = "Описание причины",
                    FieldType = typeof (string),
                    Name = "IWBPosAdjustmentReasonDesc",
                    SourceName = "IWBPosAdjustmentReasonDesc",
                    IsEnabled = false,
                    Visible = true
                }
            };
            return new ObservableCollection<DataField>(fields);
        }

        protected override void CreateMainMenu()
        {
            var bar = Menu.GetOrCreateBarItem(StringResources.Edit, 1);
            bar.MenuItems.Add(new CommandMenuItem
            {
                Caption = StringResources.Edit,
                Command = EditReasonCommand,
                ImageSmall = ImageResources.DCLEdit16.GetBitmapImage(),
                ImageLarge = ImageResources.DCLEdit32.GetBitmapImage(),
                GlyphAlignment = GlyphAlignmentType.Top,
                DisplayMode = DisplayModeType.Default,
                Priority = 1
            });

            bar.MenuItems.Add(new SeparatorMenuItem {Priority = 2});
        }

        protected override void Edit()
        {
            OnEditReason();
        }
    }

    public class IwbPosWithCpvGrd : EditableBusinessObject
    {
        #region .  Properties  .
        private decimal? _adjReason;
        private string _adjReasonDesc;
        public decimal IWBPosNum { get; set; }
        public decimal? MandantId { get; set; }

        public decimal? IwbPosId { get; set; }
        public string SKUName { get; set; }
        public decimal? IWBPosCount { get; set; }
        public double IWBPosProductCount { get; set; }
        public decimal? IWBPosAdjustmentReason
        {
            get { return _adjReason; }
            set
            {
                _adjReason = value;
                OnPropertyChanged("IWBPosAdjustmentReason");
            }
        }
        public string IWBPosAdjustmentReasonDesc
        {
            get { return _adjReasonDesc; }
            set
            {
                _adjReasonDesc = value;
                OnPropertyChanged("IWBPosAdjustmentReasonDesc");
            }
        }
        #endregion
        
        public IwbPosWithCpvGrd(IWBPos iwbpos, decimal? iwbPosAdjustmentReason, string iwbPosAdjustmentReasonDesc)
        {
            IwbPosId = iwbpos.IWBPosID;
            IWBPosNum = iwbpos.IWBPosNumber;
            SKUName = iwbpos.VSkuName;
            IWBPosCount = iwbpos.IWBPosCount;
            IWBPosProductCount = iwbpos.IWBPosProductCount;
            IWBPosAdjustmentReason = iwbPosAdjustmentReason;
            MandantId = iwbpos.MandantID;
            IWBPosAdjustmentReasonDesc = iwbPosAdjustmentReasonDesc;
        }
    }
}
