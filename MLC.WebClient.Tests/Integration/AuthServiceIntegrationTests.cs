using MLC.WebClient.Tests.Unit;
using NUnit.Framework;

namespace MLC.WebClient.Tests.Integration
{
    [TestFixture]
    public class AuthServiceIntegrationTests : AuthServiceTests
    {
        public AuthServiceIntegrationTests()
        {
            IsIntegrationTest = true;
        }
    }
}