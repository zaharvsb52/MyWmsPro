using System;
using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class SalesInvoiceWrapper : BaseWrapper
    {
        public DateTime? OWBOUTDATEPLAN { get; set; }
        public decimal? OWBRECIPIENT { get; set; }
        public decimal OWBPRIORITY { get; set; }
        public decimal? OWBID { get; set; }
        public decimal? MANDANTID { get; set; }
        public decimal? ADDRESSBOOKID_R { get; set; }
        public string STATUSCODE_R { get; set; }
        public string OWBRESERVTYPE { get; set; }
        public string OWBPRODUCTNEED { get; set; }
        public string OWBNAME { get; set; }
        public string OWBHOSTREF { get; set; }
        public List<OWBPosWrapper> OWBPOSL { get; set; }
        public string OWBRECIPIENT_NAME { get; set; }
        public string OWBRECIPIENT_CODE { get; set; }
        public string OWBRECIPIENT_INN { get; set; }
        public string OWBRECIPIENT_OKPO { get; set; }
        public string OWBRECIPIENT_PHONE { get; set; }
        public string OWBFACTORY { get; set; }
        public decimal? OWBCOUNTPOS { get; set; }
        public string OWBDESC { get; set; }
        public decimal? OWBREPLACEKITS { get; set; }
        public decimal? OWBALLOWBASE { get; set; }
        public DateTime? OWBHOSTREFDATE { get; set; }

        /// <summary>
        /// Если OWBREPEAT == 1, то если накладная в статусе OWB_CANCELED - создаем, OWB_CREATED - ничего не делаем, в остальных - проверка на возможность создания.
        /// </summary>
        public decimal? OWBREPEAT { get; set; }

        public decimal? OWBCREATE_RECIPIENT { get; set; }
        public decimal? FACTORYID_R { get; set; }
        public decimal? OWBCONVERTSKU { get; set; }
        public string OWBPAYER_NAME { get; set; }
        public string OWBPAYER_CODE { get; set; }
        public string OWBPAYER_INN { get; set; }
        public string OWBPAYER_OKPO { get; set; }
        public string OWBPAYER_PHONE { get; set; }
        public decimal? OWBPAYER { get; set; }
        public int? OWBCREATE_PAYER { get; set; }
        public string OWBTYPE { get; set; }
        public string MandantCode { get; set; }
        public List<AddressBookWrapper> Address { get; set; }
        public List<OWBCpvWrapper> CUSTOMPARAMVAL { get; set; }
        public List<TransitDataWrapper> TRANSITDATAL { get; set; }
        public decimal? OWBRECREATE { get; set; }
        public decimal? OWBCHECKMULTIPLE { get; set; }
        public decimal? OWBOWNER { get; set; }
        public string OWBPARTNERGROUP { get; set; }
        public string STATUSCODE_R_NAME { get; set; }
        public string OWBARRIVED { get; set; }
        public string OWBDEPARTED { get; set; }
        public string OWBLOADBEGIN { get; set; }
        public string OWBLOADEND { get; set; }
        public decimal? OWB_HOSTREF_NAME { get; set; }
        public string OWBPARTNERORDER { get; set; }
        public EcomClientWrapper OWBCLIENTRECIPIENT { get; set; }
        public EcomClientWrapper OWBCLIENTPAYER { get; set; }
        public DateTime? OWBPLANNEDDELIVERYDATE { get; set; }
        
        /// <summary>
        /// Признак формирования позиций накладной по номеру короба позиции.
        /// </summary>
        public int? OWBBOXRESERVE { get; set; }

        public decimal? OWBCARRIER { get; set; }

        /// <summary>
        /// Наименование перевозчика, хост-идентификатор, и т.д.
        /// </summary>
        public string OWBCARRIERCODE { get; set; }

        /// <summary>
        /// Режим обновления CPV. Если свойство > 0 и в случае существования накладной обновляются только CPV для любого статуса накладной, при этом позиции и CPV не удаляются. 
        /// </summary>
        public int? UPDATEEXISTCPV { get; set; }

        /// <summary>
        /// Управление обновлением юридического адреса партнера.
        /// </summary>
        public int? DONOTUPDATELEGALADDRESS { get; set; }

        /// <summary>
        /// Автоматическое резервирование расходной накладной.
        /// </summary>
        public int? OWBAUTORES { get; set; }
    }
}
