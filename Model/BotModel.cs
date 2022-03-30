using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A2B_APP_Extension.Model
{

    public class Skype
    {
        public string Address { get; set; }
        public string Message { get; set; }
        public Mention Mention { get; set; }
    }

    public class Mention
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }


}
