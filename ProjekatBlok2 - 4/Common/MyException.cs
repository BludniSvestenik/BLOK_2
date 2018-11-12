using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Common
{
    [DataContract]
    public class MyException
    {
        [DataMember]
        public string Greska { get; set; }
    }
}
