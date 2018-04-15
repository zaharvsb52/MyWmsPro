using System;
using System.Collections.Generic;

namespace wmsMLC.APS.wmsSI.Wrappers
{
    public class PartnerWrapper : BaseWrapper
    {
        public bool PARTNERLOCKED { get; set; }
        public DateTime? PARTNERDATECONTRACT { get; set; }
        public decimal? PARTNERID { get; set; }
        public decimal? MANDANTID { get; set; }
        public string PARTNERSETTLEMENTACCOUNT { get; set; }
        public string PARTNERPHONE { get; set; }
        public string PARTNEROKVED { get; set; }
        public string PARTNEROKPO { get; set; }
        public string PARTNEROGRN { get; set; }
        public string PARTNERNAME { get; set; }
        public string PARTNERKPP { get; set; }
        public string PARTNERINN { get; set; }
        public string PARTNERHOSTREF { get; set; }
        public string PARTNERFULLNAME { get; set; }
        public string PARTNERFAX { get; set; }
        public string PARTNEREMAIL { get; set; }
        public string PARTNERCORRESPONDENTACCOUNT { get; set; }
        public string PARTNERCONTRACT { get; set; }
        public string PARTNERCODE { get; set; }
        public string PARTNERBIK { get; set; }
        public string MandantCode { get; set; }
        public List<AddressBookWrapper> ADDRESS { get; set; }
        public List<PartnerGpvWrapper> GPVs { get; set; }
        public string PARTNERGROUPCODE { get; set; }
        public decimal? PARTNERCREATEBYHOSTREF { get; set; }
        public decimal? PARTNERCOMMERCTIME { get; set; }
        public string PARTNERCOMMERCTIMEMEASURE { get; set; }
        public decimal? PARTNERHOSTREFNAME { get; set; }
        public string PARTNERGROUPHOSTREF { get; set; }

        /// <summary>
        /// Управление обновлением юридического адреса партнера.
        /// </summary>
        public int? DONOTUPDATELEGALADDRESS { get; set; }
    }
}