using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using wmsMLC.Business.Objects;
using wmsMLC.General.BL;
using wmsMLC.General.PL;
using wmsMLC.General.PL.Model;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.DCL.Content.ViewModels.ArtMassInput
{
    [SysObjectName("Art")]
    public class ArtItem : Art
    {
        public SkuListMassInputViewModel SKUList { get; set; }
        public Art2GroupListMassInputViewModel Art2GroupList { get; set; }

        private static readonly List<string> _excludedArtFields = new List<string>()
        {
            Art.ARTCODEPropertyName, 
            Art.UserInsPropertyName,
            Art.DateInsPropertyName,
            Art.UserUpdPropertyName,
            Art.DateUpdPropertyName,
            Art.TransactPropertyName,
            Art.SKULPropertyName,
            Art.GROUP2ARTLPropertyName,
            Art.CUSTOMPARAMVALPropertyName
        };

        protected List<string> ExcludedFields
        {
            get { return _excludedArtFields; }
        }

        private readonly Lazy<ObservableCollection<DataField>> _fields;

        public ObservableCollection<DataField> Fields
        {
            get { return _fields.Value; }
        }

        public static IEnumerable<DataField> GetFields(Type type)
        {
            return DataFieldHelper.Instance.GetDataFields(type, SettingDisplay.Detail);
        }


        public ArtItem()
        {
            _fields = new Lazy<ObservableCollection<DataField>>(
                () => new ObservableCollection<DataField>(GetFields(typeof(ArtItem)).Where(f => !ExcludedFields.Any(e => e.Equals(f.FieldName, StringComparison.OrdinalIgnoreCase)))),
                LazyThreadSafetyMode.ExecutionAndPublication);

            SKUList = new SkuListMassInputViewModel(false);
            Art2GroupList = new Art2GroupListMassInputViewModel(false);
        }
    }
}