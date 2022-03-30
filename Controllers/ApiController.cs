using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
//using RestSharp;

namespace A2B_APP_Extension.Controllers
{
    class Post
    {
        public async Task<Boolean> SkypeSendMessage(string Message, string AddressId)
        {
            try
            {
                var values = new Dictionary<string, string>
                {
                    { "address", AddressId },
                    { "message", Message },
                    { "secret_token", "$2b$10$/LfdTKa3kbpaTxmX.xh7jupJlJ44kv1JHo6juzUEdcDlJ2bXg.nyq" }
                };

                var data = new FormUrlEncodedContent(values);
                var url = "https://sjo5-api2.ngrok.io/skype-bot-messaging";
                using var client = new HttpClient();

                var response = await client.PostAsync(url, data);

                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(result);
                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return false;
            }


        }

    }

}
