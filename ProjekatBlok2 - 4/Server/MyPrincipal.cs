using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class MyPrincipal : IPrincipal
    {
        private static readonly Dictionary<string, List<string>> Relations = new Dictionary<string, List<string>>()
        {
            // GROUP == ROLE                   PERMISSIONS
            { $"{Environment.MachineName}\\ADMINISTRATOR_NA", new List<string>() { "Delete", "Read", "Modify", "Ban" } },
            { $"{Environment.MachineName}\\NADZOR_NA", new List<string>() { "Read" } },
            { $"{Environment.MachineName}\\KORISNIK_NA", new List<string>() { "Read", "Delete", "Modify" } },
        };

        public List<string> _permissions;

        public IIdentity Identity { get; private set; }

        public List<string> Permissions
        {
            get
            {                
                return _permissions;
            }

            private set
            {
                _permissions = value;
            }
        }

        public MyPrincipal(WindowsIdentity identity)
        {
            Identity = identity;
            Permissions = new List<string>();

            foreach (var group in (Identity as WindowsIdentity).Groups)
            {
                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                string name = sid.Translate(typeof(NTAccount)).ToString();

                if (Relations.ContainsKey(name))
                {
                    Relations[name].Where(p => !Permissions.Contains(p)).ToList().ForEach(p => Permissions.Add(p));
                }
            }
        }

        public bool IsInRole(string permission)
        {
            bool result = Permissions.Contains(permission);

            
            return result;
        }
    }
}
