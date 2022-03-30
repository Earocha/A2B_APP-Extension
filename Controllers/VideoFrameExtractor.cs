using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
using A2B_APP_Extension.Schedulers;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using ShareFile.Api.Client.Models;
using A2B_APP_Extension.Data;
using A2B_APP_Extension.Model;
using ShareFile.Api.Client;
using A2B_APP_Extension.Controllers;
using A2B_APP_Extension;
using A2BHQ_Desktop_VideoRendering.Controllers.Sharefile;

namespace A2B_APP_Extension.Controllers.Sharefile
{
    public class VideoFrameExtractor
    {

        public string StatusLabel { get; set; }

        public async Task SetJobAsync()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();
            // and start it off
            await scheduler.Start();
            // define the job and tie it to our HelloJob class
            IJobDetail job = JobBuilder.Create<JobVFE>()
                .WithIdentity("job1", "group1")
                .Build();
            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(120)
                    .RepeatForever())
                .Build();
            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task Start()
        {
            try
            {
                string folderId = "fo5dc20f-547e-4b5b-9684-426f2571a338";
                SFRequests SharefileService = new SFRequests();
                var SharefileItems = await SharefileService.GetItemChildren(folderId);

                dynamic SFRecordingsList;
                string sql = "SELECT * FROM Recordings";
                ModMysqlCommands ExecuteMysql = new ModMysqlCommands(sql);
                SFRecordingsList = await ExecuteMysql.DBGetAllItems("SoxRecordings");

                Console.WriteLine(SFRecordingsList);

                foreach (var item in SharefileItems.Feed)
                {
                    var PullVideo = SFRecordingsList.ContainsValue(item.Id.ToString());

                    if (PullVideo)
                    {
                        /* SharefileRecordings SaveVideo = new SharefileRecordings();
                         SaveVideo.Id = item.Id;
                         SaveVideo.Videoname = item.FileName;
                         SaveVideo.Sflink = "https://a2q2.sharefile.com/home/shared/fo9516f7-0c8d-4333-be45-7a2c3077cb17";
                         SaveVideo.SfItemId = item.Id;
                         SaveVideo.Date = DateTime.Now.ToString();
                         SaveVideo.Execute_screenshots = "yes";

                         string UpdateQuery = "UPDATE Recordings set Videoname='" + SaveVideo.Videoname + "',Sflink='" + SaveVideo.Sflink + "',SfItemId='" + SaveVideo.SfItemId + "', Date='" + SaveVideo.Date + "',Execute_screenshots='" + SaveVideo.Execute_screenshots + "' where SfItemId='" + SaveVideo.SfItemId + "';";

                         var res = await ExecuteMysql.StartCommand(UpdateQuery);
                         Console.WriteLine("MYSQL UPDATE RESULT --->" + res);*/
                    }
                    else
                    { //Insert


                        var sfItem1 = await SharefileService.ProcessDownLoad(item.Id.ToString());
                        SharefileRecordings SaveVideo = new SharefileRecordings();
                        SaveVideo.Id = item.Id;
                        SaveVideo.Videoname = item.FileName;
                        SaveVideo.Sflink = "https://a2q2.sharefile.com/home/shared/fo9516f7-0c8d-4333-be45-7a2c3077cb17";
                        SaveVideo.SfItemId = item.Id;
                        SaveVideo.Date = DateTime.Now.ToString();
                        SaveVideo.Execute_screenshots = "yes";

                        string InsertSQLCommand = "INSERT INTO Recordings (Id, Videoname, Sflink, SfItemId,Date,Execute_screenshots) VALUES ('" + SaveVideo.Id + "','" + SaveVideo.Videoname + "','" + SaveVideo.Sflink + "', '" + SaveVideo.SfItemId + "', '" + SaveVideo.Date + "', '" + SaveVideo.Execute_screenshots + "')";

                        var res = await ExecuteMysql.StartCommand(InsertSQLCommand);
                        Console.WriteLine("MYSQL INSERT RESULT --->" + res);

                        Post SkypeRequest = new Post();
                        SkypeAddressId SkypeId = new SkypeAddressId();
                        string message = "New KeyReport Image File Extracted (TEST)\n Video Name: " + SaveVideo.Videoname + "\n Sharefile: " + SaveVideo.Sflink;
                        await SkypeRequest.SkypeSendMessage(message, SkypeId.KRUAT);
                    }
                }
            }
            catch (Exception err)
            {
                Post SkypeRequest = new Post();
                SkypeAddressId SkypeId = new SkypeAddressId();
                // SkypeRequest.SkypeSendMessage(err.ToString(), SkypeId.MarkCG);
            }

        }
    }
}
