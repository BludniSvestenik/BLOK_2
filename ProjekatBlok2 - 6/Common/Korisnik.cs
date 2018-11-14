using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Common
{
    [DataContract]
    public class Korisnik : Entitet
    {
        [DataMember]
        public List<string> MojiKolaboratori = new List<string>();
        [DataMember]
        public List<string> StraniKolaboratori = new List<string>();

        public Korisnik(string _username, string _password) : base(_username, _password) { }
        
        void DodajKolaboratora(string Novi)
        {
            foreach(string item in MojiKolaboratori)
            {
                if(item == Novi)
                {
                    MyException e = new MyException
                    {
                        Greska = "Korisnik " + Novi + " vec postoji kao kolaborator!"
                    };

                    throw new FaultException<MyException>(e);
                }

                else if(Novi == Username)
                {
                    MyException e = new MyException
                    {
                        Greska = "Korisnik " + Novi + " ste vi!"
                    };

                    throw new FaultException<MyException>(e);
                }
            }

            MojiKolaboratori.Add(Novi);
        }

        void DodajStranogKolaboratora(string Novi)
        {
            foreach (string item in StraniKolaboratori)
            {
                if (item == Novi)
                {
                    MyException e = new MyException
                    {
                        Greska = "Korisnik " + Novi + " vec postoji kao strani kolaborator!"
                    };

                    throw new FaultException<MyException>(e);
                }

                else if (Novi == Username)
                {
                    MyException e = new MyException
                    {
                        Greska = "Korisnik " + Novi + " ste vi!"
                    };

                    throw new FaultException<MyException>(e);
                }
            }

            StraniKolaboratori.Add(Novi);
        }
    }
}
