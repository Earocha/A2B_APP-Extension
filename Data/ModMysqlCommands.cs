using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using A2B_APP_Extension.Model;
using System.Collections;

namespace A2B_APP_Extension.Data
{
    public class ModMysqlCommands
    {
        private string CommandString;

        public ModMysqlCommands(string Command)
        {
            CommandString = Command;
        }

        public async Task<dynamic> DBGetAllItems(string RequestType)
        {
            switch (RequestType)
            {
                case "SoxRecordings":
                    try
                    {
                        MysqlCredentials GetPrivateMysqlData = new MysqlCredentials();
                        MySqlConnection conn = new MySqlConnection(GetPrivateMysqlData.MeetingsConnectionString);
                        SharefileRecordings SFRecordings = new SharefileRecordings();


                        conn.Open();
                        MySqlCommand cmd = new MySqlCommand(CommandString, conn);
                        MySqlDataReader rdr = cmd.ExecuteReader();
                        //List<SharefileRecordings> SFRecordingsList = new List<SharefileRecordings>();

                        int count = 0;
                        IDictionary<int, string> SFRecordingsList = new Dictionary<int, string>();
                        while (rdr.Read())
                        {
                            SFRecordingsList.Add(count, rdr["SfItemId"].ToString());
                            count++;
                        }

                        rdr.Close();
                        conn.Close();
                        return SFRecordingsList;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        return ex.ToString();
                    }


                default:
                    return "No Type found";
            }
        }
        public async Task<bool> StartCommand(string InsertCommand)
        {
            try
            {
                MysqlCredentials GetPrivateMysqlData = new MysqlCredentials();
                MySqlConnection conn = new MySqlConnection(GetPrivateMysqlData.MeetingsConnectionString);

                conn.Open();

                MySqlCommand cmd = new MySqlCommand(InsertCommand, conn);
                cmd.ExecuteNonQuery();

                conn.Close();

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }
    }
}
