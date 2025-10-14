using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoschControl
{
    public class CamData
    {
        public string Ip { get; private set; }
        public string Prog { get; private set; }
        public Object Stream { get; private set; }
        public CamData(string ip,string prog, Object stream)
        {
            this.Ip = ip;
            this.Prog = prog;
            this.Stream = stream;
        }
    }
}
