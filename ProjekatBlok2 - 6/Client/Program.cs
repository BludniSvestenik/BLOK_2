using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Principal;

namespace Client
{
    class Program
    {

        static string _username = string.Empty;

        static void Main(string[] args)
        {
            string uloga = string.Empty;


            uloga = Konekcija();           

            if (uloga == "GRESKA")
            {
                Console.WriteLine("Neuspesna autentifikacija!");
                Console.WriteLine("Pokrenite ponovo program!");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Ulogovani ste kao: " + uloga.ToUpper());

            switch (uloga)
            {
                case "Korisnik":
                    KorisnikPetlja();
                    break;
                case "Nadzor":
                    NadzorPetlja();
                    break;
                case "Admin":
                    AdminPetlja();
                    break;
                default:
                    Console.WriteLine("Greska u meniju!");
                    break;
            }

            Console.ReadLine();

        }

        #region Inicijalna_Konekcija
        static string Konekcija()
        {
            IServiceKonekcija _konekcija;

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            ChannelFactory<IServiceKonekcija> factory = new ChannelFactory<IServiceKonekcija>(
                                                            binding,
                                                            new EndpointAddress("net.tcp://localhost:6018/IServiceKonekcija"));
           

            _konekcija = factory.CreateChannel();

            try
            {
                Console.Write("Unesite korisnicko ime: ");
                _username = Console.ReadLine();
                Console.Write("Unesite lozinku: ");
                string _password = Console.ReadLine();

                string uloga = string.Empty;

                string ulogaWindows = WindowsIdentity.GetCurrent().Name;

                Console.WriteLine("Windows nalog: " + ulogaWindows);

                string[] parsiranaWindowsUloga = ulogaWindows.Split('\\');

                if (_username != parsiranaWindowsUloga[1])
                {
                    uloga = "GRESKA";

                    return uloga;
                }

                uloga = _konekcija.UspostaviKonekciju(_username, _password);

                

                factory.Close();

                return uloga;
            }

            catch (FaultException<MyException> ex)
            {
                Console.WriteLine("Error : " + ex.Detail.Greska);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error : " + e.Message);
            }

            factory.Close();

            return "GRESKA";
        }
        #endregion

        #region Korisnik_Petlja
        static void KorisnikPetlja()
        {
            IServiceKorisnik _konekcijaKorisnik;

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            ChannelFactory<IServiceKorisnik> factory = new ChannelFactory<IServiceKorisnik>(
                                                            binding,
                                                            new EndpointAddress("net.tcp://localhost:6015/IServiceKorisnik"));

            factory.Credentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

            _konekcijaKorisnik = factory.CreateChannel();


            bool kraj = true;

            //PROVERA PORUKE OD NADZORA
            #region Thread
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                List<string> poruke = new List<string>();
                Dictionary<string, int> Parametri = _konekcijaKorisnik.PreuzmiParametre(_username);

                while (kraj)
                {
                    try
                    {
                        poruke = _konekcijaKorisnik.ProveriListuPoruka(_username);

                        if (poruke.Count != 0)
                        {
                            int vreme = Parametri["Vreme"];
                            Console.WriteLine("-------------------------------------------------------------------------------");
                            Console.WriteLine("--------------------------------UPOZORENJE-------------------------------------");
                            Console.WriteLine("-------------------------------------------------------------------------------");
                            Console.WriteLine("Imate " + Parametri["Vreme"] + " sekundi da smanjite broj foldera ili txt fajlova u vasem fajl sistemu");

                            while (vreme != 0)
                            {
                                Thread.Sleep(1000);
                                List<string> trenutniFajlSistem = _konekcijaKorisnik.TrenutniSadrzajFajlSistema(_username, "NULL");


                                if (trenutniFajlSistem.Count > Parametri["MaxBrojFajlova"])
                                {
                                    vreme--;

                                    if (vreme == 0)
                                    {
                                        kraj = false;
                                        Console.WriteLine("ADMIN CE VAS USKORO BANOVATI!");
                                        break;
                                    }
                                    continue;
                                }

                                else
                                {
                                    Console.WriteLine("-------------------------------------------------------------------------------");
                                    Console.WriteLine("Uspesno ste oslobodili vas fajl sistem, sada ste sigurni!");
                                    Thread.Sleep(3000);
                                    break;
                                }
                            }
                        }
                    }
                    catch (FaultException<MyException> ex)
                    {
                        Console.WriteLine("Error : " + ex.Detail.Greska);
                        Thread.Sleep(3000);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error : " + e.Message);
                        Thread.Sleep(3000);
                    }

                }
                
            }).Start(); 
            #endregion

