using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServiceNadzor : Baza, IServiceNadzor
    {
        public void SaljiPorukuAdminu(string username, string usernameAdmin, List<string> crnaLista)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Nadzor")
            {
                foreach(string item in crnaLista)
                {
                    if(!BazaPoruka[usernameAdmin].Contains(item))
                    {
                        BazaPoruka[usernameAdmin].Add(item);
                        ActionLogs("Nadzor " + username + " je poslao spisak korisnika za banovanje adminu " + usernameAdmin);
                    }
                }

            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao nadzor!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public void SaljiPorukuKorisniku(string username, string usernameKorisnik, string poruka)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Nadzor")
            {
                if(poruka == "UPOZORENJE")
                {
                    BazaPoruka[usernameKorisnik].Add(poruka);
                    ActionLogs("Nadzor " + username + " je poslao poruku korisniku " + usernameKorisnik + " tipa UPOZORENJE");
                }
                else if(poruka == "OPROSTENO")
                {
                    BazaPoruka[usernameKorisnik].RemoveAll(item => item == "UPOZORENJE");
                    ActionLogs("Nadzor " + username + " je poslao poruku korisniku " + usernameKorisnik + " tipa OPROSTENO");
                }
                else if(poruka == "BAN")
                {
                    BazaPoruka[usernameKorisnik].Add(poruka);
                    ActionLogs("Nadzor " + username + " je poslao poruku korisniku " + usernameKorisnik + " tipa BAN");
                }

            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao nadzor!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public Dictionary<string, List<string>> ZatraziListuSvihPutanjaFajlSistema(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Nadzor")
            {
                return BazaFajlSistema;
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao nadzor!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public List<string> ProveriListuPoruka(string username, string usernameKorisnik)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Nadzor")
            {

                List<string> retval = BazaPoruka[usernameKorisnik];

                return retval;
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao nadzor!"
                };

                throw new FaultException<MyException>(e);
            }

        }

        public void LogOut(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Nadzor")
            {

                EntitetiBaza[username].IsActive = false;
                ActionLogs("Nadzor " + username + " se izlogovao");
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao nadzor!"
                };

                throw new FaultException<MyException>(e);
            }
        }


        public Dictionary<string, int> PreuzmiParametre(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Nadzor")
            {

                return Parametri;
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao nadzor!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        #region Log
        private void ActionLogs(string actionDone)
        {
            StreamWriter file = new StreamWriter("actionLogs.txt", true);


            file.WriteLine("|================================================================================================================================================================================================================|");
            file.WriteLine("Dogadjaj: " + actionDone + " | Datum i Vreme: " + DateTime.Now.ToString());
            file.WriteLine("|================================================================================================================================================================================================================|");

            file.Close();
        }
        #endregion

        #region Autorizacija
        private string Autorizacija(string username)
        {
            string uloga = string.Empty;

            foreach (KeyValuePair<string, Entitet> item in EntitetiBaza)
            {
                if (item.Key == username)
                {
                    uloga = item.Value.GetType().Name;
                    break;
                }
            }


            return uloga;
        }

        
        #endregion
    }
}
