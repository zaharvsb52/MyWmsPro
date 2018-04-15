using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectListView))]
    public class PartnerListViewModel : ObjectListViewModelBase<Partner>
    {
        protected override ObservableCollection<DataField> GetFields(Type type, wmsMLC.General.PL.SettingDisplay settings)
        {
            //if(Mode == ObjectListMode.LookUpList)
            //    return new ObservableCollection<DataField>(base.GetFields(type, settings).Where(i => (new[] { Partner.PARTNERIDPropertyName, Partner.PARTNERCODEPropertyName, Partner.MANDANTIDPropertyName, Partner.PARTNERNAMEPropertyName, Partner.VADDRESSBOOKCOMPLEXPropertyName }).Contains(i.SourceName)));
            return base.GetFields(type, settings);
        }
    }
}