            //GLAVNA PETLJA//
            while (kraj)
            {
                MeniKorisnik();

                string ulaz = Console.ReadLine();

                if(kraj == false)
                {
                    _konekcijaKorisnik.LogOut(_username); //Ukoliko je korisnik banovan
                    break;
                }


                switch(ulaz)
                {
                    //TRENUTNI SADRZAJ FAJL SISTEMA
                    case "1":
                        {

                            Console.Clear();
                            Console.Write("Da li zelite da vidite svoj fajl sistem ili kolaboratorski fajl sistem? (S|K)");
                            string odg = Console.ReadLine();

                            if (odg.ToUpper() == "S")
                            {
                                try
                                {
                                    Console.Clear();
                                    List<string> linkoviDoSadrzajaFoldera = _konekcijaKorisnik.TrenutniSadrzajFajlSistema(_username, "NULL");

                                    foreach (string item in linkoviDoSadrzajaFoldera)
                                    {
                                        if (!item.Contains(".txt"))
                                        {
                                            Console.WriteLine("Folder: " + item);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Txt:    " + item);
                                        }

                                    }

                                    Console.WriteLine("----------------------------------------------");
                                    Console.WriteLine("Pritisnite bilo koje dugme za povratak na meni");
                                    Console.ReadLine();
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            else if (odg.ToUpper() == "K")
                            {
                                
                                Console.Write("Unesite username korisnika koji vas je proglasio za kolaboratora: ");
                                string kolab = Console.ReadLine();
                                Console.Clear();

                                try
                                {
                                    List<string> linkoviDoSadrzajaFoldera = _konekcijaKorisnik.TrenutniSadrzajFajlSistema(_username, kolab);

                                    foreach (string item in linkoviDoSadrzajaFoldera)
                                    {
                                        if (!item.Contains(".txt"))
                                        {
                                            Console.WriteLine("Folder: " + item);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Txt:    " + item);
                                        }

                                    }

                                    Console.WriteLine("----------------------------------------------");
                                    Console.WriteLine("Pritisnite bilo koje dugme za povratak na meni");
                                    Console.ReadLine();
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }

                            else
                            {
                                Console.WriteLine("Pogresan unos!");
                            }


                        }                       
                        break;
                    
                    //KREIRAJ FOLDER
                    case "2":
                        {
                            Console.Clear();

                            Console.Write("Da li zelite da modifikujete svoj fajl sistem ili kolaboratorski fajl sistem? (S|K)");
                            string odg = Console.ReadLine();

                            if(odg.ToUpper() == "S")
                            {
                                Console.Clear();
                                Console.Write("Unesite ime foldera: ");
                                string imeFoldera = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.KreirajFolder(_username, imeFoldera, "NULL");
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }

                            else if(odg.ToUpper() == "K")
                            {
                                Console.Clear();
                                Console.Write("Unesite username korisnika koji vas je proglasio za kolaboratora: ");
                                string kolab = Console.ReadLine();
                                Console.Write("Unesite ime foldera: ");
                                string imeFoldera = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.KreirajFolder(_username, imeFoldera, kolab);
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }

                            else
                            {
                                Console.WriteLine("Pogresan unos!");
                            }

                        }
                        break;

                    //KREIRAJ TXT
                    case "3":
                        {
                            Console.Clear();

                            Console.Write("Da li zelite da modifikujete svoj fajl sistem ili kolaboratorski fajl sistem? (S|K)");
                            string odg = Console.ReadLine();

                            if (odg.ToUpper() == "S")
                            {
                                Console.Clear();
                                Console.Write("Unesite ime txt-a: ");
                                string imeTxt = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.KreirajTxt(_username, imeTxt, "NULL");
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            else if (odg.ToUpper() == "K")
                            {
                                Console.Write("Unesite username korisnika koji vas je proglasio za kolaboratora: ");
                                string kolab = Console.ReadLine();
                                Console.Write("Unesite ime txt-a: ");
                                string imeTxt = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.KreirajTxt(_username, imeTxt, kolab);
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }

                            else
                            {
                                Console.WriteLine("Pogresan unos!");
                            }

                        }
                        break;

                    //MODIFIKUJ FOLDER
                    case "4":
                        {
                            Console.Clear();

                            Console.Write("Da li zelite da modifikujete svoj fajl sistem ili kolaboratorski fajl sistem? (S|K)");
                            string odg = Console.ReadLine();

                            if (odg.ToUpper() == "S")
                            {
                                Console.Clear();
                                Console.Write("Unesite ime foldera koji zelite da modifikujete: ");
                                string imeFoldera = Console.ReadLine();
                                Console.Write("Unesite novo ime za folder: ");
                                string novoImeFoldera = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.ModifikujFolder(_username, imeFoldera, novoImeFoldera, "NULL");
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            else if (odg.ToUpper() == "K")
                            {
                                Console.Clear();
                                Console.Write("Unesite username korisnika koji vas je proglasio za kolaboratora: ");
                                string kolab = Console.ReadLine();
                                Console.Write("Unesite ime foldera koji zelite da modifikujete: ");
                                string imeFoldera = Console.ReadLine();
                                Console.Write("Unesite novo ime za folder: ");
                                string novoImeFoldera = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.ModifikujFolder(_username, imeFoldera, novoImeFoldera, kolab);
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Pogresan unos!");
                            }


                        }
                        break;

                    //MODIFIKUJ TXT
                    case "5":
                        {
                            Console.Clear();
                            Console.Write("Da li zelite da modifikujete svoj fajl sistem ili kolaboratorski fajl sistem? (S|K)");
                            string odg = Console.ReadLine();

                            if (odg.ToUpper() == "S")
                            {
                                Console.Clear();

                                Console.Write("Unesite ime txt-a koji zelite da modifikujete: ");
                                string imeTxt = Console.ReadLine();
                                Console.Write("Unesite sadrzaj koji zelite da upisete: ");
                                string noviSadrzaj = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.ModifikujTxt(_username, imeTxt, noviSadrzaj, "NULL");
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            else if (odg.ToUpper() == "K")
                            {
                                Console.Clear();
                                Console.Write("Unesite username korisnika koji vas je proglasio za kolaboratora: ");
                                string kolab = Console.ReadLine();
                                Console.Write("Unesite ime txt-a koji zelite da modifikujete: ");
                                string imeTxt = Console.ReadLine();
                                Console.Write("Unesite sadrzaj koji zelite da upisete: ");
                                string noviSadrzaj = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.ModifikujTxt(_username, imeTxt, noviSadrzaj, kolab);
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Pogresan unos!");
                            }


                        }
                        break;

                    //OBRISI FOLDER
                    case "6":
                        {
                            Console.Clear();
                            Console.Write("Da li zelite da modifikujete svoj fajl sistem ili kolaboratorski fajl sistem? (S|K)");
                            string odg = Console.ReadLine();

                            if (odg.ToUpper() == "S")
                            {
                                Console.Clear();

                                Console.Write("Unesite ime foldera koji zelite da obrisete: ");
                                string imeFolderaBrisanje = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.ObrisiFolder(_username, imeFolderaBrisanje, "NULL");
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            else if (odg.ToUpper() == "K")
                            {
                                Console.Clear();
                                Console.Write("Unesite username korisnika koji vas je proglasio za kolaboratora: ");
                                string kolab = Console.ReadLine();

                                Console.Write("Unesite ime foldera koji zelite da obrisete: ");
                                string imeFolderaBrisanje = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.ObrisiFolder(_username, imeFolderaBrisanje, kolab);
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Pogresan unos!");
                            }
                        }
                        break;

                    //OBRISI TXT
                    case "7":
                        {
                            Console.Clear();

                            Console.Write("Da li zelite da modifikujete svoj fajl sistem ili kolaboratorski fajl sistem? (S|K)");
                            string odg = Console.ReadLine();

                            if (odg.ToUpper() == "S")
                            {
                                Console.Clear();

                                Console.Write("Unesite ime txt-a koji zelite da obrisete: ");
                                string imeTxtBrisanje = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.ObrisiTxt(_username, imeTxtBrisanje, "NULL");
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            else if (odg.ToUpper() == "K")
                            {
                                Console.Clear();
                                Console.Write("Unesite username korisnika koji vas je proglasio za kolaboratora: ");
                                string kolab = Console.ReadLine();

                                Console.Write("Unesite ime txt-a koji zelite da obrisete: ");
                                string imeTxtBrisanje = Console.ReadLine();

                                try
                                {
                                    _konekcijaKorisnik.ObrisiTxt(_username, imeTxtBrisanje, kolab);
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }

                            else
                            {
                                Console.WriteLine("Pogresan unos!");
                            }

                        }
                        break;

                    //ZATRAZI SPISAK KORISNIKA
                    case "8":
                        {
                            Console.Clear();

                            try
                            {
                                List<string> listaSvihKorisnika = _konekcijaKorisnik.ZatraziSpisakKorisnika(_username);

                                foreach (string item in listaSvihKorisnika)
                                {

                                    Console.WriteLine("Korisnik: " + item);

                                }

                                Console.WriteLine("----------------------------------------------");
                                Console.WriteLine("Pritisnite bilo koje dugme za povratak na meni");
                                Console.ReadLine();
                            }
                            catch (FaultException<MyException> ex)
                            {
                                Console.WriteLine("Error : " + ex.Detail.Greska);
                                Thread.Sleep(3000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error : " + e.Message);
                                Thread.Sleep(3000);
                            }
                        }
                        break;

                    //DODAJ KOLABORATORA
                    case "9":
                        {
                            Console.Clear();
                            Console.Write("Unesite username korisnika koga zelite da dodate za kolaboratora: ");
                            string novi = Console.ReadLine();

                            try
                            {
                                _konekcijaKorisnik.DodajKolaboratora(_username, novi);
                            }
                            catch (FaultException<MyException> ex)
                            {
                                Console.WriteLine("Error : " + ex.Detail.Greska);
                                Thread.Sleep(3000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error : " + e.Message);
                                Thread.Sleep(3000);
                            }
                        }
                        break;

                    //PREGLED KOLABORATORA
                    case "10":
                        {
                            Console.Clear();

                            try
                            {
                                List<string> sviKolaboratori = _konekcijaKorisnik.PregledKolaboratora(_username);

                                Console.WriteLine("Moji kolaboratori:");

                                foreach(string item in sviKolaboratori)
                                {
                                    if(item != "PREKID")
                                    {
                                        Console.WriteLine("Korisnik: " + item);
                                        continue;
                                    }

                                    Console.WriteLine("------------------");
                                    Console.WriteLine("Strani kolaboratori:");
                                }

                                Console.WriteLine("----------------------------------------------");
                                Console.WriteLine("Pritisnite bilo koje dugme za povratak na meni");
                                Console.ReadLine();

                            }
                            catch (FaultException<MyException> ex)
                            {
                                Console.WriteLine("Error : " + ex.Detail.Greska);
                                Thread.Sleep(3000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error : " + e.Message);
                                Thread.Sleep(3000);
                            }
                        }
                        break;

                    //KRAJ PROGRAMA
                    case "0":
                        {
                            _konekcijaKorisnik.LogOut(_username);
                            kraj = false;
                        }
                        break;


                    default:
                        Console.WriteLine("Pogresan unos!");
                        Thread.Sleep(3000);
                        break;
                }

                Console.Clear();
            }
        }

        static void MeniKorisnik()
        {
            Console.WriteLine("Vi ste: " + _username);
            Console.WriteLine("-----------------------------");
            Console.WriteLine("ODABERITE OPCIJU:");
            Console.WriteLine("------------------------------");
            Console.WriteLine("1 - Trenutni sadrzaj fajl sistema");
            Console.WriteLine("2 - Kreiraj folder");
            Console.WriteLine("3 - Kreiraj txt");
            Console.WriteLine("4 - Modifikuj folder");
            Console.WriteLine("5 - Modifikuj txt");
            Console.WriteLine("6 - Obrisi folder");
            Console.WriteLine("7 - Obrisi txt");
            Console.WriteLine("8 - Zatrazi spisak korisnika");
            Console.WriteLine("9 - Dodaj kolaboratora");
            Console.WriteLine("10 - Pregled kolaboratora");
            Console.WriteLine("0 - Kraj");
            Console.WriteLine("------------------------------");
            Console.Write(">");
        }
        #endregion

        #region Nadzor_Petlja
        static void NadzorPetlja()
        {

            IServiceNadzor _konekcijaNadzor;

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            ChannelFactory<IServiceNadzor> factory = new ChannelFactory<IServiceNadzor>(
                                                            binding,
                                                            new EndpointAddress("net.tcp://localhost:6016/IServiceNadzor"));

            factory.Credentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

            _konekcijaNadzor = factory.CreateChannel();


            bool kraj = true;

            //PROVERA FAJL SISTEMA
            #region Thread
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                Dictionary<string, List<string>> ceoFajlSistem = new Dictionary<string, List<string>>();

                List<string> sivaLista = new List<string>(); //KORISNICI UPOZORENJE

                List<string> crnaLista = new List<string>(); //KORISNICI BAN

                List<string> poruke = new List<string>();

                Dictionary<string, int> Parametri = _konekcijaNadzor.PreuzmiParametre(_username);
                
            while (kraj)
                {

                    try
                    {
                        ceoFajlSistem = _konekcijaNadzor.ZatraziListuSvihPutanjaFajlSistema(_username);

                        foreach (KeyValuePair<string, List<string>> item in ceoFajlSistem)
                        {
                            if (ceoFajlSistem[item.Key].Count > Parametri["MaxBrojFajlova"] && !sivaLista.Contains(item.Key) && !crnaLista.Contains(item.Key))
                            {
                                _konekcijaNadzor.SaljiPorukuKorisniku(_username, item.Key, "UPOZORENJE");
                                Console.WriteLine("----------------------------------------------------------------------------");
                                Console.WriteLine("Korisnik " + item.Key + " je prekoracio ogranicenje, stavljen je u sivu listu!");
                                Console.WriteLine("----------------------------------------------------------------------------");
                                sivaLista.Add(item.Key);

                                new Thread(() =>
                                {
                                    Thread.CurrentThread.IsBackground = true;

                                    int vreme = Parametri["Vreme"];

                                    while (vreme > 0)
                                    {
                                        Thread.Sleep(1000);
                                        vreme--;

                                        Dictionary<string, List<string>> trenutniFajlSistem = _konekcijaNadzor.ZatraziListuSvihPutanjaFajlSistema(_username);


                                        if (trenutniFajlSistem[item.Key].Count > Parametri["MaxBrojFajlova"])
                                        {

                                            if (vreme == 0)
                                            {
                                                _konekcijaNadzor.SaljiPorukuKorisniku(_username, item.Key, "BAN");
                                                
                                            }
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }


                                }).Start();
                            }
                        }

                        ceoFajlSistem = _konekcijaNadzor.ZatraziListuSvihPutanjaFajlSistema(_username);

                        foreach (string item in sivaLista)
                        {
                            if (ceoFajlSistem[item].Count <= Parametri["MaxBrojFajlova"])
                            {
                                _konekcijaNadzor.SaljiPorukuKorisniku(_username, item, "OPROSTENO");
                                Console.WriteLine("----------------------------------------------------------------------------");
                                Console.WriteLine("Korisnik " + item + " je skinut sa sive liste!");
                                Console.WriteLine("----------------------------------------------------------------------------");
                                sivaLista.Remove(item);
                                break;
                            }


                            poruke = _konekcijaNadzor.ProveriListuPoruka(_username, item);

                            if (poruke.Contains("BAN") && sivaLista.Contains(item))
                            {
                                sivaLista.Remove(item);
                                crnaLista.Add(item);
                                _konekcijaNadzor.SaljiPorukuAdminu(_username, "Aleksa", crnaLista);
                                Console.WriteLine("----------------------------------------------------------------------------");
                                Console.WriteLine("Korisnik " + item + " je dodat na crnu listu!");
                                Console.WriteLine("----------------------------------------------------------------------------");
                                break;
                            }

                        }
                    }
                    catch (FaultException<MyException> ex)
                    {
                        Console.WriteLine("Error : " + ex.Detail.Greska);
                        Thread.Sleep(3000);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error : " + e.Message);
                        Thread.Sleep(3000);
                    }

                }
                
            }).Start(); 
            #endregion

            //GLAVNA PETLJA//
            while (kraj)
            {
                MeniNadzor();

                string ulaz = Console.ReadLine();

                switch (ulaz)
                {
                    //ZATRAZI CELOKUPAN FAJL SISTEM
                    case "1":
                        {
                            Console.Clear();

                            try
                            {
                                Dictionary<string, List<string>> celokupanFajlSistem = _konekcijaNadzor.ZatraziListuSvihPutanjaFajlSistema(_username);


                                Console.WriteLine("FAJL SISTEM");
                                Console.WriteLine("==============================");

                                foreach (KeyValuePair<string, List<string>> item in celokupanFajlSistem)
                                {

                                    Console.WriteLine("Korisnik: " + item.Key);

                                    foreach(string itemList in item.Value)
                                    {
                                        if (!itemList.Contains(".txt"))
                                        {
                                            Console.WriteLine("Folder: " + itemList);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Txt:    " + itemList);
                                        }
                                    }

                                    Console.WriteLine("-----------------------------");

                                }

                                Console.WriteLine("----------------------------------------------");
                                Console.WriteLine("Pritisnite bilo koje dugme za povratak na meni");
                                Console.ReadLine();
                            }
                            catch (FaultException<MyException> ex)
                            {
                                Console.WriteLine("Error : " + ex.Detail.Greska);
                                Thread.Sleep(3000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error : " + e.Message);
                                Thread.Sleep(3000);
                            }
                        }
                        break;

                    //KRAJ PROGRAMA
                    case "0":
                        {
                            _konekcijaNadzor.LogOut(_username);
                            kraj = false;
                        }
                        break;

                    default:
                        Console.WriteLine("Pogresan unos!");
                        Thread.Sleep(3000);
                        break;
                }

                Console.Clear();
            }
        }

        static void MeniNadzor()
        {
            Console.WriteLine("Vi ste: " + _username);
            Console.WriteLine("-----------------------------");
            Console.WriteLine("ODABERITE OPCIJU:");
            Console.WriteLine("------------------------------");
            Console.WriteLine("1 - Zatrazi celokupan fajl sistem");
            Console.WriteLine("0 - Kraj");
            Console.WriteLine("------------------------------");
            Console.Write(">");
        }
        #endregion

        #region Admin_Petlja
        static void AdminPetlja()
        {
            IServiceAdmin _konekcijaAdmin;

            NetTcpBinding binding = new NetTcpBinding();
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            ChannelFactory<IServiceAdmin> factory = new ChannelFactory<IServiceAdmin>(
                                                            binding,
                                                            new EndpointAddress("net.tcp://localhost:6017/IServiceAdmin"));

            factory.Credentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;

            _konekcijaAdmin = factory.CreateChannel();


            bool kraj = true;
            List<string> banovaniKorisnici = new List<string>();
            List<string> crnaLista = new List<string>();

            //PROVERA PORUKE OD NADZORA
            #region Thread
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                List<string> crnaListaIspis = new List<string>();

                while (kraj)
                {
                    try
                    {
                        crnaLista = _konekcijaAdmin.ProveriListuPoruka(_username);

                        if (crnaLista.Count != 0)
                        {
                            foreach (string item in crnaLista)
                            {
                                if (!crnaListaIspis.Contains(item))
                                {
                                    Console.WriteLine("-------------------------------------------------------------------------------");
                                    Console.WriteLine("Nadzor je zatrazio banovanje korisnika " + item);
                                    Console.WriteLine("-------------------------------------------------------------------------------");
                                    crnaListaIspis.Add(item);
                                }

                            }


                        }
                    }
                    catch (FaultException<MyException> ex)
                    {
                        Console.WriteLine("Error : " + ex.Detail.Greska);
                        Thread.Sleep(3000);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error : " + e.Message);
                        Thread.Sleep(3000);
                    }

                }

            }).Start(); 
            #endregion

            //GLAVNA PETLJA//
            while (kraj)
            {
                MeniAdmin();

                string ulaz = Console.ReadLine();

                switch (ulaz)
                {

                    //ZATRAZI CELOKUPAN FAJL SISTEM
                    case "1":
                        {
                            Console.Clear();

                            try
                            {
                                Dictionary<string, List<string>> celokupanFajlSistem = _konekcijaAdmin.ZatraziListuSvihPutanjaFajlSistema(_username);


                                Console.WriteLine("FAJL SISTEM");
                                Console.WriteLine("==============================");

                                foreach (KeyValuePair<string, List<string>> item in celokupanFajlSistem)
                                {

                                    Console.WriteLine("Korisnik: " + item.Key);

                                    foreach (string itemList in item.Value)
                                    {
                                        if (!itemList.Contains(".txt"))
                                        {
                                            Console.WriteLine("Folder: " + itemList);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Txt:    " + itemList);
                                        }
                                    }

                                    Console.WriteLine("-----------------------------");

                                }

                                Console.WriteLine("----------------------------------------------");
                                Console.WriteLine("Pritisnite bilo koje dugme za povratak na meni");
                                Console.ReadLine();
                            }
                            catch (FaultException<MyException> ex)
                            {
                                Console.WriteLine("Error : " + ex.Detail.Greska);
                                Thread.Sleep(3000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error : " + e.Message);
                                Thread.Sleep(3000);
                            }

                        }
                        break;

                    //MODIFIKUJ FOLDER
                    case "2":
                        {
                            Console.Clear();
                            Console.Write("Unesite username korisnika ciji folder zelite da modifikujete: ");
                            string usernameKorisnik = Console.ReadLine();
                            Console.Write("Unesite ime foldera koji zelite da modifikujete: ");
                            string imeFoldera = Console.ReadLine();
                            Console.Write("Unesite novo ime za folder: ");
                            string novoImeFoldera = Console.ReadLine();

                            try
                            {
                                _konekcijaAdmin.ModifikujFolder(_username, usernameKorisnik, imeFoldera, novoImeFoldera);
                            }
                            catch (FaultException<MyException> ex)
                            {
                                Console.WriteLine("Error : " + ex.Detail.Greska);
                                Thread.Sleep(3000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error : " + e.Message);
                                Thread.Sleep(3000);
                            }
                        }
                        break;

                    //MODIFIKUJ TXT
                    case "3":
                        {
                            Console.Clear();
                            Console.Write("Unesite username korisnika ciji txt zelite da modifikujete: ");
                            string usernameKorisnik = Console.ReadLine();
                            Console.Write("Unesite ime txt-a koji zelite da modifikujete: ");
                            string imeTxt = Console.ReadLine();
                            Console.Write("Unesite sadrzaj koji zelite da upisete: ");
                            string noviSadrzaj = Console.ReadLine();

                            try
                            {
                                _konekcijaAdmin.ModifikujTxt(_username, usernameKorisnik, imeTxt, noviSadrzaj);
                            }
                            catch (FaultException<MyException> ex)
                            {
                                Console.WriteLine("Error : " + ex.Detail.Greska);
                                Thread.Sleep(3000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error : " + e.Message);
                                Thread.Sleep(3000);
                            }

                        }
                        break;

                    //OBRISI FOLDER
                    case "4":
                        {
                            Console.Clear();
                            Console.Write("Unesite username korisnika ciji folder zelite da obrisete: ");
                            string usernameKorisnik = Console.ReadLine();
                            Console.Write("Unesite ime foldera koji zelite da obrisete: ");
                            string imeFolderaBrisanje = Console.ReadLine();

                            try
                            {
                                _konekcijaAdmin.ObrisiFolder(_username, usernameKorisnik, imeFolderaBrisanje);
                            }
                            catch (FaultException<MyException> ex)
                            {
                                Console.WriteLine("Error : " + ex.Detail.Greska);
                                Thread.Sleep(3000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error : " + e.Message);
                                Thread.Sleep(3000);
                            }
                        }
                        break;

                    //OBRISI TXT
                    case "5":
                        {
                            Console.Clear();
                            Console.Write("Unesite username korisnika ciji txt zelite da obrisete: ");
                            string usernameKorisnik = Console.ReadLine();
                            Console.Write("Unesite ime txt-a koji zelite da obrisete: ");
                            string imeTxtBrisanje = Console.ReadLine();

                            try
                            {
                                _konekcijaAdmin.ObrisiTxt(_username, usernameKorisnik, imeTxtBrisanje);
                            }
                            catch (FaultException<MyException> ex)
                            {
                                Console.WriteLine("Error : " + ex.Detail.Greska);
                                Thread.Sleep(3000);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error : " + e.Message);
                                Thread.Sleep(3000);
                            }
                        }
                        break;

                    //BANUJ KORISNIKA
                    case "6":
                        {
                            Console.Clear();

                            if(crnaLista.Count > 0)
                            {
                                Console.WriteLine("-------------------------");
                                Console.WriteLine("Korisnici za banovanje: ");
                                foreach (string item in crnaLista)
                                {
                                    Console.WriteLine("Korisnik: " + item);
                                }
                                Console.WriteLine("-------------------------");
                                Console.WriteLine("Banovani korisnici: ");

                                foreach (string item in banovaniKorisnici)
                                {
                                    Console.WriteLine("Korisnik: " + item);
                                }
                                Console.WriteLine("-------------------------");

                                Console.Write("Unesite username korisnika koga zelite da banujete: ");
                                string usernameKorisnik = Console.ReadLine();

                                try
                                {
                                    if(!banovaniKorisnici.Contains(usernameKorisnik))
                                    {
                                        _konekcijaAdmin.ObrisiFajlSistem(_username, usernameKorisnik);
                                        banovaniKorisnici.Add(usernameKorisnik);
                                        Console.WriteLine("Korisnik " + usernameKorisnik + " je banovan!");
                                        Console.ReadLine();
                                    }
                                    else
                                    {
                                        Console.WriteLine("Korisnik je vec banovan!");
                                        Console.ReadLine();
                                    }
                                    
                                }
                                catch (FaultException<MyException> ex)
                                {
                                    Console.WriteLine("Error : " + ex.Detail.Greska);
                                    Thread.Sleep(3000);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error : " + e.Message);
                                    Thread.Sleep(3000);
                                }
                            }
                            
                        }
                        break;

                    //KRAJ PROGRAMA
                    case "0":
                        {
                            _konekcijaAdmin.LogOut(_username);
                            kraj = false;
                        }
                        break;

                    default:
                        Console.WriteLine("Pogresan unos!");
                        Thread.Sleep(3000);
                        break;
                }

                Console.Clear();

            }
        }

        static void MeniAdmin()
        {
            Console.WriteLine("Vi ste: " + _username);
            Console.WriteLine("-----------------------------");
            Console.WriteLine("ODABERITE OPCIJU:");
            Console.WriteLine("------------------------------");
            Console.WriteLine("1 - Zatrazi celokupan fajl sistem");
            Console.WriteLine("2 - Modifikuj folder");
            Console.WriteLine("3 - Modifikuj txt");
            Console.WriteLine("4 - Obrisi folder");
            Console.WriteLine("5 - Obrisi txt");
            Console.WriteLine("6 - Banuj korisnika i obrisi mu fajl sistem");
            Console.WriteLine("0 - Kraj");
            Console.WriteLine("------------------------------");
            Console.Write(">");
        }
        #endregion
    }
}
