using System;

namespace AttributeAuthorization
{
    /// <summary>
    /// Marker attribute to declare a method requires no authorization (e.g. is public)
    /// 
    /// If the same method or class is attributed with RequiresAuth, this attribute
    /// will be ignored and auth will be required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequiresNoAuth : Attribute {}
}
