using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace Common
{
    [ServiceContract]
    public interface IServiceKonekcija
    {
        [OperationContract]
        [FaultContract(typeof(MyException))]
        string UspostaviKonekciju(string username, string password);
    }
}
