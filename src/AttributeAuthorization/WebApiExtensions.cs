using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;

namespace AttributeAuthorization
{
    public static class WebApiExtensions
    {
        public static void SetupHandlerOnInitialization(this HttpConfiguration config, 
            Func<Dictionary<string, AuthPermissions>, DelegatingHandler> handlerFactory,
            Action<IHttpRoute, Dictionary<string, AuthPermissions>> undefinedRouteAction = null)
        {
            config.Initializer = configuration =>
            {
                var builder = new RoutePermissionsBuilder(configuration, undefinedRouteAction);
                config.MessageHandlers.Add(handlerFactory(builder.Build())); 
            };
        }

        public static void SetupHandlerOnInitialization(this HttpConfiguration config,
            Func<HttpRequestMessage, IEnumerable<string>> authResolver,
            Action<IHttpRoute, Dictionary<string, AuthPermissions>> undefinedRouteAction = null)
        {
            config.Initializer = configuration =>
            {
                var builder = new RoutePermissionsBuilder(configuration, undefinedRouteAction);
                config.MessageHandlers.Add(new SimpleAttributeAuthHandler(builder.Build(), authResolver));
            };
        }
    }
}
