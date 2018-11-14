using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class ServiceAdmin : Baza, IServiceAdmin
    {
        public void ModifikujFolder(string username, string usernameKorisnik, string foldername, string newFoldername)
        {

            IPrincipal principal = Thread.CurrentPrincipal;

            if (!principal.IsInRole("Modify"))
            {
                MyException ex = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(ex);
            }

            string uloga = Autorizacija(username);

            if (uloga == "Admin")
            {

                if(!EntitetiBaza.ContainsKey(usernameKorisnik))
                {
                    MyException e = new MyException
                    {
                        Greska = "Ne postoji korisnik " + usernameKorisnik + "!"
                    };

                    throw new FaultException<MyException>(e);
                }

                string pathString = System.IO.Path.Combine(usernameKorisnik, foldername);
                string newPathString = System.IO.Path.Combine(usernameKorisnik, newFoldername);

                if(newPathString.Contains("\"") || newPathString.Contains("|") || newPathString.Contains("/") || newPathString.Contains(">") 
                    || newPathString.Contains("<") || newPathString.Contains("*") || newPathString.Contains("?") || newPathString.Contains(":"))
                {
                    MyException e = new MyException
                    {
                        Greska = "Format nije dobar!"
                    };

                    throw new FaultException<MyException>(e);
                }

                if (System.IO.Directory.Exists(pathString))
                {
                    System.IO.Directory.Move(pathString, newPathString);

                    int index = BazaFajlSistema[usernameKorisnik].IndexOf(pathString);
                    BazaFajlSistema[usernameKorisnik][index] = newPathString;
                    ActionLogs("Admin " + username + " " + "je preimenovao folder " + foldername + " korisnika " + usernameKorisnik + " u " + newFoldername);
                }

            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public void ModifikujTxt(string username, string usernameKorisnik, string filename, string noviSadrzaj)
        {

            IPrincipal principal = Thread.CurrentPrincipal;

            if (!principal.IsInRole("Modify"))
            {
                MyException ex = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(ex);
            }

            string uloga = Autorizacija(username);

            if (uloga == "Admin")
            {

                if (!EntitetiBaza.ContainsKey(usernameKorisnik))
                {
                    MyException e = new MyException
                    {
                        Greska = "Ne postoji korisnik " + usernameKorisnik + "!"
                    };

                    throw new FaultException<MyException>(e);
                }

                if (!filename.Contains(".txt"))
                {
                    filename += ".txt";
                }

                string pathString = System.IO.Path.Combine(usernameKorisnik, filename);

                if (pathString.Contains("\"") || pathString.Contains("|") || pathString.Contains("/") || pathString.Contains(">")
                    || pathString.Contains("<") || pathString.Contains("*") || pathString.Contains("?") || pathString.Contains(":"))
                {
                    MyException e = new MyException
                    {
                        Greska = "Format nije dobar!"
                    };

                    throw new FaultException<MyException>(e);
                }

                if (System.IO.File.Exists(pathString))
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(pathString, true))
                    {
                        file.WriteLine(noviSadrzaj);
                        ActionLogs("Admin " + username + " " + "je dodao novi sadrzaj: |" + noviSadrzaj + "| u " + filename + " koji pripada korisniku " + usernameKorisnik);
                    }
                }
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public void ObrisiFajlSistem(string username, string usernameKorisnik)
        {

            IPrincipal principal = Thread.CurrentPrincipal;

            if (!principal.IsInRole("Ban"))
            {
                MyException ex = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(ex);
            }

            string uloga = Autorizacija(username);

            if (uloga == "Admin")
            {

                foreach(KeyValuePair<string, Entitet> item in EntitetiBaza)
                {
                    if(item.Key == usernameKorisnik)
                    {
                        System.IO.DirectoryInfo di = new DirectoryInfo(usernameKorisnik);

                        foreach (FileInfo file in di.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in di.GetDirectories())
                        {
                            dir.Delete(true);
                        }

                        BazaFajlSistema[usernameKorisnik].RemoveAll(itemFS => itemFS != "NULL");
                        ActionLogs("Admin " + username + " je banovao i obrisao fajl sistem korisnika " + usernameKorisnik);

                        if (System.IO.File.Exists("banovani_korisnici.txt"))
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter("banovani_korisnici.txt", true))
                            {
                                file.WriteLine(usernameKorisnik);
                                return;
                            }
                        }
                        else
                        {
                            System.IO.File.Create("banovani_korisnici.txt").Close();
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter("banovani_korisnici.txt", true))
                            {
                                file.WriteLine(usernameKorisnik);
                                return;
                            }
                        }
                    }
                }

                MyException e = new MyException
                {
                    Greska = "Ne postoji taj korisnik!"
                };

                throw new FaultException<MyException>(e);

            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(e);
            }

        }

        public void ObrisiFolder(string username, string usernameKorisnik, string foldername)
        {

            IPrincipal principal = Thread.CurrentPrincipal;

            if (!principal.IsInRole("Delete"))
            {
                MyException ex = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(ex);
            }

            string uloga = Autorizacija(username);

            if (uloga == "Admin")
            {

                if (!EntitetiBaza.ContainsKey(usernameKorisnik))
                {
                    MyException e = new MyException
                    {
                        Greska = "Ne postoji korisnik " + usernameKorisnik + "!"
                    };

                    throw new FaultException<MyException>(e);
                }

                string pathString = System.IO.Path.Combine(usernameKorisnik, foldername);

                if (pathString.Contains("\"") || pathString.Contains("|")  || pathString.Contains("/") || pathString.Contains(">")
                    || pathString.Contains("<") || pathString.Contains("*") || pathString.Contains("?") || pathString.Contains(":"))
                {
                    MyException e = new MyException
                    {
                        Greska = "Format nije dobar!"
                    };

                    throw new FaultException<MyException>(e);
                }

                if (System.IO.Directory.Exists(pathString))
                {
                    System.IO.Directory.Delete(pathString);
                    BazaFajlSistema[usernameKorisnik].Remove(pathString);
                    ActionLogs("Admin " + username + " " + "je obrisao folder " + foldername + " korisnika " + usernameKorisnik);
                }
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public void ObrisiTxt(string username, string usernameKorisnik, string filename)
        {

            IPrincipal principal = Thread.CurrentPrincipal;

            if (!principal.IsInRole("Delete"))
            {
                MyException ex = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(ex);
            }

            string uloga = Autorizacija(username);

            if (uloga == "Admin")
            {

                if (!EntitetiBaza.ContainsKey(usernameKorisnik))
                {
                    MyException e = new MyException
                    {
                        Greska = "Ne postoji korisnik " + usernameKorisnik + "!"
                    };

                    throw new FaultException<MyException>(e);
                }

                if (!filename.Contains(".txt"))
                {
                    filename += ".txt";
                }

                string pathString = System.IO.Path.Combine(usernameKorisnik, filename);

                if (pathString.Contains("\"") || pathString.Contains("|") || pathString.Contains("/") || pathString.Contains(">")
                    || pathString.Contains("<") || pathString.Contains("*") || pathString.Contains("?") || pathString.Contains(":"))
                {
                    MyException e = new MyException
                    {
                        Greska = "Format nije dobar!"
                    };

                    throw new FaultException<MyException>(e);
                }

                if (System.IO.File.Exists(pathString))
                {
                    System.IO.File.Delete(pathString);
                    BazaFajlSistema[usernameKorisnik].Remove(pathString);
                    ActionLogs("Admin " + username + " " + "je obrisao txt " + filename + " korisnika " + usernameKorisnik);
                }
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public Dictionary<string, List<string>> ZatraziListuSvihPutanjaFajlSistema(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Admin")
            {
                ActionLogs("Admin " + username + " je zatrazio bazu fajl sistema");
                return BazaFajlSistema;
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public List<string> ProveriListuPoruka(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Admin")
            {

                return BazaPoruka[username];
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public void LogOut(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Admin")
            {

                EntitetiBaza[username].IsActive = false;
                ActionLogs("Admin " + username + " se izlogovao");
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao admin!"
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
