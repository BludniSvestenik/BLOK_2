using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Common
{
    [DataContract]
    public class Admin : Entitet
    {

        public Admin(string _username, string _password) : base(_username, _password) { }

    }
}
