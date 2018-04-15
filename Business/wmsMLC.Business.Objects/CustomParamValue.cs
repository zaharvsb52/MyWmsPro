using System;

namespace wmsMLC.Business.Objects
{
    public class CustomParamValue : WMSBusinessObject
    {
        #region .  Constants  .
        public const string CustomParamCodePropertyName = "CUSTOMPARAMCODE_R";
        public const string CPV2EntityPropertyName = "CPV2ENTITY";
        public const string CPVKeyPropertyName = "CPVKEY";
        public const string CPVValuePropertyName = "CPVVALUE";
        public const string CPVParentPropertyName = "CPVPARENT";
        public const string VCustomParamParentPropertyName = "VCUSTOMPARAMPARENT";
        public const string FormattedValuePropertyName = "FormattedValue";
        public const string IsReadOnlyPropertyName = "IsReadOnly";
        public const string IsMustSetPropertyName = "IsMustSet";
        public const string IsMustHavePropertyName = "IsMustHave";
        public const string VCUSTOMPARAMCOUNTPropertyName = "VCUSTOMPARAMCOUNT";
        public const string VCUSTOMPARAMDESCPropertyName = "VCUSTOMPARAMDESC";
        #endregion .  Constants  .

        #region .  Properties  .
        public decimal? CPVID
        {
            get { return GetProperty<decimal?>(GetPrimaryKeyPropertyName()); }
            set { SetProperty(GetPrimaryKeyPropertyName(), value); }
        }

        public string CustomParamCode
        {
            get { return GetProperty<string>(ChangePropertyName(CustomParamCodePropertyName)); }
            set { SetProperty(ChangePropertyName(CustomParamCodePropertyName), value); }
        }

        public string CPV2Entity
        {
            get { return GetProperty<string>(ChangePropertyName(CPV2EntityPropertyName)); }
            set { SetProperty(ChangePropertyName(CPV2EntityPropertyName), value); }
        }

        public string CPVKey
        {
            get { return GetProperty<string>(ChangePropertyName(CPVKeyPropertyName)); }
            set { SetProperty(ChangePropertyName(CPVKeyPropertyName), value); }
        }

        public string CPVValue
        {
            get { return GetProperty<string>(ChangePropertyName(CPVValuePropertyName)); }
            set { SetProperty(ChangePropertyName(CPVValuePropertyName), value); }
        }

        public decimal? CPVParent
        {
            get { return GetProperty<decimal?>(ChangePropertyName(CPVParentPropertyName)); }
            set { SetProperty(ChangePropertyName(CPVParentPropertyName), value); }
        }

        public string VCustomParamParent
        {
            get { return GetProperty<string>(ChangePropertyName(VCustomParamParentPropertyName)); }
            set { SetProperty(ChangePropertyName(VCustomParamParentPropertyName), value); }
        }

        public decimal VCustomParamCount
        {
            get { return GetProperty<decimal>(ChangePropertyName(VCUSTOMPARAMCOUNTPropertyName)); }
            set { SetProperty(ChangePropertyName(VCUSTOMPARAMCOUNTPropertyName), value); }
        }

        public string VCustomParamDesc
        {
            get { return GetProperty<string>(ChangePropertyName(VCUSTOMPARAMDESCPropertyName)); }
            set { SetProperty(ChangePropertyName(VCUSTOMPARAMDESCPropertyName), value); }
        }

        public CustomParam Cp { get; set; }

        private string _formattedValue;

        public string FormattedValue
        {
            get { return _formattedValue; }
            set
            {
                if (_formattedValue == value)
                    return;
                _formattedValue = value;
                OnPropertyChanged(FormattedValuePropertyName);
            }
        }

        public bool IsReadOnly
        {
            get { return Cp == null || Cp.IsReadOnly; }
        }

        public bool IsMustSet
        {
            get { return Cp == null || Cp.CustomParamMustSet; }
        }

        public bool IsMustHave
        {
            get { return Cp == null || Cp.CustomParamMustHave; }
        }

        #endregion .  Properties  .

        /// <summary>
        /// Преобразование базового названия свойства.
        /// </summary>
        public virtual string ChangePropertyName(string basePropertyName)
        {
            return GetType() == typeof(CustomParamValue)
                ? basePropertyName
                : string.Format("{0}_{1}", basePropertyName, GetType().Name).ToUpper();
        }

        //public static string ChangePropertyNameByType(string basePropertyName, Type type)
        //{
        //    return type == typeof(CustomParamValue)
        //        ? basePropertyName
        //        : string.Format("{0}_{1}", basePropertyName, type.Name).ToUpper();
        //}
    }
}