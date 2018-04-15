using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Editors;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.ViewModels;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Main.Views.Controls;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Content.Views.Controls
{
    public class PartnerWithAddressLookupEdit : CustomLookUpEdit
    {
        public string PartnerIdPropertyName { get; set; }
        
        public string PartnerNamePropertyName { get; set; }
        
        public string AddressBookIdPropertyName { get; set; }

        protected override async Task<IEnumerable<object>> GetDataAsync()
        {
            var filter = GetDataFilter();
            if (IsSimpleMode && (CanUseVirtualField() || string.IsNullOrEmpty(filter)))
                return null;

            return await Task.Factory.StartNew(() =>
            {
                var mgr = LookupHelper.GetItemSourceManager(LookupInfo);
                var result = Partner.SplitWithAddress(mgr.GetFiltered(filter).Cast<Partner>());
                return result;
            });
        }

        protected override CustomLookUpOptPopupContent CreateWindow()
        {
            var model = (IObjectListViewModel) new PartnerWithAddressListViewModel();
            model.Mode = ObjectListMode.LookUpList3Points;
            model.AllowAddNew = true;
            model.InitializeMenus();

            ((PanelViewModelBase)model).SetPanelCaptionPrefix(DataContext.GetType());
            ((PanelViewModelBase)model).IsActive = true;

            if (EditValue != null)
            {
                model.ValueMember = GetValueMember();
                model.EditValue = EditValue;
            }

            // выставляем ограничения на кол-во строк
            if (model.CustomFilters != null)
            {
                model.CustomFilters.MaxRowCount = MaxFetchItemsCount;
                model.CustomFilters.FilterExpression = "NONE";
                model.CustomFilters.SqlFilterExpression = FilterInternal;

                if (!string.IsNullOrEmpty(FilterInternal)) //Внимание. Метод зависит от наличия меню - model.InitializeMenus
                    model.ChangeImageFilter(false);
            }

            // если у нас уже нафильтровано какое-то кол-во значений - отдаем их в форму
            if (ItemsSource != null)
            {
                var items = new ObservableRangeCollection<Partner>();
                foreach (var item in ((IList)ItemsSource).OfType<Partner>())
                {
                    var obj = (Partner)item.Clone();
                    obj.Address = item.Address;
                    obj.AcceptChanges();
                    items.Add(obj);
                }
                model.SetSource(items);
            }
            else // иначе лезем в БД за данными для "..."
            {
                model.ApplyFilter();
            }

            var result = new CustomLookUpOptPopupContent
            {
                DataContext = model,
            };

            if (result.Owner == null && Application.Current.MainWindow.IsActive)
                result.Owner = Application.Current.MainWindow;
            return result;
        }

        protected override async void OnShowWindow()
        {
            if (!CanShowWindow())
                return;

            if (IsSimpleMode && (ItemsSource == null || (ItemsSource is IList && ((IList)ItemsSource).Count == 0)) &&
                EditValue != null)
            {
                await RefreshData(false);
            }

            using (var window = CreateWindow())
            {
                if (window.ShowDialog() == true)
                {
                    var model = window.DataContext as IObjectListViewModel;
                    if (model != null && model.SelectedItem != null)
                    {
                        var pd = TypeDescriptor.GetProperties(LookupInfo.ItemType);
                        var property = pd.Find(LookupInfo.ValueMember, true);
                        if (property != null)
                        {
                            // чтобы не получать еще раз данные - берем из из модели
                            InItemSourceChanging = true; //Значение InItemSourceChanging будет изменено на false в SetItemsSource
                            IsSimpleMode = false;
                            if (string.IsNullOrEmpty(DisplayMember) || string.IsNullOrEmpty(ValueMember))
                                SetLookupDisplyProperties();
                            //EditValue = property.GetValue(model.SelectedItem);
                            SetItemsSource(model.GetSource());
                            SelectedItem = model.SelectedItem;
                            SetProperties((Partner) model.SelectedItem);
                        }
                    }
                }
            }
        }

        //protected override void OnSelectedItemChanged(object oldValue, object newValue)
        //{
        //    base.OnSelectedItemChanged(oldValue, newValue);
        //    var sel = (Partner)newValue;
        //    SetProperties(sel);
        //}

        protected override object GetCorrectedEditValue(object editValue)
        {
            var bo = editValue as WMSBusinessObject;
            if (bo != null)
                return bo.GetProperty(LookupInfo.ValueMember);
            // если тип поля и тип ключа лукапа не совпадают. то сами приводим к нужному
            if (editValue != null && ValueMemberType != null && !ValueMemberType.IsInstanceOfType(editValue))
                return SerializationHelper.ConvertToTrueType(editValue, ValueMemberType);
            return editValue;
        }

        protected override void OnCustomEditValueChanged(object sender, EditValueChangedEventArgs e)
        {
        }

        private void SetProperties(Partner partner)
        {
            if (partner == null)
                return;
            var obj = DataContext as WMSBusinessObject;
            obj.SetProperty(PartnerNamePropertyName, partner.PartnerName);
            obj.SetProperty(PartnerIdPropertyName, partner.PartnerId);
            obj.SetProperty(AddressBookIdPropertyName, partner.Address != null && partner.Address.Any() ? partner.Address[0].ADDRESSBOOKID : null);
        }
    }
}
