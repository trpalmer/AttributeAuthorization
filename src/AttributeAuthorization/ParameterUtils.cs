using System;

namespace AttributeAuthorization
{
    public static class ParameterUtils
    {
        public static void CheckNull(this object obj, string paramName = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void CheckNullOrEmpty(this string s, string paramName = null, bool allowWhitespace = false)
        {
            s.CheckNull(paramName);
            Func<string, bool> checkFunc = allowWhitespace ? (Func<string, bool>)String.IsNullOrEmpty : String.IsNullOrWhiteSpace;

            if (checkFunc(s))
            {
                throw new ArgumentException("Empty strings are not allowed", paramName);
            }
        }
    }
}
