using System;
using Microsoft.VisualStudio.TestTools.LoadTesting;
using wmsMLC.General;

namespace wmsMLC.WebAndLoadTest
{
    public class CopyParamtersPlugin : ILoadTestPlugin
    {
        //store the load test object.  
        private LoadTest _loadTest;

        public void Initialize(LoadTest loadTest)
        {
            _loadTest = loadTest;

            //connect to the TestStarting event.
            _loadTest.TestStarting += new EventHandler<TestStartingEventArgs>(mLoadTest_TestStarting);
        }

        void mLoadTest_TestStarting(object sender, TestStartingEventArgs e)
        {
            //When the test starts, copy the load test context parameters to
            //the test context parameters
            foreach (string key in _loadTest.Context.Keys)
            {
                e.TestContextProperties.Add(key, _loadTest.Context[key]);
            }
        }
    }
}
