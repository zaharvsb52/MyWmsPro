using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using log4net;
using NUnit.Framework;
using wmsMLC.APS.wmsSI;
using wmsMLC.APS.wmsSI.Helpers;
using wmsMLC.APS.wmsSI.Wrappers;
using wmsMLC.General;
using wmsMLC.General.DAL;

namespace wmsMLC.Tests.Processes
{
    [TestFixture]
    public class ArtLoadTest : SiBaseTest
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(ArtLoadTest));

        [Test]
        public override void DoTest()
        {
            Log4NetHelper.Configure("TEST");

            var si = new IntegrationService();
            ArtWrapper[] items;
            var path = @"Data\ArtWrapper.xml";
            //var path = @"Data\ArtWrapper_SpeedTest.xml";
            var serialiser = new XmlSerializer(typeof(ArtWrapper[]));
            using (TextReader reader = new StreamReader(path))
            {
                items = (ArtWrapper[])serialiser.Deserialize(reader);
                reader.Close();
            }

            //var startTime = DateTime.Now;
            var result = si.ArtPackageLoad(items);
            //var endresult = string.Format("Загружено за {0}", DateTime.Now - startTime);
        }

        [Ignore]
        public void TestLoadCpv()
        {
            var uowFactory = IoC.Instance.Resolve<IUnitOfWorkFactory>();

            var item = new ArtWrapper
            {
                MANDANTID = 68,
                MandantCode = "VGB",
                ARTCODE = "VGB7320560620122",
                ARTNAME = "7320560620122",
                ARTUPDATE = 1,
                CUSTOMPARAMVAL = new List<ArtCpvWrapper>
                {
                     new ArtCpvWrapper
                     {
                          CUSTOMPARAMCODE_R_ARTCPV = "ARTProviderArtName",
                          CPVVALUE_ARTCPV = "Привет!"
                     },

                     new ArtCpvWrapper
                     {
                          CUSTOMPARAMCODE_R_ARTCPV = "ARTPartnerHostRef",
                          CPVVALUE_ARTCPV = @"ООО ""Рога и копыта"", Limited"
                     },

                     //new ArtCpvWrapper
                     //{
                     //     CUSTOMPARAMCODE_R_ARTCPV = "ARTPartner",
                     //     CPVVALUE_ARTCPV = @"ООО ""Рога и копыта"", Limited"
                     //}
                }
            };

            using (var uow = uowFactory.Create(false))
            {
                try
                {
                    uow.BeginChanges();
                    var result = ArtLoadHelper.LoadCpv(item: item, cpSource: null, cpTarget: null, uow: uow, log: _log);
                    uow.CommitChanges();
                    //uow.RollbackChanges();
                }
                catch 
                {
                    uow.RollbackChanges();
                    throw;
                }
            }
        }
    }
}