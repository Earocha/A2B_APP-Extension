using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2B_APP_Extension.Model
{
    public class MysqlCredentials
    {
        public string MeetingsConnectionString { get; } = "Server=soxroxapp.com; Database=sox; User=a2bapp; Password=hpEe4U2sknh4dWCwEX3mEPXYfeN2zQKJZesEw@; Port=3306;  Connect Timeout=180; Persist Security Info=False";

        public string MeetingsConnectionString2 { get; } = "soxroxapp.com;Initial Catalog=sox;User ID=a2bapp;Password=hpEe4U2sknh4dWCwEX3mEPXYfeN2zQKJZesEw@";
    }
    public class SkypeAddressId
    {
        public string TestingCG { get; } = "19:06c1eabd06c145349d51342e92e0db8b@thread.skype";
        public string MarkCG { get; } = "19:719f92c717d2443092465f6d98ad16bc@thread.skype";

        public string KRUAT { get; } = "19:fcd1745719e14d8da3809f14ef95ce63@thread.skype";
    }
}
