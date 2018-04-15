using System;
using System.ServiceModel;

namespace wmsMLC.General.Services
{
    [ServiceContract(SessionMode = SessionMode.Required, Namespace = "http://wms.my.ru/services/")]
    public interface IWmsService
    {
        [OperationContract(IsInitiating = true)]
        string StartSession(ClientTypeCode clientType, decimal? clientSessionId);

        [OperationContract(IsInitiating = false, IsTerminating = true)]
        string TerminateSession();

        [OperationContract(IsInitiating = false)]
        void SetClientSession(decimal clientSessionId);



        [OperationContractAttribute(IsInitiating = false)]
        byte[] ProcessTelegram(byte[] telegram);

        [OperationContractAttribute(AsyncPattern = true, IsInitiating = false)]
        IAsyncResult BeginProcessTelegram(byte[] telegramm, AsyncCallback callback, object asyncState);

        byte[] EndProcessTelegram(IAsyncResult result);
    }
}