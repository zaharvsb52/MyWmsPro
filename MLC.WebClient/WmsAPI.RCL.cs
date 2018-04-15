using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MLC.WebClient
{
    public partial class WmsAPI
    {
        /// <summary>
        /// Получение количества списков пикинга, доступных данному пользователю при использовании указанного погрузчика
        /// </summary>
        /// <param name="truckCode">
        /// Код погрузчика. Если оставить пустым, то будет использован код,
        /// привязанный к текущей сессии этого пользователя</param>
        /// <returns>Количество доступных списков</returns>
        public Task<int> GetAvailablePickListCountAsync(string truckCode = null)
        {
            return WithTransaction("getAvailablePickListCount")
                .AddParameter("truckCode", truckCode)
                .ProcessAsync<int>();
        }

        /// <summary>
        /// Получение количества транспортных заданий, доступных данному пользователю при использовании указанного погрузчика
        /// </summary>
        /// <param name="wmsSessionId">
        /// Код сессии рабты клиента. Обычно берется из WmsEnvironment.SessionId</param>
        /// <returns>Словать, где ключом является статус задания, а значением - кол-во заданий в этом статусе</returns>
        public Task<Dictionary<string, int>> GetAvailableTransportTaskCountAsync(int wmsSessionId)
        {
            var rawData = WithTransaction("getAvailableTransportTaskCount")
                .AddParameter("wmsSessionId", wmsSessionId)
                .ProcessAsync<List<List<object>>>();

            return rawData.ContinueWith(t =>
            {
                if (t.Exception != null)
                    throw t.Exception;

                // трансформируем в удобный массив
                var res = new Dictionary<string, int>();
                if (t.Result != null && t.Result.Count > 0)
                {
                    foreach (var row in t.Result)
                    {
                        if (row.Count != 2)
                            throw new FormatException("Ожидался массив из 2х элементов. А получен из " + row.Count);

                        res.Add((string) row[0], Convert.ToInt32(row[1]));
                    }
                }
                return res;
            });
        }
    }
}
