using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.Content.Views.Controls;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.DCL.Resources;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof (ObjectView))]
    public class OWBViewModel : ObjectViewModelBase<OWB>, IFieldProvider
    {
        private List<Employee2OWB> Employee2OwbDeleteList = new List<Employee2OWB>();
        protected override void SourceObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.SourceObjectPropertyChanged(sender, e);

            if (Source == null)
                return;

            var editable = Source as IEditable;
            if (editable.IsInRejectChanges)
                return;

            if (!e.PropertyName.EqIgnoreCase(OWB.OWBRECIPIENTPropertyName) && 
                !e.PropertyName.EqIgnoreCase(OWB.MANDANTIDPropertyName))
                return;

            if (e.PropertyName.EqIgnoreCase(OWB.MANDANTIDPropertyName))
            {
                if (Source.IsNew)
                    Source.Owner = Source.MandantID;
                return;
            }

            if (e.PropertyName.EqIgnoreCase(OWB.OWBRECIPIENTPropertyName))
            {
                //запомним список контактов, которые надо удалить
                if (Source.Employee2OwbL != null && Source.Employee2OwbL.Any())
                    //получаем только контакты с ключом (сохраненные в БД)
                    Employee2OwbDeleteList.AddRange(Source.Employee2OwbL.Where(i => i.GetKey() != null));
                //чистим список контактов
                Source.Employee2OwbL = new WMSBusinessCollection<Employee2OWB>();
                //если партнер не указан, то чистим адрес
                if (Source.OWBRecipient == null)
                {
                    Source.AddressBookID = null;
                }
                //если партнер был указан, то получим его контакты и добавим в список
                if (Source.OWBRecipient != null && Source.OWBRecipient.Value != 0)
                {
                    using (var mgr = IoC.Instance.Resolve<IBaseManager<Partner>>())
                    {
                        var partner = mgr.Get(Source.OWBRecipient);
                        var empl = partner.EmployeeL ?? new WMSBusinessCollection<Employee>();
                        foreach (var emp in empl)
                        {
                            //добавляем только контакты по-умолчанию
                            if (!emp.GetProperty<bool>(Employee.EMPLOYEEISDEFAULTPropertyName))
                                continue;
                            Source.Employee2OwbL.Add(new Employee2OWB() 
                            {
                                EMPLOYEE2OWBOWBID = Source.IsNew? -1 : Source.GetKey<decimal?>(),
                                EMPLOYEE2OWBEMPLOYEEID = emp.GetKey<decimal?>(),
                                VEMPLOYEE = emp.VEMPLOYEEFIO,
                                VOWBNAME = Source.OWBName
                            });
                        }
                    }
                }
            }
            //var filter = string.Format("ADDRESSBOOKID in (select p2a.ADDRESSBOOKID_R from wmspartner2addressbook p2a where p2a.PARTNERID_R={0})", Source.OWBRecipient);
            //using (var mgr = IoC.Instance.Resolve<IBaseManager<AddressBook>>())
            //{
            //    // запрашиваем адреса
            //    var address = mgr.GetFiltered(filter);
            //    if (address == null || !address.Any())
            //    {
            //        // если было значение - сбрасываемся
            //        if (Source.AddressBookID != null)
            //            Source.AddressBookID = null;
            //        return;
            //    }

            //    // ищем физический адрес с минимальным id
            //    var addrArray = address.ToArray();
            //    var physicalAddr = address
            //        .Where(a => "ADR_PHYSICAL".Equals(a.ADDRESSBOOKTYPECODE))
            //        .OrderBy(c => c.ADDRESSBOOKID)
            //        .FirstOrDefault();

            //    // если есть физический - проставляем его, иначе - первый
            //    var newId = physicalAddr != null
            //        ? physicalAddr.ADDRESSBOOKID
            //        : addrArray.OrderBy(c => c.ADDRESSBOOKID).First().ADDRESSBOOKID;

            //    if (newId != Source.AddressBookID)
            //        Source.AddressBookID = newId;
            //}
        }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);
            var recipientField = fields.FirstOrDefault(f => f.SourceName == OWB.OWBRECIPIENTPropertyName);
            if (recipientField != null)
                recipientField.IsEnabled = false;
            return fields;
        }

        protected override bool Save()
        {
            //получаем только новые записи
            var newEmployee2OwbList = Source.Employee2OwbL != null ? new List<Employee2OWB>(Source.Employee2OwbL.Where(i => i.IsNew)).Cast<Employee2OWB>() : null;
            
            if (!base.Save())
                return false;

            try
            {
                WaitStart();
                using (var mgr = IoC.Instance.Resolve<IBaseManager<Employee2OWB>>())
                {
                    //удаляем старые записи
                    if (Employee2OwbDeleteList != null && Employee2OwbDeleteList.Any())
                    {
                        mgr.Delete(Employee2OwbDeleteList);
                        Employee2OwbDeleteList.Clear();
                    }
                    if (newEmployee2OwbList != null && newEmployee2OwbList.Any())
                    {
                        //проставляем ID накладной, на случай, если накладная была новая
                        foreach (var e2O in newEmployee2OwbList)
                        {
                            e2O.EMPLOYEE2OWBOWBID = Source.GetKey<decimal?>();
                        }
                        mgr.Insert(ref newEmployee2OwbList);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                if (!ExceptionHandler(ex, ExceptionResources.ItemCantSave))
                    throw;
                return false;
            }
            finally 
            {
                RefreshData(false);
                WaitStop();
            }
        }

        #region .  IFieldProvider  .
        public FrameworkElement GetElement(DataField field)
        {
            if (field.SourceName == OWB.OWBRECIPIENTPropertyName)
                return new PartnerWithAddressLookupEdit() { PartnerIdPropertyName = OWB.OWBRECIPIENTPropertyName, PartnerNamePropertyName = OWB.OWBRECIPIENT_NAMEPropertyName, AddressBookIdPropertyName = OWB.ADDRESSBOOKID_RPropertyName, Name = "clue" + field.Name, LookUpCodeEditorVarFilterExt = field.LookupVarFilterExt, LookUpCodeEditorFilterExt = field.LookupFilterExt, ParentViewModelSource = this, LookUpCodeEditor = field.LookupCode, LookupButtonEnabled = field.LookupButtonEnabled };
            return null;
        }
        #endregion
    }
}