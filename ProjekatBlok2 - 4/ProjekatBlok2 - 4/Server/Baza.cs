using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

    public class Baza
    {
        public static Dictionary<string, Entitet> EntitetiBaza = new Dictionary<string, Entitet>();

        public static Dictionary<string, List<string>> BazaFajlSistema = new Dictionary<string, List<string>>();

        public static Dictionary<string, List<string>> BazaPoruka = new Dictionary<string, List<string>>();
    }
}
