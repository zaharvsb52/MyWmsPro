using System.Collections.Generic;
using System.Runtime.Serialization;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class ArtWrapper : BaseWrapper
    {
        #region . Properties .
        public decimal? MANDANTID { get; set; }
        public decimal ARTPICKORDER { get; set; }
        public decimal ARTCOMMERCDAY { get; set; }
        public double? ARTTEMPMIN { get; set; }
        public double? ARTTEMPMAX { get; set; }
        public string ARTSIZE { get; set; }
        public string ARTNAME { get; set; }
        public string ARTINPUTDATEMETHOD { get; set; }
        public string ARTHOSTREF { get; set; }
        public string ARTFACTORY { get; set; }
        public string ARTDESCEXT { get; set; }
        public string ARTDESC { get; set; }
        public string ARTCOLORTONE { get; set; }
        public string ARTCOLOR { get; set; }
        public string ARTCODE { get; set; }
        public string ARTABCD { get; set; }
        
        /// <summary>
        /// Управляющее свойство. Если ARTUPDATE == 1, то разрешено изменение артикула.
        /// </summary>
        public decimal? ARTUPDATE { get; set; }
        public decimal? FACTORYID_R { get; set; }
        public string ARTLABELCODE { get; set; }
        public decimal ARTLIFETIME { get; set; }
        public decimal ARTLIFETIMEREFRESH { get; set; }
        public string ARTLIFETIMEMEASURE { get; set; }
        public decimal? ARTCOMMERCTIME { get; set; }
        public string ARTCOMMERCTIMEMEASURE { get; set; }
        public string MandantCode { get; set; }
        public List<Art2GroupWrapper> GROUP2ARTL { get; set; }
        public List<SKUWrapper> SKUL { get; set; }
        public List<TransitDataWrapper> TRANSITDATAL { get; set; }

        /// <summary>
        /// Управляющее свойство. Если BARCODEONLY == 1, то осуществляется режим загрузки штрихкодов, ARTUPDATE = 0.
        /// </summary>
        public int? BARCODEONLY { get; set; }
        
        public string ARTBRAND { get; set; }

        /// <summary>
        /// Производитель. Ид. партнера. 
        /// </summary>
        [IgnoreDataMember]
        public decimal? ARTMANUFACTURER { get; set; }

        /// <summary>
        /// Производитель. Код партнера, HREF, наименование, полное наименование. 
        /// </summary>
        public string ARTMANUFACTURERCODE { get; set; }

        public string ARTMARK { get; set; }
        public string ARTMODEL { get; set; }
        public string ARTTYPE { get; set; }
        public string ARTIWBTYPE { get; set; }
        public string ARTDANGERLEVEL { get; set; }
        public string ARTDANGERSUBLEVEL { get; set; }
        public List<ArtCpvWrapper> CUSTOMPARAMVAL { get; set; }

        /// <summary>
        /// Код wf для предобработки данных перед загрузкой артикула.
        /// </summary>
        public string BEFOREPROCESSINGWFCODE { get; set; }

        /// <summary>
        /// Код страны происхождения по стандарту ISO. 2-ух или 3-ёх символьное, 
        /// или наименование страны. Для загрузки в БД - 3-ёх символное.
        /// </summary>
        public string COUNTRYCODE_R { get; set; }

        /// <summary>
        /// Признак создания производителя.
        /// </summary>
        public int? CREATEMANUFACTURER { get; set; }

        //Новые поля - begin
        public int? NOTNEEEDEDUPDATEARTGROUP { get; set; }
        //Новые поля - end
        #endregion . Properties .
    }
}
