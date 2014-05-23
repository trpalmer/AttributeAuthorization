using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace AttributeAuthorization
{
    public class AuthRoutePermissions
    {
        private static readonly string[] EmptyList = new string[0];
        private readonly Dictionary<string, AuthPermissions> _routePermissions;
        private readonly Func<HttpRequestMessage, bool> _shouldAllowNotDefined;
        private readonly Func<HttpRequestMessage, IEnumerable<string>> _authResolver;

        public AuthRoutePermissions(Dictionary<string, AuthPermissions> routePermissions, 
            Func<HttpRequestMessage, IEnumerable<string>> authResolver = null,
            Func<HttpRequestMessage, bool> shouldAllowUndefined = null)
        {
            routePermissions.CheckNull("routePermission");
            _routePermissions = routePermissions;
            _authResolver = authResolver ?? NoPermissions;
            _shouldAllowNotDefined = shouldAllowUndefined ?? NotDefinedDenied;
        }

        private bool NotDefinedDenied(HttpRequestMessage request)
        {
            return false;
        }

        private IEnumerable<string> NoPermissions(HttpRequestMessage request)
        {
            return EmptyList;
        }

        public bool AuthNotRequired(HttpRequestMessage request)
        {
            request.CheckNull("request");
            AuthPermissions permissions;
            return InternalAuthNotRequired(request, out permissions);
        }

        private bool InternalAuthNotRequired(HttpRequestMessage request, out AuthPermissions permissions)
        {
            bool result;
            permissions = null;

            var route = FindRoute(request);

            if (route != null && _routePermissions.ContainsKey(route.Route.RouteTemplate))
            {
                permissions = _routePermissions[route.Route.RouteTemplate];
                result = (!permissions.Accepted.Any() && permissions.AuthNotRequired);
            }
            else
            {
                result = _shouldAllowNotDefined(request);
            }
            return result;
        }

        public bool IsAllowed(HttpRequestMessage request, IEnumerable<string> requestedPermissions = null)
        {
            request.CheckNull("request");

            AuthPermissions permissions;
            var result = InternalAuthNotRequired(request, out permissions);
            if (!result && permissions != null)
            {
                requestedPermissions = requestedPermissions ?? _authResolver(request);
                result = permissions.Accepted.Any() && permissions.Accepted.Intersect(requestedPermissions).Any();
            }
            return result;    
        }

        public IHttpRouteData FindRoute(HttpRequestMessage request)
        {
            IHttpRouteData result = null;
            object propValue;
            if (request.Properties.TryGetValue(HttpPropertyKeys.HttpRouteDataKey, out propValue))
            {
                result = propValue as IHttpRouteData;
            }
            return result;
        }
    }
}