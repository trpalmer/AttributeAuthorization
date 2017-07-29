using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;

namespace AttributeAuthorization
{
    public class RoutePermissionsBuilder
    {
        internal class RouteInfo
        {
            public RouteInfo(string controllerName, string actionName)
            {
                ControllerName = controllerName;
                ActionName = actionName;
            }

            public string ControllerName { get; private set; }
            public string ActionName { get; private set; }
        }

        private readonly HttpConfiguration _configuration;
        private IHttpControllerSelector _controllerSelector;
        private Action<IHttpRoute, Dictionary<string, AuthPermissions>> _undefinedRoute;

        // consider: Methods that are decorated with RequiresAuth AND RequiresNoAuth. Current just falls back to not allowed
        // Add a callback on build, include a callback that would throw an exception on build?
        public RoutePermissionsBuilder(HttpConfiguration configuration, Action<IHttpRoute, Dictionary<string, AuthPermissions>> undefinedRouteAction = null)
        {
            configuration.CheckNull("configuration");
            _configuration = configuration;
            _undefinedRoute = undefinedRouteAction;
        }

        public Dictionary<string, AuthPermissions> Build()
        {
            InitializeFromConfiguration();
            return BuildPermissions();
        }

        private void InitializeFromConfiguration()
        {
            _controllerSelector = _configuration.Services.GetService(typeof(IHttpControllerSelector)) as IHttpControllerSelector;
            if (_controllerSelector == null)
            {
                throw new NullReferenceException(String.Format("Could not locate IHttpControllerSelector in configuration"));
            }
        }

        private Dictionary<string, AuthPermissions> BuildPermissions()
        {
            var map = new Dictionary<string, AuthPermissions>();
            foreach (var route in _configuration.Routes)
            {
                RouteInfo info;
                HttpControllerDescriptor controllerDescriptor;
                if (ExtractRouteInfo(route, out info) && FindControllerDescriptor(info, out controllerDescriptor))
                {
                    var mapAdditions = new Dictionary<string, AuthPermissions>();
                    var method = controllerDescriptor.ControllerType.GetMethod(info.ActionName);

                    bool isPublic = method.GetCustomAttributes<RequiresNoAuth>().Any() || controllerDescriptor.GetCustomAttributes<RequiresNoAuth>().Any();

                    var accepted = method.GetCustomAttributes<RequiresAuth>().SelectMany(a => a.Expand())
                        .Concat(controllerDescriptor.GetCustomAttributes<RequiresAuth>().SelectMany(a => a.Expand()))
                        .Distinct().ToList();

                    mapAdditions = GetMapForRoute(route, isPublic, accepted);
                    foreach (var addMap in mapAdditions)
                    {
                        if (!map.ContainsKey(addMap.Key))
                        {
                            map.Add(addMap.Key, addMap.Value);
                        }
                    }
                }
                else
                {
                    if (_undefinedRoute != null)
                    {
                        _undefinedRoute(route, map);
                    }
                }
            }
            return map;
        }

        private bool ExtractRouteInfo(IHttpRoute route, out RouteInfo info)
        {
            bool result = false;
            info = null;
            if (route.Defaults.ContainsKey("controller") && route.Defaults.ContainsKey("action"))
            {
                info = new RouteInfo(route.Defaults["controller"].ToString(), route.Defaults["action"].ToString());
                result = true;
            }
            return result;
        }

        private bool FindControllerDescriptor(RouteInfo info, out HttpControllerDescriptor descriptor)
        {
            descriptor = _controllerSelector.GetControllerMapping()
                .FirstOrDefault(s => String.Compare(s.Key, info.ControllerName, StringComparison.InvariantCultureIgnoreCase) == 0).Value;
            return descriptor != null;
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
