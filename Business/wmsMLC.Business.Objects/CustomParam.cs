using System;

namespace wmsMLC.Business.Objects
{
    public class CustomParam : WMSBusinessObject
    {
        #region .  Constants  .
        public const string CustomParamCodePropertyName = "CUSTOMPARAMCODE";
        public const string CustomParamParentPropertyName = "CUSTOMPARAMPARENT";
        public const string CustomParamNamePropertyName = "CUSTOMPARAMNAME";
        public const string CustomParamDataTypePropertyName = "CUSTOMPARAMDATATYPE";
        public const string CustomParamCountPropertyName = "CUSTOMPARAMCOUNT";
        public const string CustomParam2EntityPropertyName = "CUSTOMPARAM2ENTITY";
        public const string CustomParamMustSetPropertyName = "CUSTOMPARAMMUSTSET";
        public const string CustomParamFormatPropertyName = "CUSTOMPARAMFORMAT";
        public const string CustomparamInputdisablePropertyName = "CUSTOMPARAMINPUTDISABLE";
        public const string ObjectlookupCode_RPropertyName = "OBJECTLOOKUPCODE_R";
        public const string CustomParamDefaultPropertyName = "CUSTOMPARAMDEFAULT";
        public const string CUSTOMPARAMSAVEMODEPropertyName = "CUSTOMPARAMSAVEMODE";
        public const string CUSTOMPARAMDESCPropertyName = "CUSTOMPARAMDESC";
        public const string CUSTOMPARAMMUSTHAVEPropertyName = "CUSTOMPARAMMUSTHAVE";
        public const string CUSTOMPARAMSOURCEPropertyName = "CUSTOMPARAMSOURCE";
        public const string CUSTOMPARAMTARGETPropertyName = "CUSTOMPARAMTARGET";
        
        #endregion .  Constants  .

        #region .  Properties  .
        public string CustomParamParent
        {
            get { return GetProperty<string>(CustomParamParentPropertyName); }
            set { SetProperty(CustomParamParentPropertyName, value); }
        }

        public string CustomParamName
        {
            get { return GetProperty<string>(CustomParamNamePropertyName); }
            set { SetProperty(CustomParamNamePropertyName, value); }
        }

        public decimal CustomParamDataType
        {
            get { return GetProperty<decimal>(CustomParamDataTypePropertyName); }
            set { SetProperty(CustomParamDataTypePropertyName, value); }
        }

        public decimal CustomParamCount
        {
            get { return GetProperty<decimal>(CustomParamCountPropertyName); }
            set { SetProperty(CustomParamCountPropertyName, value); }
        }

        public string CustomParam2Entity
        {
            get { return GetProperty<string>(CustomParam2EntityPropertyName); }
            set { SetProperty(CustomParam2EntityPropertyName, value); }
        }

        public bool CustomParamMustSet
        {
            get { return GetProperty<bool>(CustomParamMustSetPropertyName); }
            set { SetProperty(CustomParamMustSetPropertyName, value); }
        }

        public string CustomParamFormat
        {
            get { return GetProperty<string>(CustomParamFormatPropertyName); }
            set { SetProperty(CustomParamFormatPropertyName, value); }
        }

        public bool CustomparamInputdisable
        {
            get { return GetProperty<bool>(CustomparamInputdisablePropertyName); }
            set { SetProperty(CustomparamInputdisablePropertyName, value); }
        }

        public string ObjectlookupCode_R
        {
            get { return GetProperty<string>(ObjectlookupCode_RPropertyName); }
            set { SetProperty(ObjectlookupCode_RPropertyName, value); }
        }

        public string CustomParamDefault
        {
            get { return GetProperty<string>(CustomParamDefaultPropertyName); }
            set { SetProperty(CustomParamDefaultPropertyName, value); }
        }

        public bool CUSTOMPARAMSAVEMODE
        {
            get { return GetProperty<bool>(CUSTOMPARAMSAVEMODEPropertyName); }
            set { SetProperty(CUSTOMPARAMSAVEMODEPropertyName, value); }
        }

        public string CustomParamDesc
        {
            get { return GetProperty<string>(CUSTOMPARAMDESCPropertyName); }
            set { SetProperty(CUSTOMPARAMDESCPropertyName, value); }
        }

        public bool CustomParamMustHave
        {
            get { return GetProperty<bool>( CUSTOMPARAMMUSTHAVEPropertyName); }
            set { SetProperty(CUSTOMPARAMMUSTHAVEPropertyName, value); }
        }

        public string CustomParamSource
        {
            get { return GetProperty<string>(CUSTOMPARAMSOURCEPropertyName); }
            set { SetProperty(CUSTOMPARAMSOURCEPropertyName, value); }
        }

        public string CustomParamTarget
        {
            get { return GetProperty<string>(CUSTOMPARAMTARGETPropertyName); }
            set { SetProperty(CUSTOMPARAMTARGETPropertyName, value); }
        }

        public Type ValueType { get; set; }

        public bool IsReadOnly { get; set; }
        #endregion .  Properties  .
        }
}