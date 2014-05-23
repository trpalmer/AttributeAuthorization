using System;
using System.Collections.Generic;
using System.Linq;

namespace AttributeAuthorization
{
    /// <summary>
    /// Attribute to define an authorization for a method. If the incoming request
    /// contains the authorization defined, it is allowed.
    /// 
    /// Sub-authorizations are supported and automatically expanded with the authorization:subAuthorization 
    /// notation. If the allowed authorization defined is metrics:read, the method will be
    /// allowed if the incomging request contains either metrics
    /// (the parent authorization) or the exact authorization metrics:read.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequiresAuth : Attribute
    {
        public RequiresAuth(string allowedIfHas)
        {
            AllowedIfHas = allowedIfHas ?? String.Empty;
        }

        public string AllowedIfHas { get; set; }

        public IEnumerable<string> Expand()
        {
            var result = new List<string> { AllowedIfHas };
            if (AllowedIfHas.Contains(":"))
            {
                var parent = AllowedIfHas.Split(new[] { ':' }).FirstOrDefault();
                if (!String.IsNullOrEmpty(parent))
                {
                    result.Insert(0, parent);
                }
            }
            return result;
        }
    }
}
