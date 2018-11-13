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
    public class ServiceKorisnik : Baza, IServiceKorisnik
    {
        public List<string> TrenutniSadrzajFajlSistema(string username, string kolab)
        {
            string uloga = Autorizacija(username);

            if(uloga == "Korisnik")
            {

                if(kolab != "NULL")
                {

                    if (!EntitetiBaza.ContainsKey(kolab))
                    {
                        MyException e = new MyException
                        {
                            Greska = "Ne postoji korisnik" + kolab + "!"
                        };

                        throw new FaultException<MyException>(e);
                    }



                    if (((Korisnik)EntitetiBaza[username]).StraniKolaboratori.Contains(kolab))
                    {
                        if(((Korisnik)EntitetiBaza[kolab]).MojiKolaboratori.Contains(username))
                        {
                            ActionLogs("Korisnik " + username + " je zatrazio pregled fajl sistema korisnika " + kolab);
                            return BazaFajlSistema[kolab];
                        }
                        else
                        {
                            MyException e = new MyException
                            {
                                Greska = "Vi niste kolaborator korisnika " + kolab
                            };

                            throw new FaultException<MyException>(e);
                        }
                    }
                    else
                    {
                        MyException e = new MyException
                        {
                            Greska = "Vi niste kolaborator korisnika " + kolab
                        };

                        throw new FaultException<MyException>(e);
                    }


                    
                }
                ActionLogs("Korisnik " + username + " je zatrazio pregled svog fajl sistema");
                return BazaFajlSistema[username];
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao korisnik!"
                };

                throw new FaultException<MyException>(e);
            }
            
        }

        public void DodajKolaboratora(string username, string novi)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Korisnik")
            {

                foreach(KeyValuePair<string, Entitet> item in EntitetiBaza)
                {
                    if (item.Value.Username == novi)
                    {
                        ((Korisnik)EntitetiBaza[username]).MojiKolaboratori.Add(novi);
                        ((Korisnik)EntitetiBaza[novi]).StraniKolaboratori.Add(username);
                        ActionLogs("Korisnik " + username + " je dodao korisnika " + novi + " za svog kolaboratora");
                        return;
                       
                    }
                }

                MyException e = new MyException
                {
                    Greska = "Korisnik " + novi + " ne postoji u bazi!"
                };

                throw new FaultException<MyException>(e);
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao Korisnik!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public List<string> PregledKolaboratora(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Korisnik")
            {
                List<string> listaKolaboratora = new List<string>();
                
                foreach(string item in ((Korisnik)EntitetiBaza[username]).MojiKolaboratori)
                {
                    listaKolaboratora.Add(item);
                }

                listaKolaboratora.Add("PREKID");

                foreach(string item in ((Korisnik)EntitetiBaza[username]).StraniKolaboratori)
                {
                    listaKolaboratora.Add(item);
                }

                ActionLogs("Korisnik " + username + " je zatrazio pregled svojih kolaboratora");
                return listaKolaboratora;
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao Korisnik!"
                };

                throw new FaultException<MyException>(e);
            }

        }

        public void KreirajFolder(string username, string foldername, string kolab)
        {


            using ((Thread.CurrentPrincipal.Identity as WindowsIdentity).Impersonate())
            {


                try
                {
                    IPrincipal principal = Thread.CurrentPrincipal;// as MyPrincipal;

                    if (!principal.IsInRole("Modify"))
                    {
                        MyException ex = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(ex);
                    }

                    string uloga = Autorizacija(username);

                    if (uloga == "Korisnik")
                    {


                        if (kolab != "NULL")
                        {

                            if (!EntitetiBaza.ContainsKey(kolab))
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Ne postoji korisnik" + kolab + "!"
                                };

                                throw new FaultException<MyException>(e);
                            }

                            if (((Korisnik)EntitetiBaza[username]).StraniKolaboratori.Contains(kolab))
                            {
                                if (((Korisnik)EntitetiBaza[kolab]).MojiKolaboratori.Contains(username))
                                {
                                    string pathStringKolab = System.IO.Path.Combine(kolab, foldername);

                                    if (pathStringKolab.Contains("\"") || pathStringKolab.Contains("|") || pathStringKolab.Contains("/") || pathStringKolab.Contains(">")
                                        || pathStringKolab.Contains("<") || pathStringKolab.Contains("*") || pathStringKolab.Contains("?") || pathStringKolab.Contains(":"))
                                    {
                                        MyException e = new MyException
                                        {
                                            Greska = "Format nije dobar!"
                                        };

                                        throw new FaultException<MyException>(e);
                                    }


                                    System.IO.Directory.CreateDirectory(pathStringKolab);

                                    BazaFajlSistema[kolab].Add(pathStringKolab);

                                    ActionLogs("Korisnik " + username + " je kreirao folder: " + foldername + " kolaboratoru " + kolab);

                                    return;

                                }
                                else
                                {
                                    MyException e = new MyException
                                    {
                                        Greska = "Vi niste kolaborator korisnika " + kolab
                                    };

                                    throw new FaultException<MyException>(e);
                                }
                            }
                            else
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Vi niste kolaborator korisnika " + kolab
                                };

                                throw new FaultException<MyException>(e);
                            }
                        }

                        string pathString = System.IO.Path.Combine(username, foldername);

                        if (pathString.Contains("\"") || pathString.Contains("|") || pathString.Contains("/") || pathString.Contains(">")
                                        || pathString.Contains("<") || pathString.Contains("*") || pathString.Contains("?") || pathString.Contains(":"))
                        {
                            MyException e = new MyException
                            {
                                Greska = "Format nije dobar!"
                            };

                            throw new FaultException<MyException>(e);
                        }

                        System.IO.Directory.CreateDirectory(pathString);

                        BazaFajlSistema[username].Add(pathString);

                        ActionLogs("Korisnik " + username + " je kreirao folder: " + foldername);
                    }
                    else
                    {
                        MyException e = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(e);
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    MyException ex = new MyException
                    {
                        Greska = "Niste autorizovani kao Korisnik!"
                    };

                    throw new FaultException<MyException>(ex);
                }

            }

        }

        public void KreirajTxt(string username, string filename, string kolab)
        {
            using ((Thread.CurrentPrincipal.Identity as WindowsIdentity).Impersonate())
            {
                try
                {

                    IPrincipal principal = Thread.CurrentPrincipal;// as MyPrincipal;

                    if (!principal.IsInRole("Modify"))
                    {
                        MyException ex = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(ex);
                    }

                    string uloga = Autorizacija(username);

                    if (uloga == "Korisnik")
                    {
                        if (!filename.Contains(".txt"))
                        {
                            filename += ".txt";
                        }

                        if (kolab != "NULL")
                        {

                            if (!EntitetiBaza.ContainsKey(kolab))
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Ne postoji korisnik" + kolab + "!"
                                };

                                throw new FaultException<MyException>(e);
                            }

                            if (((Korisnik)EntitetiBaza[username]).StraniKolaboratori.Contains(kolab))
                            {
                                if (((Korisnik)EntitetiBaza[kolab]).MojiKolaboratori.Contains(username))
                                {
                                    string pathStringKolab = System.IO.Path.Combine(kolab, filename);

                                    if (pathStringKolab.Contains("\"") || pathStringKolab.Contains("|") || pathStringKolab.Contains("/") || pathStringKolab.Contains(">")
                                        || pathStringKolab.Contains("<") || pathStringKolab.Contains("*") || pathStringKolab.Contains("?") || pathStringKolab.Contains(":"))
                                    {
                                        MyException e = new MyException
                                        {
                                            Greska = "Format nije dobar!"
                                        };

                                        throw new FaultException<MyException>(e);
                                    }

                                    System.IO.File.Create(pathStringKolab).Close();

                                    BazaFajlSistema[kolab].Add(pathStringKolab);

                                    ActionLogs("Korisnik " + username + " je kreirao txt: " + filename + " kolaboratoru " + kolab);

                                    return;

                                }
                                else
                                {
                                    MyException e = new MyException
                                    {
                                        Greska = "Vi niste kolaborator korisnika " + kolab
                                    };

                                    throw new FaultException<MyException>(e);
                                }
                            }
                            else
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Vi niste kolaborator korisnika " + kolab
                                };

                                throw new FaultException<MyException>(e);
                            }

                        }

                        string pathString = System.IO.Path.Combine(username, filename);

                        if (pathString.Contains("\"") || pathString.Contains("|") || pathString.Contains("/") || pathString.Contains(">")
                                        || pathString.Contains("<") || pathString.Contains("*") || pathString.Contains("?") || pathString.Contains(":"))
                        {
                            MyException e = new MyException
                            {
                                Greska = "Format nije dobar!"
                            };

                            throw new FaultException<MyException>(e);
                        }

                        System.IO.File.Create(pathString).Close();

                        BazaFajlSistema[username].Add(pathString);

                        ActionLogs("Korisnik " + username + " je kreirao txt: " + filename);
                    }
                    else
                    {
                        MyException e = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(e);
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    MyException ex = new MyException
                    {
                        Greska = "Niste autorizovani kao Korisnik!"
                    };

                    throw new FaultException<MyException>(ex);
                }

            }
        }

        public void ModifikujTxt(string username, string filename, string noviSadrzaj, string kolab)
        {
            using ((Thread.CurrentPrincipal.Identity as WindowsIdentity).Impersonate())
            {
                try
                {

                    IPrincipal principal = Thread.CurrentPrincipal;// as MyPrincipal;

                    if (!principal.IsInRole("Modify"))
                    {
                        MyException ex = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(ex);
                    }

                    string uloga = Autorizacija(username);

                    if (uloga == "Korisnik")
                    {

                        if (kolab != "NULL")
                        {

                            if (!EntitetiBaza.ContainsKey(kolab))
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Ne postoji korisnik" + kolab + "!"
                                };

                                throw new FaultException<MyException>(e);
                            }

                            if (((Korisnik)EntitetiBaza[username]).StraniKolaboratori.Contains(kolab))
                            {
                                if (((Korisnik)EntitetiBaza[kolab]).MojiKolaboratori.Contains(username))
                                {
                                    if (!filename.Contains(".txt"))
                                    {
                                        filename += ".txt";
                                    }

                                    string pathStringKolab = System.IO.Path.Combine(kolab, filename);

                                    if (pathStringKolab.Contains("\"") || pathStringKolab.Contains("|") || pathStringKolab.Contains("/") || pathStringKolab.Contains(">")
                                        || pathStringKolab.Contains("<") || pathStringKolab.Contains("*") || pathStringKolab.Contains("?") || pathStringKolab.Contains(":"))
                                    {
                                        MyException e = new MyException
                                        {
                                            Greska = "Format nije dobar!"
                                        };

                                        throw new FaultException<MyException>(e);
                                    }

                                    if (System.IO.File.Exists(pathStringKolab))
                                    {
                                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(pathStringKolab, true))
                                        {
                                            file.WriteLine(noviSadrzaj);
                                            ActionLogs("Korisnik " + username + " " + "je dodao novi sadrzaj: |" + noviSadrzaj + "| u " + filename + " koji pripada korisniku kolaboratoru " + kolab);
                                        }
                                    }

                                    return;

                                }
                                else
                                {
                                    MyException e = new MyException
                                    {
                                        Greska = "Vi niste kolaborator korisnika " + kolab
                                    };

                                    throw new FaultException<MyException>(e);
                                }
                            }
                            else
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Vi niste kolaborator korisnika " + kolab
                                };

                                throw new FaultException<MyException>(e);
                            }


                        }

                        if (!filename.Contains(".txt"))
                        {
                            filename += ".txt";
                        }

                        string pathString = System.IO.Path.Combine(username, filename);

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
                                ActionLogs("Korisnik " + username + " " + "je dodao novi sadrzaj: |" + noviSadrzaj + "| u " + filename);
                            }
                        }
                    }
                    else
                    {
                        MyException e = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(e);
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    MyException ex = new MyException
                    {
                        Greska = "Niste autorizovani kao Korisnik!"
                    };

                    throw new FaultException<MyException>(ex);
                }
            }
        }

        public void ModifikujFolder(string username, string foldername, string newFoldername, string kolab)
        {
            using ((Thread.CurrentPrincipal.Identity as WindowsIdentity).Impersonate())
            {
                try
                {

                    IPrincipal principal = Thread.CurrentPrincipal;// as MyPrincipal;

                    if (!principal.IsInRole("Modify"))
                    {
                        MyException ex = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(ex);
                    }

                    string uloga = Autorizacija(username);

                    if (uloga == "Korisnik")
                    {

                        if (kolab != "NULL")
                        {

                            if (!EntitetiBaza.ContainsKey(kolab))
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Ne postoji korisnik" + kolab + "!"
                                };

                                throw new FaultException<MyException>(e);
                            }

                            if (((Korisnik)EntitetiBaza[username]).StraniKolaboratori.Contains(kolab))
                            {
                                if (((Korisnik)EntitetiBaza[kolab]).MojiKolaboratori.Contains(username))
                                {
                                    string pathStringKolab = System.IO.Path.Combine(kolab, foldername);
                                    string newPathStringKolab = System.IO.Path.Combine(kolab, newFoldername);

                                    if (newPathStringKolab.Contains("\"") || newPathStringKolab.Contains("|") || newPathStringKolab.Contains("/") || newPathStringKolab.Contains(">")
                                        || newPathStringKolab.Contains("<") || newPathStringKolab.Contains("*") || newPathStringKolab.Contains("?") || newPathStringKolab.Contains(":"))
                                    {
                                        MyException e = new MyException
                                        {
                                            Greska = "Format nije dobar!"
                                        };

                                        throw new FaultException<MyException>(e);
                                    }

                                    if (System.IO.Directory.Exists(pathStringKolab))
                                    {
                                        System.IO.Directory.Move(pathStringKolab, newPathStringKolab);

                                        int index = BazaFajlSistema[kolab].IndexOf(pathStringKolab);
                                        BazaFajlSistema[kolab][index] = newPathStringKolab;
                                        ActionLogs("Korisnik " + username + " " + "je preimenovao folder " + foldername + " korisnika kolaboratora" + kolab + " u " + newFoldername);
                                    }


                                    return;

                                }
                                else
                                {
                                    MyException e = new MyException
                                    {
                                        Greska = "Vi niste kolaborator korisnika " + kolab
                                    };

                                    throw new FaultException<MyException>(e);
                                }
                            }
                            else
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Vi niste kolaborator korisnika " + kolab
                                };

                                throw new FaultException<MyException>(e);
                            }

                        }


                        string pathString = System.IO.Path.Combine(username, foldername);
                        string newPathString = System.IO.Path.Combine(username, newFoldername);

                        if (newPathString.Contains("\"") || newPathString.Contains("|") || newPathString.Contains("/") || newPathString.Contains(">")
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

                            int index = BazaFajlSistema[username].IndexOf(pathString);
                            BazaFajlSistema[username][index] = newPathString;
                            ActionLogs("Korisnik " + username + " " + "je preimenovao folder " + foldername + " u " + newFoldername);
                        }

                    }
                    else
                    {
                        MyException e = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(e);
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    MyException ex = new MyException
                    {
                        Greska = "Niste autorizovani kao Korisnik!"
                    };

                    throw new FaultException<MyException>(ex);
                }
            }
        }

        public void ObrisiFolder(string username, string foldername, string kolab)
        {
            using ((Thread.CurrentPrincipal.Identity as WindowsIdentity).Impersonate())
            {
                try
                {

                    IPrincipal principal = Thread.CurrentPrincipal;// as MyPrincipal;

                    if (!principal.IsInRole("Delete"))
                    {
                        MyException ex = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(ex);
                    }

                    string uloga = Autorizacija(username);

                    if (uloga == "Korisnik")
                    {


                        if (kolab != "NULL")
                        {

                            if (!EntitetiBaza.ContainsKey(kolab))
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Ne postoji korisnik" + kolab + "!"
                                };

                                throw new FaultException<MyException>(e);
                            }

                            if (((Korisnik)EntitetiBaza[username]).StraniKolaboratori.Contains(kolab))
                            {
                                if (((Korisnik)EntitetiBaza[kolab]).MojiKolaboratori.Contains(username))
                                {
                                    string pathStringKolab = System.IO.Path.Combine(kolab, foldername);

                                    if (pathStringKolab.Contains("\"") || pathStringKolab.Contains("|") || pathStringKolab.Contains("/") || pathStringKolab.Contains(">")
                                        || pathStringKolab.Contains("<") || pathStringKolab.Contains("*") || pathStringKolab.Contains("?") || pathStringKolab.Contains(":"))
                                    {
                                        MyException e = new MyException
                                        {
                                            Greska = "Format nije dobar!"
                                        };

                                        throw new FaultException<MyException>(e);
                                    }

                                    if (System.IO.Directory.Exists(pathStringKolab))
                                    {
                                        System.IO.Directory.Delete(pathStringKolab);
                                        BazaFajlSistema[kolab].Remove(pathStringKolab);
                                        ActionLogs("Korisnik " + username + " " + "je obrisao folder " + foldername + " korisnika kolaboratora" + kolab);
                                    }

                                    return;

                                }
                                else
                                {
                                    MyException e = new MyException
                                    {
                                        Greska = "Vi niste kolaborator korisnika " + kolab
                                    };

                                    throw new FaultException<MyException>(e);
                                }
                            }
                            else
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Vi niste kolaborator korisnika " + kolab
                                };

                                throw new FaultException<MyException>(e);
                            }

                        }

                        string pathString = System.IO.Path.Combine(username, foldername);

                        if (pathString.Contains("\"") || pathString.Contains("|") || pathString.Contains("/") || pathString.Contains(">")
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
                            BazaFajlSistema[username].Remove(pathString);
                            ActionLogs("Korisnik " + username + " " + "je obrisao folder " + foldername);
                        }
                    }
                    else
                    {
                        MyException e = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(e);
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    MyException ex = new MyException
                    {
                        Greska = "Niste autorizovani kao Korisnik!"
                    };

                    throw new FaultException<MyException>(ex);
                }
            }
        }

        public void ObrisiTxt(string username, string filename, string kolab)
        {
            using ((Thread.CurrentPrincipal.Identity as WindowsIdentity).Impersonate())
            {
                try
                {

                    IPrincipal principal = Thread.CurrentPrincipal;// as MyPrincipal;

                    if (!principal.IsInRole("Delete"))
                    {
                        MyException ex = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(ex);
                    }

                    string uloga = Autorizacija(username);

                    if (uloga == "Korisnik")
                    {

                        if (kolab != "NULL")
                        {

                            if (!EntitetiBaza.ContainsKey(kolab))
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Ne postoji korisnik" + kolab + "!"
                                };

                                throw new FaultException<MyException>(e);
                            }


                            if (((Korisnik)EntitetiBaza[username]).StraniKolaboratori.Contains(kolab))
                            {
                                if (((Korisnik)EntitetiBaza[kolab]).MojiKolaboratori.Contains(username))
                                {
                                    if (!filename.Contains(".txt"))
                                    {
                                        filename += ".txt";
                                    }

                                    string pathStringKolab = System.IO.Path.Combine(kolab, filename);

                                    if (pathStringKolab.Contains("\"") || pathStringKolab.Contains("|") || pathStringKolab.Contains("/") || pathStringKolab.Contains(">")
                                        || pathStringKolab.Contains("<") || pathStringKolab.Contains("*") || pathStringKolab.Contains("?") || pathStringKolab.Contains(":"))
                                    {
                                        MyException e = new MyException
                                        {
                                            Greska = "Format nije dobar!"
                                        };

                                        throw new FaultException<MyException>(e);
                                    }

                                    if (System.IO.File.Exists(pathStringKolab))
                                    {
                                        System.IO.File.Delete(pathStringKolab);
                                        BazaFajlSistema[kolab].Remove(pathStringKolab);
                                        ActionLogs("Korisnik " + username + " " + "je obrisao txt " + filename + " korisnika kolaboratora " + kolab);
                                    }

                                    return;

                                }
                                else
                                {
                                    MyException e = new MyException
                                    {
                                        Greska = "Vi niste kolaborator korisnika " + kolab
                                    };

                                    throw new FaultException<MyException>(e);
                                }
                            }
                            else
                            {
                                MyException e = new MyException
                                {
                                    Greska = "Vi niste kolaborator korisnika " + kolab
                                };

                                throw new FaultException<MyException>(e);
                            }

                        }


                        if (!filename.Contains(".txt"))
                        {
                            filename += ".txt";
                        }

                        string pathString = System.IO.Path.Combine(username, filename);

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
                            BazaFajlSistema[username].Remove(pathString);
                            ActionLogs("Korisnik " + username + " " + "je obrisao txt " + filename);
                        }
                    }
                    else
                    {
                        MyException e = new MyException
                        {
                            Greska = "Niste autorizovani kao Korisnik!"
                        };

                        throw new FaultException<MyException>(e);
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    MyException ex = new MyException
                    {
                        Greska = "Niste autorizovani kao Korisnik!"
                    };

                    throw new FaultException<MyException>(ex);
                }
            }
        }

        public List<string> ZatraziSpisakKorisnika(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Korisnik")
            {
                List<string> listaKorisnika = new List<string>();
                List<Entitet> listaEntiteta = EntitetiBaza.Values.ToList();

                foreach (KeyValuePair<string, Entitet> item in EntitetiBaza)
                {
                    if (item.Value.GetType().Name == "Korisnik")
                    {
                        listaKorisnika.Add(item.Value.Username);
                    }
                }

                ActionLogs("Korisnik " + username + " je zatrazio bazu svih korisnika");
                return listaKorisnika;
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao Korisnik!"
                };

                throw new FaultException<MyException>(e);
            }
        }

        public List<string> ProveriListuPoruka(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Korisnik")
            {

                List<string> retval = BazaPoruka[username];

                return retval;
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao Korisnik!"
                };

                throw new FaultException<MyException>(e);
            }
            
        }
        public void LogOut(string username)
        {
            string uloga = Autorizacija(username);

            if (uloga == "Korisnik")
            {

                EntitetiBaza[username].IsActive = false;
                ActionLogs("Korisnik " + username + " se izlogovao");
            }
            else
            {
                MyException e = new MyException
                {
                    Greska = "Niste autorizovani kao Korisnik!"
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
