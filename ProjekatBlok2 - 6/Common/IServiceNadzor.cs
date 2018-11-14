using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace Common
{
    [ServiceContract]
    public interface IServiceNadzor
    {
        [OperationContract]
        [FaultContract(typeof(MyException))]
        Dictionary<string, List<string>> ZatraziListuSvihPutanjaFajlSistema(string username);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void SaljiPorukuKorisniku(string username, string usernameKorisnik, string poruka);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void SaljiPorukuAdminu(string username, string usernameAdmin, List<string> crnaLista);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        List<string> ProveriListuPoruka(string username, string usernameKorisnik);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        void LogOut(string username);

        [OperationContract]
        [FaultContract(typeof(MyException))]
        Dictionary<string, int> PreuzmiParametre(string username);
    }
}
