using System;
using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class IWBPosWrapper : BaseWrapper
    {
        public bool IWBPOSMANUAL { get; set; }
        public DateTime? IWBPOSPRODUCTDATE { get; set; }
        public DateTime? IWBPOSEXPIRYDATE { get; set; }
        public decimal? SKUID_R { get; set; }
        public decimal? MANDANTID { get; set; }
        public decimal? IWBPOSNUMBER { get; set; }
        public decimal? IWBPOSID { get; set; }
        public decimal? IWBPOSCOUNT { get; set; }
        public decimal? IWBID_R { get; set; }
        public double? IWBPOSPRICEVALUE { get; set; }
        public string STATUSCODE_R_NAME { get; set; }
        public string STATUSCODE_R { get; set; }
        public string SKUID_R_NAME { get; set; }
        public string QLFCODE_R { get; set; }
        public string IWBPOSTONE { get; set; }
        public string IWBPOSTE { get; set; }
        public string IWBPOSSIZE { get; set; }
        public string IWBPOSSERIALNUMBER { get; set; }
        public string IWBPOSMEASURE { get; set; }
        public string IWBPOSHOSTREF { get; set; }
        public string IWBPOSFACTORY { get; set; }
        public string IWBPOSCOLOR { get; set; }
        public string IWBPOSBLOCKING_NAME { get; set; }
        public string IWBPOSBLOCKING { get; set; }
        public string IWBPOSBATCH { get; set; }
        public string IWBPOSARTNAME { get; set; }
        public string IWBID_R_NAME { get; set; }
        public string IWBPOSARTDESC { get; set; }
        public string IWBPOSGROUPCHECK { get; set; }
        public string IWBPOSLOT { get; set; }
        public string IWBPOSARTCODE { get; set; }
        public double? IWBPOSCOUNT2SKU { get; set; }
        public string IWBPOSINVOICENUMBER { get; set; }
        public DateTime? IWBPOSINVOICEDATE { get; set; }
        public decimal? FACTORYID_R { get; set; }
        public string IWBBATCHCODE { get; set; }
        public string IWBPOSBOXNUMBER { get; set; }
        public decimal? IWBPOSOWNER { get; set; }
        public decimal? IWBPOSCHECKMULTIPLE { get; set; }
        public double? IWBPOSPRODUCTCOUNT { get; set; }
        public List<TransitDataWrapper> TRANSITDATAL { get; set; }
        public List<Art2GroupWrapper> GROUP2ARTL { get; set; }

        /// <summary>
        /// Если CHECKBARCODE > 0, требуется проверка по ШК. 
        /// При этом: 
        /// если CHECKBARCODE == 1 - ошибка;
        /// Если CHECKBARCODE == 2 - предупреждение;
        /// Если CHECKBARCODE == 3 && string.IsNullOrEmpty(BARCODE) - предупреждение;
        /// Если CHECKBARCODE == 4 - нет ошибки, но BARCODE добавляем, если !string.IsNullOrEmpty(BARCODE);
        /// В остальных случаях - ошибка.
        /// </summary>
        public int? CHECKBARCODE { get; set; }
        public string BARCODE { get; set; }

        public List<IWBPosCpvWrapper> CUSTOMPARAMVAL { get; set; }

        /// <summary>
        /// Код страны происхождения по стандарту ISO. 2-ух или 3-ёх символьное.
        /// </summary>
        public string COUNTRYCODE_R { get; set; }
    }
}
