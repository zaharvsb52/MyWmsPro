using System;
using wmsMLC.General.BL.Annotations;

namespace wmsMLC.Business.Objects
{
    public class Entity2GC : WMSBusinessObject, IHasParent
    {
        #region .  Constants  .
        public const string ENTITY2GCIDPropertyName = "ENTITY2GCID";
        public const string ENTITY2GCGCCODEPropertyName = "ENTITY2GCGCCODE";
        public const string ENTITY2GCENTITYPropertyName = "ENTITY2GCENTITY";
        public const string ENTITY2GCKEYPropertyName = "ENTITY2GCKEY";
        public const string ENTITY2GCATTRPropertyName = "ENTITY2GCATTR";
        public const string CUSTOMPARAMVALPropertyName = "CUSTOMPARAMVAL";

        #endregion
        #region .  Properties  .
        public decimal? ENTITY2GCID
        {
            get { return GetProperty<decimal?>(ENTITY2GCIDPropertyName); }
            set { SetProperty(ENTITY2GCIDPropertyName, value); }
        }
      
        public string ENTITY2GCENTITY
        {
            get { return GetProperty<string>(ENTITY2GCENTITYPropertyName); }
            set { SetProperty(ENTITY2GCENTITYPropertyName, value); }
        }

        public string ENTITY2GCKEY
        {
            get { return GetProperty<string>(ENTITY2GCKEYPropertyName); }
            set { SetProperty(ENTITY2GCKEYPropertyName, value); }
        }

        public string ENTITY2GCATTR
        {
            get { return GetProperty<string>(ENTITY2GCATTRPropertyName); }
            set { SetProperty(ENTITY2GCATTRPropertyName, value); }
        }

        #endregion

        string IHasParent.SourceTypeName { get; set; }

        string IHasParent.TargetTypeName { get; set; }
    }

    public interface IHasParent
    {
        string SourceTypeName { get; set; }

        string TargetTypeName { get; set; }
    }
}