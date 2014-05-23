using System.Collections.Generic;

namespace AttributeAuthorization
{
    public class AuthPermissions
    {
        public AuthPermissions()
        {
            Accepted = new List<string>();
        }

        public bool AuthNotRequired { get; set; }
        public List<string> Accepted { get; set; }
    }
}
