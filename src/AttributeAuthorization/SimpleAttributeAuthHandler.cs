using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AttributeAuthorization
{
    public class SimpleAttributeAuthHandler : DelegatingHandler
    {
        private readonly AuthRoutePermissions _routePermissions;

        public SimpleAttributeAuthHandler(Dictionary<string, AuthPermissions> routePermissions, Func<HttpRequestMessage, IEnumerable<string>> authResolver,
            Func<HttpRequestMessage, bool> allowNotDefined = null)
        {
            _routePermissions = new AuthRoutePermissions(routePermissions, authResolver, allowNotDefined);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (!_routePermissions.IsAllowed(request))
            {
                return SendUnauthorizedResponse();
            }

            return base.SendAsync(request, token);
        }

        public virtual Task<HttpResponseMessage> SendUnauthorizedResponse(string content = null)
        {
            return Task<HttpResponseMessage>.Factory.StartNew(() =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                if (!String.IsNullOrEmpty(content))
                {
                    response.Content = new StringContent(content);
                }
                return response;
            });
        }
    }

}
