using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServiceKonekcija : Baza, IServiceKonekcija
    {
        public string UspostaviKonekciju(string username, string password)
        {


            if (System.IO.File.Exists("banovani_korisnici.txt"))
            {
                string[] lines = System.IO.File.ReadAllLines("banovani_korisnici.txt");

                foreach (string line in lines)
                {
                    if(line == username)
                    {
                        MyException e = new MyException
                        {
                            Greska = "Vi ste banovani korisnik!"
                        };

                        throw new FaultException<MyException>(e);
                    }
                }
            }



            bool validnost = Autentifikacija(username, password);

            if(validnost)
            {

                string uloga = Autorizacija(username);

                if(uloga == string.Empty)
                {
                    MyException e = new MyException
                    {
                        Greska = "Autorizacija je neuspesna!"
                    };

                    throw new FaultException<MyException>(e);
                }

                else
                {
                    if(EntitetiBaza[username].IsActive == true)
                    {
                        MyException e = new MyException
                        {
                            Greska = "Korisnik je vec ulogovan!"
                        };

                        throw new FaultException<MyException>(e);
                    }

                    EntitetiBaza[username].IsActive = true;
                    ActionLogs(uloga + " " + username + " se ulogovao");
                    return uloga;
                }
            }

            else
            {
                MyException e = new MyException
                {
                    Greska = "Autentifikacija je neuspesna!"
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
            file.WriteLine("");

            file.Close();
        }
        #endregion

        #region Autentifikacija
        private bool Autentifikacija(string username, string password)
        {


            bool validnost = false;

            foreach(KeyValuePair<string,Entitet> item in EntitetiBaza)
            {

                
                    if (item.Value.Username == username)
                    {

                        if (item.Value.Password == password)
                        {

                            validnost = true;
                            break;
                        }

                        else
                        {
                            break;
                        }
                    }
                
            }

            return validnost;

        }
        #endregion

        #region Autorizacija
        private string Autorizacija(string username)
        {
            string uloga = string.Empty;

            foreach(KeyValuePair<string, Entitet> item in EntitetiBaza)
            {
                if(item.Key == username)
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
