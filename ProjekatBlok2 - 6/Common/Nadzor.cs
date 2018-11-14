using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Common
{
    [DataContract]
    public class Nadzor : Entitet
    {

        public Nadzor(string _username, string _password) : base(_username, _password) { }
        

        
    }
}
