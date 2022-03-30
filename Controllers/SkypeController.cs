using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using A2B_APP_Extension.Model;

namespace A2B_APP_Extension.Controllers.Bot
{
    public class ODDSkype
    {

        private readonly IConfiguration _config;
        public ODDSkype(IConfiguration config)
        {
            _config = config;
        }

        public Task<bool> SendSkypeNotif(Skype skype, string botName)
        {
            try
            {
                string msAppId = string.Empty;
                string msAppPassword = string.Empty;

                switch (botName)
                {
                    case "OddBot":
                        msAppId = _config.GetSection("MSBot").GetSection("OddBotId").Value;
                        msAppPassword = _config.GetSection("MSBot").GetSection("OddBotSecret").Value;
                        break;
                    case "SXIAPlannerBot":
                        msAppId = _config.GetSection("MSBot").GetSection("SXIAPlannerBotId").Value;
                        msAppPassword = _config.GetSection("MSBot").GetSection("SXIAPlannerBotSecret").Value;
                        break;
                    case "PlannerBot":
                        msAppId = _config.GetSection("MSBot").GetSection("PlannerBotId").Value;
                        msAppPassword = _config.GetSection("MSBot").GetSection("PlannerBotSecret").Value;
                        break;
                }


                var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), msAppId, msAppPassword);
                var conversation = new ConversationAccount(true, id: skype.Address);

                string message = string.Empty;

                List<Entity> listEntity = null;
                if (skype.Mention != null)
                {
                    var entity = new Microsoft.Bot.Schema.Mention()
                    {
                        Type = "mention",
                        Mentioned = new ChannelAccount { Id = skype.Mention.Id, Name = skype.Mention.Name },
                        Text = $"<at>{skype.Mention.Name}</at>",
                    };

                    listEntity = new List<Entity>() { entity };
                    message = $"{skype.Message} <br /> <at>{skype.Mention.Name}</at>";
                }
                else
                {
                    message = $"{skype.Message}";
                }

                var activity = new Microsoft.Bot.Schema.Activity();

                activity.Text = message;
                if (listEntity != null)
                    activity.Entities = listEntity;
                // Note on ChannelId:
                // The Bot Framework channel is identified by a unique ID.
                // For example, "skype" is a common channel to represent the Skype service.
                // We are inventing a new channel here.
                activity.ServiceUrl = "https://smba.trafficmanager.net/apis/";
                activity.ChannelId = "skype";
                //From = new ChannelAccount(id: "user", name: "Levin"),
                //Recipient = new ChannelAccount(id: "bot", name: "Bot"),
                activity.Conversation = conversation;
                activity.Timestamp = DateTime.UtcNow;
                activity.Id = Guid.NewGuid().ToString();
                activity.Type = ActivityTypes.Message;
                activity.Locale = "en-En";
                activity.TextFormat = "markdown";


                connector.Conversations.SendToConversation(activity);

                ////var members = await connector.Conversations.GetActivityMembersAsync(conversation.Id, activity.Id);
                //var members = await connector.Conversations.GetConversationMembersAsync(conversation.Id);

                //// Concatenate information about all the members into a string
                //WriteLog write = new WriteLog();
                //var sb = new StringBuilder();
                //foreach (var member in members)
                //{
                //    write.Display(member);
                //    Debug.WriteLine("--------------------------------------------");
                //}

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString(), "ErrorSendSkypeNotif");

                //skype.Message = $"<b>SX App Error</b> <br /> <br />Method: SendSkypeNotif<br />{ex}";
                //skype.Address = _config.GetSection("Skype").GetSection("ErrorGC").Value.ToString(); //Live


