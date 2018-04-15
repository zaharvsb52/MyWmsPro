using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.Helpers;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.BL.Helpers;
using wmsMLC.General.PL.Model;

namespace wmsMLC.DCL.Content.ViewModels
{
    public class PartnerWithAddressListViewModel : ObjectListViewModelBase<Partner>
    {
        protected override ObservableCollection<DataField> GetFields(Type type, wmsMLC.General.PL.SettingDisplay settings)
        {
            var fields = base.GetFields(type, settings).Where(i => (new[] { Partner.PARTNERIDPropertyName, Partner.PARTNERCODEPropertyName, Partner.MANDANTIDPropertyName, Partner.PARTNERNAMEPropertyName, Partner.VADDRESSBOOKCOMPLEXPropertyName }).Contains(i.SourceName));
            return new ObservableCollection<DataField>(fields);
        }

        protected override void CreateMainMenu()
        {
            base.CreateMainMenu();
            //удаляем меню Создать и Редактировать
            if (Menu != null && Menu.Bars != null)
            {
                foreach (var bar in Menu.Bars.Where(p => p != null && p.Caption.EqIgnoreCase(StringResources.Commands)).ToArray())
                {
                    foreach (var menu in bar.MenuItems.Where(p => p.Caption.EqIgnoreCase(StringResources.New) || p.Caption.EqIgnoreCase(StringResources.Edit)).ToArray())
                    {
                        bar.MenuItems.Remove(menu);
                    }
                }
            }
        }

        protected override async Task<IEnumerable<Partner>> GetFilteredDataAsync(string sqlFilter)
        {
            var now = DateTime.Now;
            return await Task.Factory.StartNew(() =>
            {
                using (var manager = GetManager())
                {
                    // перед получением очищаем данные - нам не нужен кэш при явном запросе журнала
                    if (IsNeedClearCache)
                        manager.ClearCache();
                    // получаем данные
                    var result = Partner.SplitWithAddress(manager.GetFiltered(sqlFilter));
                    TotalRowItemAdditionalInfo =
                        string.Format(StringResources.ListViewModelBaseTotalRowItemAdditionalInfo,
                            (DateTime.Now - now).TotalSeconds, manager.LastQueryExecutionTime);
                    return result;
                }
            });
        }

        protected override void Delete()
        {
            var errorMessage = new StringBuilder();
            try
            {
                WaitStart();

                if (!ConnectionManager.Instance.AllowRequest())
                    return;

                if (!DeleteConfirmation()) return;

                using (var mgr = GetManager())
                {
                    foreach (var p in SelectedItems)
                    {
                        var obj = mgr.Get(p.GetKey());
                        if (p.Address != null && p.Address.Any())
                        {
                            var address = p.Address[0];
                            if (!CanDeleteAddress(address))
                            {
                                //нельзя удалять
                                errorMessage.AppendLine(string.Format("Нельзя удалить адрес '{0}'.{1}Существуют записи в которых данный адрес используется.", address, Environment.NewLine));
                                continue;
                            }
                            var addr = obj.Address.FirstOrDefault(a => Equals(a.GetKey(), p.Address[0].GetKey()));
                            obj.Address.Remove(addr);
                            mgr.Update(obj);
                            //если остались адреса, то партнера не удаляем
                            if (obj.Address.Count > 0)
                                continue;
                        }
                        if (!CanDeletePartner(obj))
                        {
                            //нельзя удалять
                            errorMessage.AppendLine(string.Format("Нельзя удалить партнера '{0}'.{1}Существуют записи в которых данный партнер используется.", obj.PartnerName, Environment.NewLine));
                        }
                        else
                        {
                            mgr.Delete(obj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemsCantDelete))
                    throw;
            }
            finally
            {
                RefreshData();
                WaitStop();
                if (errorMessage.Length > 0)
                    ShowErrorMessage(errorMessage.ToString());
            }
        }

        private void ShowErrorMessage(string message)
        {
            var vs = GetViewService();
            var dr = vs.ShowDialog(StringResources.Error
                , message
                , MessageBoxButton.OK
                , MessageBoxImage.Warning
                , MessageBoxResult.OK);
        }

        private bool CanDeleteAddress(AddressBook address)
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<OWB>>())
            {
                //TODO: добавить еще проверки на сущностях (уточнить)
                var owbList = mgr.GetFiltered(string.Format("ADDRESSBOOKID_R = {0}", address.GetKey()), FilterHelper.GetAttrEntity<OWB>(new[] { OWB.OWBIDPropertyName }));
                if (owbList.Any())
                    return false;
            }
            return true;
        }

        private bool CanDeletePartner(Partner partner)
        {
            using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
            {
                //INFO: проверяем использование партнера в IWB и OWB
                //TODO: добавить еще проверки на сущностях (уточнить)
                var partnerList = mgr.GetFiltered(string.Format("partnerid in (select p.partnerid from wmspartner p where partnerid = {0} and" +
                    " (exists (select * from wmsiwb where partnerid_r = p.partnerid or iwbsender = p.partnerid or iwbrecipient = p.partnerid) or" +
                    " exists (select * from wmsowb where partnerid_r = p.partnerid or owbrecipient = p.partnerid)))", partner.GetKey()), GetModeEnum.Partial);
                if (partnerList.Any())
                    return false;
            }
            return true;
        }
    }
}
