using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using ShareFile.Api.Client;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Models;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Client.Transfers;
using Microsoft.Extensions.Configuration;
using A2B_APP_Extension.Model;
using System.IO;
using Ionic.Zip;
using A2B_APP_Extension.Controllers;
using A2B_APP_Extension.Data;
using OpenCvSharp;

namespace A2BHQ_Desktop_VideoRendering.Controllers.Sharefile
{
    public class SFRequests
    {
        public async Task<dynamic> GetItemChildren(string folderId)
        {
            try
            {
                Session session = null;
                ShareFileClient sfClient = null;
                SharefileAPI SFCred = new SharefileAPI();
                SharefileUser user = new SharefileUser()
                {
                    ControlPlane = SFCred.ControlPlane,
                    Username = SFCred.UserName,
                    Password = SFCred.Password,
                    Subdomain = SFCred.Subdomain,
                };
                string oauthClientId = SFCred.ClientId;
                string oauthClientSecret = SFCred.ClientSecret;

                //Username = "development.a2bhq@gmail.com",
                //Password = "Sharefilead*963.",
                // Authenticate with username/password
                sfClient = await PasswordAuthentication(user, oauthClientId, oauthClientSecret);

                //Start session
                session = await sfClient.Sessions.Login().Expand("Principal").ExecuteAsync();
                var sfFolders = await sfClient.Items.GetChildren(sfClient.Items.GetEntityUriFromId(folderId), false).ExecuteAsync();
                return sfFolders;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        private async Task<ShareFileClient> PasswordAuthentication(SharefileUser SFUser, string ClientID, string ClientSecret)
        {
            // Initialize ShareFileClient.
            var configuration = Configuration.Default();
            configuration.Logger = new DefaultLoggingProvider();

            var sfClient = new ShareFileClient("https://secure.sf-api.com/sf/v3/", configuration);
            var oauthService = new OAuthService(sfClient, ClientID, ClientSecret);

            // Perform a password grant request.  Will give us an OAuthToken
            var oauthToken = await oauthService.PasswordGrantAsync(SFUser.Username, SFUser.Password, SFUser.Subdomain, SFUser.ControlPlane);

            // Add credentials and update sfClient with new BaseUri
            sfClient.AddOAuthCredentials(oauthToken);
            sfClient.BaseUri = oauthToken.GetUri();
            //Console.WriteLine("Sharefile authenticated");

            return sfClient;
        }

        public async Task<dynamic> ProcessDownLoad(string resourceId)
        {
            Item sfItem = new Item();
            try
            {

                Session session = null;
                ShareFileClient sfClient = null;
                SharefileAPI SFCred = new SharefileAPI();
                SharefileUser user = new SharefileUser()
                {
                    ControlPlane = SFCred.ControlPlane,
                    Username = SFCred.UserName,
                    Password = SFCred.Password,
                    Subdomain = SFCred.Subdomain,
                };

                string oauthClientId = SFCred.ClientId;
                string oauthClientSecret = SFCred.ClientSecret;

                sfClient = await PasswordAuthentication(user, oauthClientId, oauthClientSecret);
                session = await sfClient.Sessions.Login().Expand("Principal").ExecuteAsync();

                KeyReportFilter filter = new KeyReportFilter();

                if (session.IsAuthenticated)
                {
                    var sfItem1 = await sfClient.Items.Get(sfClient.Items.GetEntityUriFromId(resourceId), false).ExecuteAsync();
                    if (sfItem1 != null && sfItem1.FileName != null)
                    {
                        string FileName = sfItem1.FileName;

                        var sfItemRaw = await sfClient.Items.Stream(sfClient.Items.GetEntityUriFromId(sfItem1.StreamID), false).ExecuteAsync();
                        if (sfItemRaw != null)
                        {
                            foreach (var item2 in sfItemRaw.Feed)
                            {
                                if (!CheckFileIfExistsVideo(item2.FileName, item2.FileSizeBytes))
                                {
                                    if (await Download(sfClient, item2, filter, "video", item2.FileName))
                                    {
                                        string startupPath = Directory.GetCurrentDirectory();
                                        string savePath = Path.Combine(startupPath, "include", "sharefile", "download", "screenshots", item2.FileName);
                                        string file = Path.Combine(startupPath, "include", "sharefile", "download", "video", item2.FileName, item2.FileName);

                                        if (!Directory.Exists(savePath))
                                        {
                                            Directory.CreateDirectory(savePath);
                                            var taskStart = Task.Run(() => VideoExtractor(file, savePath, item2.FileName));
                                            Task.WhenAll(taskStart).Wait();

                                            Console.WriteLine($"Video Extraced Successfully | {DateTime.Now}");
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Post SkypeRequest = new Post();
                SkypeAddressId SkypeId = new SkypeAddressId();
                await SkypeRequest.SkypeSendMessage(err.ToString(), SkypeId.MarkCG);
                return false;
            }

            return false;
        }

        private bool CheckFileIfExistsVideo(string filename, long? fileSize)
        {
            try
            {
                string startupPath = Directory.GetCurrentDirectory();
                string path = Path.Combine(startupPath, "include", "sharefile", "download", "video", filename);
                FileInfo file = new FileInfo(path);
                if (file != null && file.Exists)
                {
                    if (file.Length.Equals(fileSize))
                    {
                        Console.WriteLine($"Item {filename} already exists");
                        return true;
                    }
                }
                return false;
            }
            catch (Exception err)
            {
                Post SkypeRequest = new Post();
                SkypeAddressId SkypeId = new SkypeAddressId();
                _ = SkypeRequest.SkypeSendMessage(err.ToString(), SkypeId.MarkCG);
                return false;
            }

        }

        public static async Task<bool> Download(ShareFileClient sfClient, Item itemToDownload, dynamic filter, string ItemType, string filename)
        {
            try
            {
                string result = string.Empty;
                string startupPath = Directory.GetCurrentDirectory();
                string path = Path.Combine(startupPath, "include", "sharefile", "download", $"{filter.ClientName}", $"{filter.FY}", $"{filter.KeyReportName}");

                if (ItemType == "video")
                {
                    path = Path.Combine(startupPath, "include", "sharefile", "download", "video", filename);
                }

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var downloader = sfClient.GetAsyncFileDownloader(itemToDownload);
                var file = System.IO.File.Open(Path.Combine(path, itemToDownload.Name), FileMode.Append);

                decimal percent = 0;
                downloader.OnTransferProgress += (sender, e) =>
                {
                    //Download progress
                    percent = (((decimal)(e.Progress.BytesTransferred) / (decimal)(e.Progress.TotalBytes)) * 100);
                    Console.WriteLine("Downloading " + e.Progress.BytesTransferred + "/" + e.Progress.TotalBytes + " - " + percent.ToString("0.##") + "%");

                    //Download completed
                    if (e.Progress.Complete)
                    {
                        Console.WriteLine("Download complete - " + itemToDownload.Name);
                        //result = startupPath + @"\files\" + ItemToDownload.Name;
                        file.Dispose();
                    }
                };

                await downloader.DownloadToAsync(file);
                return true;
            }
            catch (Exception err)
            {
                Post SkypeRequest = new Post();
                SkypeAddressId SkypeId = new SkypeAddressId();
                //_ = SkypeRequest.SkypeSendMessage(err.ToString(), SkypeId.MarkCG);
                return false;
                //throw;
            }
        }

        public async Task<bool> VideoExtractor(string file, string savePath, string fileName)
        {
            bool isUploaded = false;
            List<string> listImage = new List<string>();
            try
            {
                #region /*old code*/
                /*using (var engine = new Engine())
                {
                    var mp4 = new MediaFile { Filename = file };

                    engine.GetMetadata(mp4);

                    var i = 1.0;
                    while (i < mp4.Metadata.Duration.Milliseconds)
                    {
                        var options = new ConversionOptions { Seek = TimeSpan.FromMilliseconds(i) };
                        var outputFile = new MediaFile { Filename = string.Format("{0}\\image-{1}.jpg", savePath, i) };
                        listImage.Add("image-" + i + ".jpg");
                        engine.GetThumbnail(mp4, outputFile, options);
                        i+=1.0;
                    }   
                }*/
                #endregion

                System.IO.Directory.CreateDirectory(savePath);
                var capture = new VideoCapture(file);
                var image = new Mat();
                int i = 0;
                Console.WriteLine("Begin extracting frames from video file..");
                while (capture.IsOpened())
                {
                    // Read next frame in video file
                    capture.Read(image);
                    if (image.Empty())
                    {
                        break;
                    }
                    if (i % 1000 == 0)
                    {
                        //Save image to disk.
                        Cv2.ImWrite(String.Format(savePath + "\\frame{0}.png", i), image);
                        Console.WriteLine(String.Format("Succesfully saved frame {0} to disk.", i));
                        listImage.Add("frame" + i + ".png");
                    }
                    i++;
                }
                Console.WriteLine(String.Format("Finished, check output at: {0}.", Path.GetFullPath(savePath)));


                var taskCompressReportFile = Task.Run(() => CompressItem(savePath, $"{fileName}-KRVideo--Extracted", listImage));
                Task.WhenAll(taskCompressReportFile).Wait();
                string zipFileName = taskCompressReportFile.Result;
                Console.WriteLine($"DONE Compress files | {DateTime.Now}");


                #region Upload to Sharefile

                Console.WriteLine($"Upload to Sharefile | {DateTime.Now}");

                if (zipFileName != string.Empty)
                {
                    SharefileItem sfItem = new SharefileItem();
                    sfItem.FileName = zipFileName;
                    sfItem.FilePath = Path.Combine(savePath, zipFileName);
                    sfItem.Directory = "KeyReportScreenshotResult";

                    var taskUploadToSF = Task.Run(() => UploadWithItemReturn(sfItem));
                    Task.WhenAll(taskUploadToSF).Wait();

                    Console.WriteLine($"DONE Upload to Sharefile {taskUploadToSF.Id}| {DateTime.Now}");

                    string startupPath = Directory.GetCurrentDirectory();
                    string dirVideo = Path.Combine(startupPath, "include", "sharefile", "download", "video", fileName);

                    //Directory.Delete(savePath, true);
                    //Directory.Delete(dirVideo, true);
                }
                #endregion

            }
            catch (Exception err)
            {
                Post SkypeRequest = new Post();
                SkypeAddressId SkypeId = new SkypeAddressId();
                _ = SkypeRequest.SkypeSendMessage(err.ToString(), SkypeId.MarkCG);
                //return false;
            }
            return isUploaded;
        }

        public Task<string> CompressItem(string path, string reportname, List<string> listFileName)
        {
            string zipFileName = string.Empty;
            try
            {
                List<string> fileFullPath = new List<string>();
                using (ZipFile zip = new ZipFile())
                {
                    if (listFileName.Any())
                    {
                        foreach (var item in listFileName)
                        {
                            fileFullPath.Add(Path.Combine(path, item));
                        }
                        zip.AddFiles(fileFullPath, false, "");
                        string dtNow = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string zipFullPath = Path.Combine(path, $"{dtNow}_{reportname}.zip");
                        zipFileName = $"{dtNow}_{reportname}.zip";
                        zip.Save(zipFullPath);
                    }
                }

            }
            catch (Exception err)
            {
                Post SkypeRequest = new Post();
                SkypeAddressId SkypeId = new SkypeAddressId();
                _ = SkypeRequest.SkypeSendMessage(err.ToString(), SkypeId.MarkCG);
            }
            return Task.FromResult(zipFileName);

        }

        public async Task<dynamic> UploadWithItemReturn(SharefileItem sharefileItem)
        {
            try
            {
                Session session = null;
                ShareFileClient sfClient = null;
                SharefileAPI SFCred = new SharefileAPI();
                SharefileUser user = new SharefileUser()
                {
                    ControlPlane = SFCred.ControlPlane,
                    Username = SFCred.UserName,
                    Password = SFCred.Password,
                    Subdomain = SFCred.Subdomain,
                };

                string oauthClientId = SFCred.ClientId;
                string oauthClientSecret = SFCred.ClientSecret;

                // Authenticate with username/password
                sfClient = await PasswordAuthentication(user, oauthClientId, oauthClientSecret);

                //Start session
                session = await sfClient.Sessions.Login().Expand("Principal").ExecuteAsync();
                //string filePath = sharefileItem.FilePath;
                var fileExtension = sharefileItem.FileName.Split('.').Last();
                var fileNameOnly = sharefileItem.FileName.Split('.').First();

                string sfDirectory = "/Shared Folders/Key Reports/Recording/Keyreport Video Screenshot results";
                string sfLink = "https://a2q2.sharefile.com/home/shared/fo9516f7-0c8d-4333-be45-7a2c3077cb17";


                Folder folder = (Folder)await sfClient.Items.ByPath(sfDirectory).ExecuteAsync();

                var uploadedFileId = await UploadFile(sfClient, folder, sharefileItem.FilePath, fileExtension, null, fileNameOnly);

                var itemUri = sfClient.Items.GetAlias(uploadedFileId);
                var uploadedFile = await sfClient.Items.Get(itemUri).ExecuteAsync();
                Console.WriteLine($"Successfully uploaded {uploadedFile}");
                return uploadedFile;
            }
            catch (Exception err)
            {
                Post SkypeRequest = new Post();
                SkypeAddressId SkypeId = new SkypeAddressId();
                _ = SkypeRequest.SkypeSendMessage(err.ToString(), SkypeId.MarkCG);
            }
            return "something went wrong";
        }


        private async Task<string> UploadFile(
            ShareFileClient sfClient,
            Folder destinationFolder,
            string FilePath,
            string FileExtension,
            string FileDetails,
            string RecordName)
        {
            string result = string.Empty;
            var file = System.IO.File.Open(FilePath, FileMode.OpenOrCreate);
            var uploadRequest = new UploadSpecificationRequest
            {
                FileName = RecordName + @"." + FileExtension,
                FileSize = file.Length,
                Details = FileDetails,
                Parent = destinationFolder.url
            };

            var uploader = sfClient.GetAsyncFileUploader(uploadRequest, file);
            decimal percent = 0;
            var uploadResponse = await uploader.UploadAsync();

            uploader.OnTransferProgress += (sender, e) =>
            {
                //Download progress
                percent = (((decimal)(e.Progress.BytesTransferred) / (decimal)(e.Progress.TotalBytes)) * 100);
                Console.WriteLine("Uploading " + e.Progress.BytesTransferred + "/" + e.Progress.TotalBytes + " - " + percent.ToString("0.##") + "%");

                //Download completed
                if (e.Progress.Complete)
                {
                    Console.WriteLine("Upload complete - " + file.Name);
                    file.Dispose();
                }
            };

            result = uploadResponse.First().Id;
            return result;

        }
    }
}
