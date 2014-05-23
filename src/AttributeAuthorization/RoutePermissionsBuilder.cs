using System;
using System.Collections.Generic;
using System.Linq;
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
                    var method = controllerDescriptor.ControllerType.GetMethod(info.ActionName);

                    bool isPublic = method.GetCustomAttributes<RequiresNoAuth>().Any() || controllerDescriptor.GetCustomAttributes<RequiresNoAuth>().Any();

                    var accepted = method.GetCustomAttributes<RequiresAuth>().SelectMany(a => a.Expand())
                        .Concat(controllerDescriptor.GetCustomAttributes<RequiresAuth>().SelectMany(a => a.Expand()))
                        .Distinct().ToList();
                    map.Add(route.RouteTemplate, new AuthPermissions {  AuthNotRequired = isPublic, Accepted = accepted });
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
    }
}
