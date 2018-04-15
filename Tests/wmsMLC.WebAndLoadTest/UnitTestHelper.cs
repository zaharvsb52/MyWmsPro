using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.LoadTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace wmsMLC.WebAndLoadTest
{
    public class UnitTestHelper
    {
        private static readonly ConcurrentDictionary<int, Tuple<AppDomain, IUnitTest>> UserInstances = new ConcurrentDictionary<int,Tuple<AppDomain,IUnitTest>>();

        #region . Public .
        public static void Initialize(TestContext context, Type objectType)
        {
            var obj = GetProxyObject(context);
            if (obj == null)
                obj = CreateProxyObject(context, objectType);
            obj.Initialize(context.Properties);
        }

        public static void Terminate(TestContext context)
        {
            var obj = GetProxyObject(context);
            obj.Terminate();
            CloseProxyObject(context);
        }

        public static void Run(TestContext context)
        {
            var obj = GetProxyObject(context);
            obj.Run();
        }

        #endregion


        #region .  Private  . 

        private static int GetUserId(TestContext context)
        {
            var userContext = (LoadTestUserContext)context.Properties[LoadTestUserContext.LoadTestUserContextKey];
            return userContext.UserId;
        }

        private static IUnitTest GetProxyObject(TestContext context)
        {
            var userId = GetUserId(context);
            if (UserInstances.ContainsKey(userId))
                return UserInstances[userId].Item2;
            return null;
        }

        private static IUnitTest CreateProxyObject(TestContext context, Type objectType)
        {
            var domain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
            var test = (IUnitTest)domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, objectType.FullName);
            UserInstances.AddOrUpdate(GetUserId(context), new Tuple<AppDomain, IUnitTest>(domain, test), (key, oldValue) => oldValue);
            return test;
        }

        private static void CloseProxyObject(TestContext context)
        {
            var userId = GetUserId(context);
            if (!UserInstances.ContainsKey(userId))
                return;            
            Tuple<AppDomain, IUnitTest> tuple = null;
            if (!UserInstances.TryRemove(userId, out tuple))
                return;
            AppDomain.Unload(tuple.Item1);
        }

        #endregion
    }
}