                ////Microsoft.Bot.Schema.Mention mention = new Microsoft.Bot.Schema.Mention();
                ////mention.Mentioned.Id = "a2b.earocha";
                ////mention.Mentioned.Name = "Mark";
                ////skype.Mention = null;
                //SendSkypeNotif(skype);
                return Task.FromResult(false);
            }
        }

        //reserve functions
        #region
        /*
                public Task<bool> SendSkypeImageNotif(Skype skype, string newFilename, string botName)
                {
                    try
                    {

                        string msAppId = string.Empty;
                        string msAppPassword = string.Empty;

                        switch (botName)
                        {
                            case "OddBot":
                                msAppId = _config.GetSection("MSBot").GetSection("OddBotId").Value;
                                msAppPassword = _config.GetSection("MSBot").GetSection("OddBotSecret").Value;
                                break;
                            case "SXIAPlannerBot":
                                msAppId = _config.GetSection("MSBot").GetSection("SXIAPlannerBotId").Value;
                                msAppPassword = _config.GetSection("MSBot").GetSection("SXIAPlannerBotSecret").Value;
                                break;
                            case "PlannerBot":
                                msAppId = _config.GetSection("MSBot").GetSection("PlannerBotId").Value;
                                msAppPassword = _config.GetSection("MSBot").GetSection("PlannerBotSecret").Value;
                                break;
                        }


                        string startupPath = Directory.GetCurrentDirectory();
                        string path = Path.Combine(startupPath, "include", "temp", newFilename);

                        var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), msAppId, msAppPassword);
                        var conversation = new ConversationAccount(true, id: skype.Address);

                        string message = string.Empty;

                        //Attachment attachment = new Attachment();
                        //attachment = GetInlineAttachment(newFilename);

                        var activity = new Microsoft.Bot.Schema.Activity();

                        //activity.Text = message;
                        activity.ServiceUrl = "https://smba.trafficmanager.net/apis/";
                        activity.ChannelId = "skype";
                        activity.Conversation = conversation;
                        activity.Timestamp = DateTime.UtcNow;
                        activity.Id = Guid.NewGuid().ToString();
                        activity.Type = ActivityTypes.Message;
                        activity.Locale = "en-En";
                        activity.TextFormat = "markdown";
                        activity.Attachments = new List<Attachment>() { GetInlineAttachment(newFilename) }; ;

                        connector.Conversations.SendToConversation(activity);

                        return Task.FromResult(true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString(), "ErrorSendSkypeNotif");
                        return Task.FromResult(false);
                    }
                }

                private static Attachment GetInlineAttachment(string newFilename)
                {
                    string startupPath = Directory.GetCurrentDirectory();
                    string path = Path.Combine(startupPath, "include", "temp", newFilename);

                    var imageData = Convert.ToBase64String(File.ReadAllBytes(path));
                    return new Attachment
                    {
                        Name = $"{newFilename}",
                        ContentType = "image/png",
                        ContentUrl = $"data:image/png;base64,{imageData}"
                    };
                }
                private static Attachment ODDGetInlineAttachment(string newFilename, string cUrl, string ftype)
                {

                    return new Attachment
                    {
                        Name = $"{newFilename}",
                        ContentType = $"{ftype}",
                        ContentUrl = $"{cUrl}"
                    };
                }
                public Task<bool> ODDSendSkypeNotif(string skypeappid, string skypeapppwd, Skype skype)
                {
                    try
                    {
                        string msAppId = skypeappid.ToString();
                        string msAppPassword = skypeapppwd.ToString();

                        var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), msAppId, msAppPassword);
                        var conversation = new ConversationAccount(true, id: skype.Address);

                        string message = string.Empty;

                        List<Entity> listEntity = null;
                        if (skype.Mention != null)
                        {
                            var mt = "<at id=\"skype.Mention.Id\">skype.Mention.Name</at>";
                            mt = mt.Replace("skype.Mention.Id", skype.Mention.Id);
                            mt = mt.Replace("skype.Mention.Name", skype.Mention.Name);

                            var entity = new Microsoft.Bot.Schema.Mention()
                            {
                                Type = "mention",
                                Mentioned = new ChannelAccount { Id = skype.Mention.Id, Name = skype.Mention.Name },
                                Text = mt,
                            };


                            listEntity = new List<Entity>() { entity };
                            message = $"{skype.Message} <br />" + mt;
                        }
                        else
                        {
                            message = $"{skype.Message}";
                        }

                        var activity = new Microsoft.Bot.Schema.Activity();

                        activity.Text = message;
                        if (listEntity != null)
                            activity.Entities = listEntity;
                        // Note on ChannelId:
                        // The Bot Framework channel is identified by a unique ID.
                        // For example, "skype" is a common channel to represent the Skype service.
                        // We are inventing a new channel here.
                        activity.ServiceUrl = "https://smba.trafficmanager.net/apis/";
                        activity.ChannelId = "skype";
                        //From = new ChannelAccount(id: "user", name: "Levin"),
                        //Recipient = new ChannelAccount(id: "bot", name: "Bot"),
                        activity.Conversation = conversation;
                        activity.Timestamp = DateTime.UtcNow;
                        activity.Id = Guid.NewGuid().ToString();
                        activity.Type = ActivityTypes.Message;
                        activity.Locale = "en-En";
                        activity.TextFormat = "markdown";


                        connector.Conversations.SendToConversation(activity);

                        ////var members = await connector.Conversations.GetActivityMembersAsync(conversation.Id, activity.Id);
                        //var members = await connector.Conversations.GetConversationMembersAsync(conversation.Id);

                        //// Concatenate information about all the members into a string
                        //WriteLog write = new WriteLog();
                        //var sb = new StringBuilder();
                        //foreach (var member in members)
                        //{
                        //    write.Display(member);
                        //    Debug.WriteLine("--------------------------------------------");
                        //}

                        return Task.FromResult(true);
                    }
                    catch (Exception ex)
                    {
                        FileLog.Write(ex.ToString(), "ErrorODDSendSkypeNotif");
                        return Task.FromResult(false);
                    }
                }
                public Task<bool> ODDSendSkypeImageNotif(Skype skype, string newFilename, string skypeappid, string skypeapppwd, string filecontent, string ftype)
                { //skype, fileName, botappid, botpwd, filecontent,ftype
                    try
                    {
                        string msAppId = skypeappid.ToString();
                        string msAppPassword = skypeapppwd.ToString();

                        string startupPath = Directory.GetCurrentDirectory();
                        string path = Path.Combine(startupPath, "include", "temp", newFilename);

                        var connector = new ConnectorClient(new Uri("https://skype.botframework.com"), msAppId, msAppPassword);
                        var conversation = new ConversationAccount(true, id: skype.Address);

                        string message = string.Empty;

                        List<Entity> listEntity = null;
                        if (skype.Mention != null)
                        {
                            var mt = "<at id=\"skype.Mention.Id\">skype.Mention.Name</at>";
                            mt = mt.Replace("skype.Mention.Id", skype.Mention.Id);
                            mt = mt.Replace("skype.Mention.Name", skype.Mention.Name);

                            var entity = new Microsoft.Bot.Schema.Mention()
                            {
                                Type = "mention",
                                Mentioned = new ChannelAccount { Id = skype.Mention.Id, Name = skype.Mention.Name },
                                Text = mt,
                            };


                            listEntity = new List<Entity>() { entity };
                            message = $"{skype.Message} <br />" + mt;
                        }
                        else
                        {
                            message = $"{skype.Message}";
                        }


                        var activity = new Microsoft.Bot.Schema.Activity();

                        //activity.Text = message;
                        activity.ServiceUrl = "https://smba.trafficmanager.net/apis/";
                        activity.ChannelId = "skype";
                        activity.Conversation = conversation;
                        activity.Timestamp = DateTime.UtcNow;
                        activity.Id = Guid.NewGuid().ToString();
                        activity.Type = ActivityTypes.Message;
                        activity.Locale = "en-En";
                        activity.TextFormat = "markdown";
                        activity.Attachments = new List<Attachment>() { ODDGetInlineAttachment(newFilename, filecontent, ftype) }; ;

                        connector.Conversations.SendToConversation(activity);

                        return Task.FromResult(true);
                    }
                    catch (Exception ex)
                    {
                        FileLog.Write(ex.ToString(), "ErrorODDSendSkypeImageNotif");
                        return Task.FromResult(false);
                    }
                }
        */
        #endregion
    }


}
