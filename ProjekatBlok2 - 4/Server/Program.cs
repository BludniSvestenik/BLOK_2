using Common;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server
{
    class Program : Baza
    {
        static void Main(string[] args)
        {

            Admin admin = new Admin("Aleksa", "1111");
            Nadzor nadzor = new Nadzor("Nikola", "2222");
            Korisnik korisnik1 = new Korisnik("Itan", "3333");
            Korisnik korisnik2 = new Korisnik("Igor", "4444");
            Korisnik korisnik3 = new Korisnik("Komsa", "5555");

            EntitetiBaza.Add(admin.Username, admin);
            EntitetiBaza.Add(nadzor.Username, nadzor);
            EntitetiBaza.Add(korisnik1.Username, korisnik1);
            EntitetiBaza.Add(korisnik2.Username, korisnik2);
            EntitetiBaza.Add(korisnik3.Username, korisnik3);

            BazaFajlSistema.Add(korisnik1.Username, new List<string>());
            BazaFajlSistema.Add(korisnik2.Username, new List<string>());
            BazaFajlSistema.Add(korisnik3.Username, new List<string>());

            BazaPoruka.Add(korisnik1.Username, new List<string>());
            BazaPoruka.Add(korisnik2.Username, new List<string>());
            BazaPoruka.Add(korisnik3.Username, new List<string>());
            BazaPoruka.Add(admin.Username, new List<string>());


            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            policies.Add(new MyAuthorizationPolicy());

            #region Korisnik
            NetTcpBinding bindingKorisnik = new NetTcpBinding();
            bindingKorisnik.Security.Mode = SecurityMode.Transport;
            bindingKorisnik.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            bindingKorisnik.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            string adresaKorisnik = "net.tcp://localhost:6015/IServiceKorisnik";
            ServiceHost hostKorisnik = new ServiceHost(typeof(ServiceKorisnik));
            hostKorisnik.AddServiceEndpoint(typeof(IServiceKorisnik),
                                    bindingKorisnik,
                                    new Uri(adresaKorisnik));

            hostKorisnik.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            hostKorisnik.Authorization.PrincipalPermissionMode = System.ServiceModel.Description.PrincipalPermissionMode.Custom;


            hostKorisnik.Open();
            Console.WriteLine("Host je otvoren na adresi : {0} - ServiceKorisnik", adresaKorisnik);

            #endregion

            #region Nadzor
            NetTcpBinding bindingNadzor = new NetTcpBinding();
            bindingNadzor.Security.Mode = SecurityMode.Transport;
            bindingNadzor.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            bindingNadzor.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            string adresaNadzor = "net.tcp://localhost:6016/IServiceNadzor";
            ServiceHost hostNadzor = new ServiceHost(typeof(ServiceNadzor));
            hostNadzor.AddServiceEndpoint(typeof(IServiceNadzor),
                                    bindingNadzor,
                                    new Uri(adresaNadzor));

            hostNadzor.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            hostNadzor.Authorization.PrincipalPermissionMode = System.ServiceModel.Description.PrincipalPermissionMode.Custom;


            hostNadzor.Open();
            Console.WriteLine("Host je otvoren na adresi : {0} - ServiceNadzor", adresaNadzor);
            #endregion

            #region Admin
            NetTcpBinding bindingAdmin = new NetTcpBinding();
            bindingAdmin.Security.Mode = SecurityMode.Transport;
            bindingAdmin.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            bindingAdmin.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;

            string adresaAdmin = "net.tcp://localhost:6017/IServiceAdmin";
            ServiceHost hostAdmin = new ServiceHost(typeof(ServiceAdmin));
            hostAdmin.AddServiceEndpoint(typeof(IServiceAdmin),
                                    bindingAdmin,
                                    new Uri(adresaAdmin));

            hostAdmin.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            hostAdmin.Authorization.PrincipalPermissionMode = System.ServiceModel.Description.PrincipalPermissionMode.Custom;

            hostAdmin.Open();
            Console.WriteLine("Host je otvoren na adresi : {0} - ServiceAdmin", adresaAdmin);           
            #endregion

            #region Konekcija
            string adresaKonekcija = "net.tcp://localhost:6018/IServiceKonekcija";
            ServiceHost hostKonekcija = new ServiceHost(typeof(ServiceKonekcija));
            hostKonekcija.AddServiceEndpoint(typeof(IServiceKonekcija),
                                    new NetTcpBinding(),
                                    new Uri(adresaKonekcija));
            hostKonekcija.Open();
            Console.WriteLine("Host je otvoren na adresi : {0} - ServiceKonekcija", adresaKonekcija);
            #endregion


 
            Console.ReadLine();
            hostKorisnik.Close(); 
            hostNadzor.Close();
            hostAdmin.Close();
            hostKonekcija.Close();
        }
    }
}
