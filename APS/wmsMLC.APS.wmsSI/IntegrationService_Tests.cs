using System;
using System.ServiceModel;
using System.Threading;

namespace wmsMLC.APS.wmsSI
{
    public partial class IntegrationService
    {
        public string TestSimpleMessage(string message)
        {
            Log.InfoFormat(message);
            return message;
        }

        public void TestExceptionThrow()
        {
            Log.InfoFormat("Test Exception");
            throw new FaultException<string>("Test Exception", new FaultReason("Test Exception"));
        }

        public void TestTimeOut()
        {
            Log.InfoFormat((new TimeSpan(0, 0, 2, 0)).ToString());
            Thread.Sleep(new TimeSpan(0, 0, 2, 0));
        }
    }
}