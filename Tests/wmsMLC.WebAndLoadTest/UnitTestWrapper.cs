using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace wmsMLC.WebAndLoadTest
{
    public abstract class UnitTestWrapper
    {
        private TestContext _testContextInstance;

        public TestContext TestContext
        {
            get { return _testContextInstance; }
            set { _testContextInstance = value; }
        }
    }
}
