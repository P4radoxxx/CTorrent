using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_torrent
{
    public static class Datas
    {
        private static List<string> _Paths = new List<string>();
        public static List<string> Paths
        {
            get { return _Paths; }
            set { if (Paths != value) { _Paths = value; } }
        }
    }
}
