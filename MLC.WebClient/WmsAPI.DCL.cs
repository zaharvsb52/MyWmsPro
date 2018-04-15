using System.Collections.Generic;

namespace MLC.WebClient
{
    public partial class WmsAPI
    {
        /// <summary>
        /// Изменение владельца товара по накладной.
        /// </summary>
        /// <param name="owbId">
        /// ID приходной накладной</param>
        /// <param name="operation">
        /// операция</param>
        /// <param name="clientType">
        /// тип клиента</param>
        /// <param name="timeout">
        /// timeout в секундах</param>
        public void ChangeOwnerByOWB(int owbid, string operation, string clientType, int timeout)
        {
            WithTransaction("changeOwnerByOwb")
                .AddParameter("owbid", owbid)
                .AddParameter("operation", operation)
                .AddParameter("clientType", clientType)
                .AddParameter("timeout", timeout > 0 ? timeout : (int?)null)
                .Process();
        }

        /// <summary>
        /// Заявка на декларирование.
        /// </summary>
        /// Коллекция id приходных накладных
        /// <param name="iwbidList"></param>
        /// <returns></returns>
        public int Application4Declaration(IEnumerable<decimal> iwbidList)
        {
            var apiresult = WithTransaction("application4Declaration")
               .AddParameter("iwbidList", iwbidList)
               .Process<dynamic>();

            return (int)apiresult;
        }

        /// <summary>
        /// Указать ГТД для выбранных накладных.
        /// </summary>
        public void UpdateIwbGtd(int[] iwbIds, string gtd, int? timeout)
        {
            WithTransaction("updateIwbGtd")
               .AddParameter("iwbIds", iwbIds)
               .AddParameter("gtd", gtd)
               .AddParameter("timeout", timeout)
               .Process();
        }
    }
}
