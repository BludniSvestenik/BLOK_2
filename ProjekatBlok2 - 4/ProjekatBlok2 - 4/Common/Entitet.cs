using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Common
{
    [DataContract]
    [KnownType(typeof(Admin))]
    [KnownType(typeof(Korisnik))]
    [KnownType(typeof(Nadzor))]
    public abstract class Entitet
    {
        private string username;
        private string password;
        private bool isActive = false;


        [DataMember]
        public bool IsActive { get => isActive; set => isActive = value; }
        [DataMember]
        public string Username { get => username; set => username = value; }
        [DataMember]
        public string Password { get => password; set => password = value; }

        public Entitet(string _username, string _password)
        {
            username = _username;
            password = _password;
        }
    }
}
