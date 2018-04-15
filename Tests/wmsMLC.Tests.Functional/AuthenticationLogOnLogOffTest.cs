using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL;
using wmsMLC.General.PL.WPF.Helpers;

namespace wmsMLC.Tests.Functional
{
    [TestFixture]
    public class AuthenticationLogOnLogOffTest
    {
        private const string AutoTestUser = "AutoTestUs";

        [TestFixtureSetUp]
        public virtual void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Неверное имя пользователя (AutoTestUs)  или пароль\n")]
        public void WrongNameOrPasswordTest()
        {
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate(AutoTestUser, AutoTestUser);
        }

        [Test, Ignore("Пока нельзя запустить SDCL из теста")]
        [ExpectedException("Недостаточно прав для выполнения операции")]
        public void WithoutRightsUserTest()
        {
            BLHelper.InitBL(dalType: DALType.Service);
            BLHelper.RegisterServiceClient("Auto", ClientTypeCode.DCL);

            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("T1", "T1");

            AuthenticationHelper.Authenticate("T1", "T1");
            var mgr = IoC.Instance.Resolve<IBaseManager<GlobalParamValue>>();
            mgr.GetAll();
        }

    }
}