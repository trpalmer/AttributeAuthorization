using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Routing;

namespace AttributeAuthorization
{
    public class RoutePermissionsBuilder
    {
        private readonly HttpConfiguration _configuration;

        // consider: Methods that are decorated with RequiresAuth AND RequiresNoAuth. Current just falls back to not allowed
        // Add a callback on build, include a callback that would throw an exception on build?
        public RoutePermissionsBuilder(HttpConfiguration configuration, Action<IHttpRoute, Dictionary<string, AuthPermissions>> undefinedRouteAction = null)
        {
            configuration.CheckNull("configuration");
            _configuration = configuration;
        }

        public Dictionary<string, AuthPermissions> Build()
        {
            return BuildPermissions();
        }

        private Dictionary<string, AuthPermissions> BuildPermissions()
        {
            var map = new Dictionary<string, AuthPermissions>();
	        var apiDescriptions = _configuration.Services.GetApiExplorer().ApiDescriptions;
			foreach (var description in apiDescriptions)
			{
				var actionDescriptor = description.ActionDescriptor;
				var controllerDescriptor = actionDescriptor.ControllerDescriptor;
                var mapAdditions = new Dictionary<string, AuthPermissions>();
                bool isPublic = actionDescriptor.GetCustomAttributes<RequiresNoAuth>().Any() ||
                                controllerDescriptor.GetCustomAttributes<RequiresNoAuth>().Any();

                var accepted = actionDescriptor.GetCustomAttributes<RequiresAuth>().SelectMany(a => a.Expand())
                    .Concat(controllerDescriptor.GetCustomAttributes<RequiresAuth>().SelectMany(a => a.Expand()))
                    .Distinct().ToList();

                mapAdditions = GetMapForRoute(description.Route, isPublic, accepted);
                foreach (var addMap in mapAdditions)
                {
                    if (!map.ContainsKey(addMap.Key))
                    {
                        map.Add(addMap.Key, addMap.Value);
                    }
                }
            }
            return map;
        }

        private Dictionary<string, AuthPermissions> GetMapForRoute(IHttpRoute route, bool isPublic, List<string> accepted)
        {
            var map = new Dictionary<string, AuthPermissions>();

            var authPermission = new AuthPermissions
            {
                AuthNotRequired = isPublic,
                Accepted = accepted
            };

            if (route.Constraints.Count == 0)
            {
                map.Add(route.RouteTemplate, authPermission);
                return map;
            }

            return GetVerbMapRoute(route, authPermission);
        }

        private Dictionary<string, AuthPermissions> GetVerbMapRoute(IHttpRoute route, AuthPermissions authPermission)
        {
            var map = new Dictionary<string, AuthPermissions>();
            var constraints = GetConstraints(route);

            if (constraints.Count > 0)
            {
                foreach (var verb in constraints)
                {
                    if (verb == HttpMethod.Options.Method)
                    {
                        continue;
                    }
                    var key = verb + ":" + route.RouteTemplate;
                    map.Add(key, authPermission);
                }
            }
            else
            {
                map.Add(route.RouteTemplate, authPermission);
            }

            return map;
        }

        private List<string> GetConstraints(IHttpRoute route)
        {
            var constraints = new List<string>();
            object allowedVerbs;

            if (route.Constraints.TryGetValue("inboundHttpMethod", out allowedVerbs))
            {
                var test = allowedVerbs.GetType();

                PropertyInfo prop = test.GetProperty("AllowedMethods");
                if (prop != null)
                {
                    var verbs = prop.GetValue(allowedVerbs) as IReadOnlyCollection<string> ?? new List<string>();
                    foreach (var verb in verbs)
                    {
                        constraints.Add(verb);
                    }
                }
            }
            return constraints;
        }
    }
}
