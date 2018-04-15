using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class Employee2OWBViewModel : ObjectViewModelBase<Employee2OWB>
    {
        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);
            //TODO: должно быть Field.SourceName
            var employee = fields.FirstOrDefault(i => i.FieldName == Employee2OWB.EMPLOYEE2OWBEMPLOYEEIDPropertyName);
            if (employee != null)
                employee.LookupVarFilterExt = "[$OWBRECIPIENT, (null, 1=1), (*, PARTNERID_R={0} )]";
            return fields;
        }
    }
}
