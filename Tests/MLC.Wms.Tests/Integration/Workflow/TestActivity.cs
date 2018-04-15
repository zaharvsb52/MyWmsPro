using System;
using System.Activities;
using System.Collections.Generic;
using NUnit.Framework;
using wmsMLC.Activities.General;
using wmsMLC.Business;
using wmsMLC.Business.Objects;

namespace MLC.Wms.Tests.Integration.Workflow
{
    [TestFixture]
    public class TestActivity
    {
        private static readonly TimeSpan DefaultTimeOut = new TimeSpan(0,0,0,30);

        [SetUp]
        public void Setup()
        {
            BLHelper.InitBL(dalType: DALType.Oracle);
        }

        [Test]
        public void ActivityDelete()
        {
            var activityNew = new UpdateActivity<TE>();
            var invokerNew = new WorkflowInvoker(activityNew);
            var te = new TE();
            te.AsDynamic().TECODE = "TestNameDel";
            te.AsDynamic().TETYPECODE_R = "EURO";
            te.AsDynamic().TECURRENTPLACE = "TestName";
            te.AsDynamic().TECREATIONPLACE = "TestName";
            te.AsDynamic().TESTATUS = 123;
            te.AsDynamic().TEMAXWEIGHT = 123;
            var parameters = new Dictionary<string, object> { { "Key", te } };
            invokerNew.Invoke(parameters, DefaultTimeOut);

            var activity = new DeleteActivity<TE>();
            var invoker = new WorkflowInvoker(activity);
            invoker.Invoke(parameters, DefaultTimeOut);

            var activityCheck = new GetByKeyActivity<TE>();
            var invokerCheck = new WorkflowInvoker(activityCheck);
            var parametersCheck = new Dictionary<string, object> { { "Key", "TestNameDel" } };
            var results = invokerCheck.Invoke(parametersCheck, DefaultTimeOut);
            Assert.True(results.Count != 0);
        }

        [Test]
        public void ActivityDeleteCollection()
        {
            var activityNew = new UpdateActivity<TE>();
            var invokerNew = new WorkflowInvoker(activityNew);
            var te = new TE();
            te.AsDynamic().TECODE = "TestNameDelCol";
            te.AsDynamic().TETYPECODE_R = "EURO";
            te.AsDynamic().TECURRENTPLACE = "TestName";
            te.AsDynamic().TECREATIONPLACE = "TestName";
            te.AsDynamic().TESTATUS = 123;
            te.AsDynamic().TEMAXWEIGHT = 123;
            var parametersNew = new Dictionary<string, object> { { "Key", te } };
            invokerNew.Invoke(parametersNew, DefaultTimeOut);

            var activityCheck = new GetByKeyActivity<TE>();
            var invokerCheck = new WorkflowInvoker(activityCheck);
            var parametersCheck = new Dictionary<string, object> { { "Key", "TestNameDelCol" } };
            var results = invokerCheck.Invoke(parametersCheck, DefaultTimeOut);
            Assert.True(results.Count == 1);

            var activity = new DeleteCollectionActivity<TE>();
            var invoker = new WorkflowInvoker(activity);
            var parameters = new Dictionary<string, object>();
            var keys = new List<TE> { te };
            parameters.Add("Keys", keys);
            invoker.Invoke(parameters, DefaultTimeOut);

            results = invokerCheck.Invoke(parametersCheck, DefaultTimeOut);
            Assert.True(results.Count != 0);
        }


        [Test]
        public void ActivityGetAll()
        {
            var activity = new GetAllActivity<TE>();
            var invoker = new WorkflowInvoker(activity);
            var parameters = new Dictionary<string, object>();
            var results = invoker.Invoke(parameters, new TimeSpan(0,0,0,30));
            Assert.True(results.Count > 0);
        }

        [Test]
        public void ActivityGetByFilter()
        {
            var activityNew = new UpdateActivity<TE>();
            var invokerNew = new WorkflowInvoker(activityNew);
            var te = new TE();
            te.AsDynamic().TECODE = "TestNameGetF";
            te.AsDynamic().TETYPECODE_R = "EURO";
            te.AsDynamic().TECURRENTPLACE = "TestName";
            te.AsDynamic().TECREATIONPLACE = "TestName";
            te.AsDynamic().TESTATUS = 123;
            te.AsDynamic().TEMAXWEIGHT = 123;
            var parametersNew = new Dictionary<string, object> {{"Key", te}};
            invokerNew.Invoke(parametersNew, DefaultTimeOut);

            var activity = new GetByFilterActivity<TE>();
            var invoker = new WorkflowInvoker(activity);
            var parameters = new Dictionary<string, object>();
            const string filter = "((TECODE = 'TestNameGetF'))";
            parameters.Add("Filter", filter);
            var results = invoker.Invoke(parameters, DefaultTimeOut);
            Assert.True(results.Count > 0);

            var activityDel = new DeleteActivity<TE>();
            var invokerDel = new WorkflowInvoker(activityDel);
            var parametersDel = new Dictionary<string, object> {{"Key", te}};
            invokerDel.Invoke(parametersDel, new TimeSpan(0, 0, 0, 30));
        }

