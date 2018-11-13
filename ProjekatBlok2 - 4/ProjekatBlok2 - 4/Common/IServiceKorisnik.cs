using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace Common
{
    [ServiceContract]
    public interface IServiceKorisnik
    {

        [OperationContract]
        [FaultContract(typeof(MyException))]
        List<string> TrenutniSadrzajFajlSistema(string username, string kolab);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void KreirajTxt(string username, string filename, string kolab);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void KreirajFolder(string username, string foldername, string kolab);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void ModifikujTxt(string username, string filename, string noviSadrzaj, string kolab);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void ModifikujFolder(string username, string foldername, string newFoldername, string kolab);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void ObrisiTxt(string username, string filename, string kolab);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void ObrisiFolder(string username, string foldername, string kolab);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        List<string> ZatraziSpisakKorisnika(string username);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void DodajKolaboratora(string username, string novi);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        List<string> PregledKolaboratora(string username);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        List<string> ProveriListuPoruka(string username);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void LogOut(string username);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        Dictionary<string, int> PreuzmiParametre(string username);

    }
}
