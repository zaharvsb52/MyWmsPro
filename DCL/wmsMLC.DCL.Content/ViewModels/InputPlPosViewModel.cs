using System.Collections.ObjectModel;
using System.Linq;
using wmsMLC.Business.Objects;
using wmsMLC.DCL.General.ViewModels;
using wmsMLC.DCL.Main.Views;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Views;

namespace wmsMLC.DCL.Content.ViewModels
{
    [View(typeof(ObjectView))]
    public class InputPlPosViewModel : ObjectViewModelBase<Working>
    {
        /// <summary>
        /// Ид. списка пикинга.
        /// </summary>
        /// <returns></returns>
        public decimal? PlId { get; set; }

        protected override ObservableCollection<DataField> GetFields(SettingDisplay displaySetting)
        {
            var fields = base.GetFields(displaySetting);

            if (PlId.HasValue)
            {
                var field = fields.FirstOrDefault(p => p.FieldName == Working.WORKID_RPropertyName);
                if (field != null)
                    field.LookupFilterExt = string.Format(
                        "exists(select 1 from wmswork2entity w2e where w2e.workid_r = wmswork.workid and w2e.work2entityentity = 'PL' and w2e.work2entitykey = '{0}')",
                            PlId);
            }

            return fields;
        }
    }
}
