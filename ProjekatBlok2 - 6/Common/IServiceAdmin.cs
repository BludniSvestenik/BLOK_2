using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace Common
{
    [ServiceContract]
    public interface IServiceAdmin
    {
        [OperationContract]
        [FaultContract(typeof(MyException))]
        Dictionary<string, List<string>> ZatraziListuSvihPutanjaFajlSistema(string username);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void ModifikujTxt(string username, string usernameKorisnik, string filename, string noviSadrzaj);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void ModifikujFolder(string username, string usernameKorisnik, string foldername, string newFoldername);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void ObrisiTxt(string username, string usernameKorisnik, string filename);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void ObrisiFolder(string username, string usernameKorisnik, string foldername);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void ObrisiFajlSistem(string username, string usernameKorisnik);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        List<string> ProveriListuPoruka(string username);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void LogOut(string username);
    }
}
