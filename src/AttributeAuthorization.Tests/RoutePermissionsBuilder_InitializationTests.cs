using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Xunit;

namespace AttributeAuthorization.Tests
{
    public class RoutePermissionsBuilder_InitializationTests
    {
        [Fact]
        public void When_IHttpControllerSelector_Not_Found_Error()
        {
            Assert.Throws<NullReferenceException>(() =>
            {
                var configuration = new HttpConfiguration();
                configuration.Services.Replace(typeof(IHttpControllerSelector), null);
                var builder = new RoutePermissionsBuilder(configuration);

                var actual = builder.Build();

            }).Message.Equals("Could not locate IHttpControllerSelector in configuration");
        }

    }
}