using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Xunit;

namespace AttributeAuthorization.Tests
{
    public class AuthRoutePermissionsTests
    {
        private HttpRequestMessage _request;
        private bool _authResolverCalled;
        private bool _shouldAllowUndefinedCalled;

        public AuthRoutePermissionsTests()
        {
            _request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");    
        }

        [Fact]
        public void When_No_Route_Found_Not_Allowed()
        {
            var permissions = new AuthRoutePermissions(new Dictionary<string, AuthPermissions> { { "template", new AuthPermissions()}},
                request =>
                {
                    _authResolverCalled = true;
                    return new List<string>();
                });

            var actual = permissions.IsAllowed(_request);

            Assert.False(actual);
            Assert.False(_authResolverCalled);
        }

        [Fact]
        public void When_No_Route_Found_ShouldAllowUndefined_Called()
        {
            bool expected = true;
            var permissions = new AuthRoutePermissions(new Dictionary<string, AuthPermissions> { { "template", new AuthPermissions() } },
                request =>
                {
                    _authResolverCalled = true;
                    return new List<string>();
                }, r =>
                {
                    _shouldAllowUndefinedCalled = true;
                    return expected;
                });

            var actual = permissions.IsAllowed(_request);

            Assert.Equal(expected, actual);
            Assert.True(_shouldAllowUndefinedCalled);
        }

        [Fact]
        public void When_RouteTemplate_NotFound_Not_Allowed()
        {
            AddRoute();
            var permissions = new AuthRoutePermissions(new Dictionary<string, AuthPermissions>(),
                request =>
                {
                    _authResolverCalled = true;
                    return new List<string>();
                });

            var actual = permissions.IsAllowed(_request);

            Assert.False(actual);
            Assert.False(_authResolverCalled);   
        }

        private void AddRoute(string templateName = "template")
        {
            _request.Properties[HttpPropertyKeys.HttpRouteDataKey] =
                new HttpRouteData(new HttpRoute(templateName,
                new HttpRouteValueDictionary(new { Controller = "controller", Action = "action" })));
        }

        [Fact]
        public void When_RouteTemplate_Found_AuthResolver_Called()
        {
            AddRoute();
            var permissions =
                new AuthRoutePermissions(
                    new Dictionary<string, AuthPermissions>
                    {
                        { "template", new AuthPermissions { Accepted = new List<string> { "write " } } }
                    }, request =>
                    {
                        _authResolverCalled = true;
                        return new List<string>();
                    });

            var actual = permissions.IsAllowed(_request);

            Assert.False(actual);
            Assert.True(_authResolverCalled);   
        }

        [Fact]
        public void When_NoPermissions_Not_Allowed()
        {
            AddRoute();
            var permissions =
                new AuthRoutePermissions(
                    new Dictionary<string, AuthPermissions>
                    {
                        { "template", new AuthPermissions() }
                    }, request =>
                    {
                        _authResolverCalled = true;
                        return new List<string>();
                    });

            var actual = permissions.IsAllowed(_request);

            Assert.False(actual);
        }

        [Fact]
        public void When_Has_Permissions_Allowed()
        {
            AddRoute();
            var permissions =
                new AuthRoutePermissions(
                    new Dictionary<string, AuthPermissions>
                    {
                        { "template", new AuthPermissions { Accepted = new List<string> { "write", "write2" } } }
                    }, request =>
                    {
                        _authResolverCalled = true;
                        return new List<string> { "write" };
                    });

            var actual = permissions.IsAllowed(_request);

            Assert.True(actual);    
        }

        [Fact]
        public void When_Auth_Not_Required_Allowed()
        {
            AddRoute();
            var permissions =
                new AuthRoutePermissions(
                    new Dictionary<string, AuthPermissions>
                    {
                        { "template", new AuthPermissions { AuthNotRequired = true } }
                    }, request =>
                    {
                        _authResolverCalled = true;
                        return new List<string> { "write" };
                    });

            var actual = permissions.IsAllowed(_request);

            Assert.True(actual);       
        }

        [Fact]
        public void When_Auth_Not_Required_And_Permissions_NotAllowed()
        {
            AddRoute();
            var permissions =
                new AuthRoutePermissions(
                    new Dictionary<string, AuthPermissions>
                    {
                        { "template", new AuthPermissions { Accepted = new List<string> { "read" }, AuthNotRequired = true } }
                    }, request =>
                    {
                        _authResolverCalled = true;
                        return new List<string>();
                    });

            var actual = permissions.IsAllowed(_request);

            Assert.False(actual);
        }

    }
}