        [Test]
        public void ActivityGetByKey()
        {
            var activityNew = new UpdateActivity<TE>();
            var invokerNew = new WorkflowInvoker(activityNew);
            var te = new TE();
            te.AsDynamic().TECODE = "TestNameGetK";
            te.AsDynamic().TETYPECODE_R = "EURO";
            te.AsDynamic().TECURRENTPLACE = "TestName";
            te.AsDynamic().TECREATIONPLACE = "TestName";
            te.AsDynamic().TESTATUS = 123;
            te.AsDynamic().TEMAXWEIGHT = 123;
            var parametersNew = new Dictionary<string, object> { { "Key", te } };
            invokerNew.Invoke(parametersNew, DefaultTimeOut);

            var activity = new GetByKeyActivity<TE>();
            var invoker = new WorkflowInvoker(activity);
            var parameters = new Dictionary<string, object> { { "Key", "TestNameGetK" } };
            var results = invoker.Invoke(parameters, DefaultTimeOut);
            Assert.True(results.Count == 1);

            var activityDel = new DeleteActivity<TE>();
            var invokerDel = new WorkflowInvoker(activityDel);
            var parametersDel = new Dictionary<string, object> { { "Key", te } };
            invokerDel.Invoke(parametersDel, DefaultTimeOut);
        }

        [Test]
        public void ActivityUpdate()
        {
            var activity = new UpdateActivity<TE>();
            var invoker = new WorkflowInvoker(activity);
            var te = new TE();
            te.AsDynamic().TECODE = "TestNameUp";
            te.AsDynamic().TETYPECODE_R = "EURO";
            te.AsDynamic().TECURRENTPLACE = "TestName";
            te.AsDynamic().TECREATIONPLACE = "TestName";
            te.AsDynamic().TESTATUS = 123;
            te.AsDynamic().TEMAXWEIGHT = 123;
            var parameters = new Dictionary<string, object> { { "Key", te } };
            invoker.Invoke(parameters, DefaultTimeOut);

            var activityCheck = new GetByKeyActivity<TE>();
            var invokerCheck = new WorkflowInvoker(activityCheck);
            var parametersCheck = new Dictionary<string, object> { { "Key", "TestNameUp" } };
            var results = invokerCheck.Invoke(parametersCheck, DefaultTimeOut);
            Assert.True(results.Count == 1);

            var activityDel = new DeleteActivity<TE>();
            var invokerDel = new WorkflowInvoker(activityDel);
            var parametersDel = new Dictionary<string, object> { { "Key", te } };
            invokerDel.Invoke(parametersDel, DefaultTimeOut);
        }

        [Test]
        public void ActivityUpdateCollection()
        {
            var activity = new UpdateCollectionActivity<TE>();
            var invoker = new WorkflowInvoker(activity);
            var te = new TE();
            te.AsDynamic().TECODE = "TestNameUpCol";
            te.AsDynamic().TETYPECODE_R = "EURO";
            te.AsDynamic().TECURRENTPLACE = "TestName";
            te.AsDynamic().TECREATIONPLACE = "TestName";
            te.AsDynamic().TESTATUS = 123;
            te.AsDynamic().TEMAXWEIGHT = 123;
            var parameters = new Dictionary<string, object> { { "Keys", new List<TE> { te } } };
            invoker.Invoke(parameters, DefaultTimeOut);

            var activityCheck = new GetByKeyActivity<TE>();
            var invokerCheck = new WorkflowInvoker(activityCheck);
            var parametersCheck = new Dictionary<string, object> { { "Key", "TestNameUpCol" } };
            var results = invokerCheck.Invoke(parametersCheck, DefaultTimeOut);
            Assert.True(results.Count == 1);

            var activityDel = new DeleteActivity<TE>();
            var invokerDel = new WorkflowInvoker(activityDel);
            var parametersDel = new Dictionary<string, object> { { "Key", te } };
            invokerDel.Invoke(parametersDel, DefaultTimeOut);
        }
    }
}