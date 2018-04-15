using System.ServiceModel;
using wmsMLC.APS.wmsSI.Wrappers;

namespace wmsMLC.APS.wmsSI
{
    [ServiceContract(Namespace = "http://wms.my.ru/services/")]
    public partial interface IIntegrationService
    {
        /// <summary>
        /// Загрузка рейса
        /// </summary>
        /// <param name="item">Телеграмма рейса</param>
        [OperationContract]
        [FaultContract(typeof(string))]
        void YExternalTrafficLoad(YExternalTrafficWrapper item);

        /// <summary>
        /// Получение актов билинга по фильтру
        /// </summary>
        /// <param name="filter">фильтр</param>
        /// <returns>список актов</returns>
        [OperationContract]
        [FaultContract(typeof(string))]
        BillWorkActWrapper[] BillWorkActGet(string filter);

        /// <summary>
        /// Подтверждение акта билинга
        /// </summary>
        /// <param name="workActCommit"></param>
        /// <returns>акт билинга</returns>
        [OperationContract]
        [FaultContract(typeof(string))]
        void BillWorkActCommit(WorkActCommit[] workActCommit);

        /// <summary>
        /// Загрузка партнера
        /// </summary>
        /// <param name="item">партнер</param>
        /// <returns>"результат загрузки"</returns>
        [OperationContract]
        [FaultContract(typeof(string))]
        ErrorWrapper[] PartnerLoad(PartnerWrapper item);

        /// <summary>
        /// Получение детализации акта
        /// </summary>
        /// <param name="filter">уникальный номер акта</param>
        /// <returns>список детализации</returns>
        [OperationContract]
        [FaultContract(typeof(string))]
        BillWorkActDetailWrapper[] BillWorkActDetailGet(string filter);
        /// <summary>
        /// Получение всех мандантов
        /// </summary>
        /// <returns>список мандантов</returns>
        [OperationContract]
        [FaultContract(typeof(string))]
        MandantWrapper[] MandantGet();
        /// <summary>
        /// Загрузка комплектов
        /// </summary>
        /// <param name="item"></param>
        /// <returns>"результат загрузки"</returns>
        [OperationContract]
        [FaultContract(typeof(string))]
        ErrorWrapper[] KitLoad(KitWrapper[] item);

        /// <summary>
        /// Загрузка артикулов пакетом
        /// </summary>
        /// <param name="artPackage"></param>
        /// <param name="beforeProcessingWfcode"></param>
        /// <returns>"результат загрузки"</returns>
        [OperationContract]
        [FaultContract(typeof(string))]
        ErrorWrapper[] ArtPackageLoad(ArtWrapper[] artPackage);

        /// <summary>
        /// Загрузка приходной накладной (возврат ошибок загрузки)
        /// </summary>
        /// <param name="item">приходная накладная</param>
        [OperationContract]
        [FaultContract(typeof(string))]
        ErrorWrapper[] ReceiptLoad(PurchaseInvoiceWrapper item);

        /// <summary>
        /// Загрузка расходной накладной (возврат ошибок загрузки)
        /// </summary>
        /// <param name="item">расходная накладная</param>
        [OperationContract]
        [FaultContract(typeof(string))]
        ErrorWrapper[] ShipmentLoad(SalesInvoiceWrapper item);

        /// <summary>
        /// Загрузка этикеток пакета артикулов (возврат ошибок загрузки)
        /// </summary>
        /// <param name="labelPackage">Этикетки пакета артикулов</param>
        [OperationContract]
        [FaultContract(typeof(string))]
        ErrorWrapper[] LabelsLoad(LabelWrapper[] labelPackage);

        /// <summary>
        /// Запрос состояния приходов
        /// </summary>
        /// <param name="request">Запрос</param>
        [OperationContract]
        [FaultContract(typeof(string))]
        PurchaseInvoiceWrapper[] StatusArrivalRequest(RequestWrapper request);

        /// <summary>
        /// Запрос состояния заказов
        /// </summary>
        /// <param name="request">Запрос</param>
        [OperationContract]
        [FaultContract(typeof(string))]
        SalesInvoiceWrapper[] StatusOrderRequest(RequestWrapper request);

        #region .  Test  .
        [OperationContract()]
        [FaultContract(typeof(string))]
        string TestSimpleMessage(string message);

        [OperationContract]
        [FaultContract(typeof(string))]
        void TestExceptionThrow();

        [OperationContract]
        [FaultContract(typeof(string))]
        void TestTimeOut();
        #endregion
    }
}