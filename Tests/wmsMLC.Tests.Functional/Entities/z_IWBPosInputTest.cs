using FluentAssertions;
using NUnit.Framework;
using wmsMLC.Business;
using wmsMLC.Business.Objects;
using wmsMLC.General;
using wmsMLC.General.BL.Validation;

namespace wmsMLC.Tests.Functional.Entities
{
    [TestFixture]
    public class IWBPosInputTest
    {
        [TestFixtureSetUp]
        public virtual void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
            //            BLHelper.InitBL(dalType: DALType.Service);
            //            BLHelper.RegisterServiceClient("Auto", "net.tcp://localhost:8035/wmsSDCLService");
            var auth = IoC.Instance.Resolve<IAuthenticationProvider>();
            auth.Authenticate("TECH_AUTOTEST", "dVAdfX0iqheq4yd");
            // тестируем валидацию - ValidatorFactory вместо EmptyValidatorFactory
            IoC.Instance.Register<IValidatorFactory, ValidatorFactory>(LifeTime.Singleton);
        }

        [Test]
        public void ValidateCollectionTest()
        {
            var item = new IWBPosInput();
            item.Validate();
            item.QLFDetailL = new WMSBusinessCollection<IWBPosQLFDetailDesc>();
            item.QLFCODE_R = IWBPosInput.QlfTypeDefect;
            item.QLFDetailL.Add(new IWBPosQLFDetailDesc());
            item.QLFDetailL.Add(new IWBPosQLFDetailDesc());
            item.Validator.Errors.Should().NotBeNull();
            item.Validator.Errors.Should().HaveCount(1);
            item.QLFCODE_R = "";
            item.Validator.Errors.Should().BeEmpty();
        }
    }
}