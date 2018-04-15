using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using FluentAssertions;
using MLC.SvcClient;
using MLC.WebClient.Impl;
using Moq;
using NUnit.Framework;

namespace MLC.WebClient.Tests.Unit
{
    [TestFixture]
    public class AuthServiceTests
    {
        public AuthServiceTests()
        {
            IsIntegrationTest = false;
        }

        protected bool IsIntegrationTest { get; set; }

        [Test]
        public virtual void When_creds_are_correct_than_auth_is_ok()
        {
            // prepare
            var handler = IsIntegrationTest ? new StubHttpMessageHandler() : CreateSuccessStubOfHttpMessageHandler();

            var httpClient = new HttpClient(handler) { BaseAddress = new Uri(TestSettings.BaseUrl) };
            var httpClientStoreMock = new Mock<IHttpClientStore>();
            httpClientStoreMock.Setup(i => i.GetOrCreate(TestSettings.BaseUrl)).Returns(httpClient);
            var httpClientStore = httpClientStoreMock.Object;

            // run
            var authService = new WmsAuthService(TestSettings.BaseUrl, httpClientStore);
            string userCode;
            var res = authService.Authenticate(TestSettings.DefaultOkLogin, TestSettings.DefaultOkPass, out userCode);

            // check
            userCode.Should().Be(TestSettings.DefaultOkLogin);
            res.Should().Be(true);
            handler.RequestMessage.Method.Should().Be(HttpMethod.Post);
            handler.RequestMessage.RequestUri.ShouldBeEquivalentTo(TestSettings.BaseUrl + "security/login");
            handler.ResponseMessage.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);

            if (IsIntegrationTest)
            {
                var cookies = handler.CookieContainer.GetCookies(new Uri(TestSettings.BaseUrl));
                cookies.Should().HaveCount(1);
                var wmsAuthCookie = cookies.OfType<Cookie>().Single(i => i.Name == ".WmsAuth");
            }
        }

        [Test]
        public virtual void When_creds_are_not_correct_than_auth_is_not_ok()
        {
            // prepare
            var handler = IsIntegrationTest ? new StubHttpMessageHandler() : CreateNonSuccessStubOfHttpMessageHandler();
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri(TestSettings.BaseUrl) };
            var httpClientStoreMock = new Mock<IHttpClientStore>();
            httpClientStoreMock.Setup(i => i.GetOrCreate(TestSettings.BaseUrl)).Returns(httpClient);
            var httpClientStore = httpClientStoreMock.Object;

            // run
            var authService = new WmsAuthService(TestSettings.BaseUrl, httpClientStore);
            string userCode;
            var res = authService.Authenticate("WrongUser", "WrongPassword", out userCode);

            // check
            userCode.Should().BeNull();
            res.Should().Be(false);
            handler.RequestMessage.Method.Should().Be(HttpMethod.Post);
            handler.RequestMessage.RequestUri.ShouldBeEquivalentTo(TestSettings.BaseUrl + "security/login");
            handler.ResponseMessage.StatusCode.ShouldBeEquivalentTo(HttpStatusCode.OK);
            var cookies = handler.CookieContainer.GetCookies(new Uri(TestSettings.BaseUrl));
            cookies.Should().BeEmpty();
        }

        public static StubHttpMessageHandler CreateSuccessStubOfHttpMessageHandler()
        {
            var content = new StringContent(string.Format("{{ \"success\": true, \"userCode\": \"{0}\"}}", TestSettings.DefaultOkLogin));
            var cookiesContainer = new CookieContainer();
            cookiesContainer.Add(new Cookie(".WmsAuth", "test", "/", "localhost"));
            return new StubHttpMessageHandler
            {
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) {Content = content},
                CookieContainer = cookiesContainer
            };

        }

        public static StubHttpMessageHandler CreateNonSuccessStubOfHttpMessageHandler()
        {
            var content = new StringContent("{ \"success\": false }");
            return new StubHttpMessageHandler
            {
                ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = content },
                CookieContainer = new CookieContainer()
            };

        }

        public static void Authenticate(string baseUrl, IHttpClientStore clientStore, string login = TestSettings.DefaultOkLogin, string pass = TestSettings.DefaultOkPass)
        {
            var authSvc = new WmsAuthService(baseUrl, clientStore);
            string userCode;
            var res = authSvc.Authenticate(login, pass, out userCode);
            if (!res)
                throw new AuthenticationException();
        }
    }
}