using System;
using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class OWBPosWrapper : BaseWrapper
    {
        public DateTime? OWBPOSPRODUCTDATE { get; set; }
        public DateTime? OWBPOSEXPIRYDATE { get; set; }
        public decimal? SKUID_R { get; set; }
        public decimal? OWBPOSNUMBER { get; set; }
        public decimal? OWBPOSID { get; set; }
        public decimal? OWBPOSCOUNT { get; set; }
        public decimal? OWBID_R { get; set; }
        public decimal? MANDANTID { get; set; }
        public double? OWBPOSPRICEVALUE { get; set; }
        public string STATUSCODE_R { get; set; }
        public string QLFCODE_R { get; set; }
        public string OWBPOSTONE { get; set; }
        public string OWBPOSSIZE { get; set; }
        public string OWBPOSSERIALNUMBER { get; set; }
        public string OWBPOSMEASURE { get; set; }
        public string OWBPOSHOSTREF { get; set; }
        public string OWBPOSFACTORY { get; set; }
        public string OWBPOSCOLOR { get; set; }
        public string OWBPOSBLOCKING { get; set; }
        public string OWBPOSBATCH { get; set; }
        public string OWBPOSARTNAME { get; set; }
        public string OWBPOSLOT { get; set; }
        public string OWBPOSGROUPCHECK { get; set; }
        public string OWBPOSARTCODE { get; set; }
        public double? OWBPOSCOUNT2SKU { get; set; }
        public string OWBPOSKITCODE { get; set; }
        public decimal? FACTORYID_R { get; set; }
        public decimal? OWBPOSOWNER { get; set; }
        public decimal? OWBPOSRESERVED { get; set; }
        public decimal? OWBPOSWANTAGE { get; set; }
        public decimal? OWBPOSCHECKMULTIPLE { get; set; }
        public List<BarcodeWrapper> BarcodeList { get; set; }
        public List<TransitDataWrapper> TRANSITDATAL { get; set; }
        public string OWBPOSBOXNUMBER { get; set; }

        /// <summary>
        /// Если CHECKBARCODE > 0, требуется проверка по ШК. 
        /// При этом: 
        /// если CHECKBARCODE == 1 - ошибка;
        /// Если CHECKBARCODE == 2 - предупреждение;
        /// Если CHECKBARCODE == 3 && string.IsNullOrEmpty(BARCODE) - предупреждение;
        /// В остальных случаях - ошибка.
        /// </summary>
        public int? CHECKBARCODE { get; set; }
        public string BARCODE { get; set; }

        public List<OWBPosCpvWrapper> CUSTOMPARAMVAL { get; set; }

        public double? OWBPOSPRICEVALUEVAT { get; set; }

        #region .  ShouldSerialize  .
        public bool ShouldSerializeOWBPOSPRICEVALUEVAT()
        {
            return OWBPOSPRICEVALUEVAT.HasValue;
        }
        #endregion .  ShouldSerialize  .
    }
}
