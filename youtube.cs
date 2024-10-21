using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Airgeadlamh.YoutubeUploader
{
    internal class UploadVideo
    {
        public static String video_title = "";
        public static string streamer_name = "";
        public static bool clip_exists = false;
        
        
        [STAThread]
        public static void upload(){
            //Console.WriteLine("Checking if clip exists");
            //Console.WriteLine("============================");
            //try
            //{
            //    new MyUploads().Run().Wait();
            //}
            //catch (AggregateException ex)
            //{
            //    foreach (var e in ex.InnerExceptions)
            //    {
            //    Console.WriteLine("Error: " + e.Message);
            //    }
            //}
//
            //if (clip_exists)
            //{
            //    Console.WriteLine("Clip already uploaded to channel, exiting");
            //    return;
            //}

            Console.WriteLine($"Uploading {video_title} to youtube");
            Console.WriteLine("==============================");
            try
            {
                new UploadVideo().Run().Wait();
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                Console.WriteLine("Error: " + e.Message);
                }
            }
        }
        public async Task Run()
        {
        UserCredential credential;
        using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                // This OAuth 2.0 access scope allows an application to upload files to the
                // authenticated user's YouTube channel, but doesn't allow other types of access.
                new[] { YouTubeService.Scope.YoutubeUpload },
                "user",
                CancellationToken.None
            );
        }

        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
        });

        var video = new Video();
        video.Snippet = new VideoSnippet();
        video.Snippet.Title = video_title;
        video.Snippet.Description = $"Source: {Program.stream_link} \nYoutube: {Program.streamer_name}";
        video.Snippet.Tags = new string[] { Program.streamer_name, "karaoke" };
        video.Snippet.CategoryId = "10"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
        video.Status = new VideoStatus();
        video.Status.PrivacyStatus = "unlisted"; // or "private" or "public"
        var filePath = Program.mp4_path;

        using (var fileStream = new FileStream(filePath, FileMode.Open))
        {
            var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
            videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
            videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

            await videosInsertRequest.UploadAsync();
        }
        }

        void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
        switch (progress.Status)
        {
            case UploadStatus.Uploading:
            Console.WriteLine("{0} bytes sent.", progress.BytesSent);
            break;

            case UploadStatus.Failed:
            Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
            break;
        }
        }

        void videosInsertRequest_ResponseReceived(Video video)
        {
        Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);
        }
    }

    internal class MyUploads
    {
        public async Task Run()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
                {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    
                    // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                    // user's account, but not other types of account access.
                    new[] { YouTubeService.Scope.YoutubeReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(this.GetType().ToString())
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.GetType().ToString()
            });

            var channelsListRequest = youtubeService.Channels.List("contentDetails");
            channelsListRequest.Mine = true;

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var channelsListResponse = await channelsListRequest.ExecuteAsync();

            foreach (var channel in channelsListResponse.Items)
            {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.
                var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                //Console.WriteLine("Videos in list {0}", uploadsListId);

                var nextPageToken = "";
                while (nextPageToken != null)
                {
                var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = uploadsListId;
                playlistItemsListRequest.MaxResults = 50;
                playlistItemsListRequest.PageToken = nextPageToken;

                // Retrieve the list of videos uploaded to the authenticated user's channel.
                var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                foreach (var playlistItem in playlistItemsListResponse.Items)
                {
                    // Print information about each video.
                    //Console.WriteLine("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
                    if (playlistItem.Snippet.Title == UploadVideo.video_title)
                    {
                        UploadVideo.clip_exists = true;
                        return;
                    }
                }

                nextPageToken = playlistItemsListResponse.NextPageToken;
                }
            }
        }
    }
}
