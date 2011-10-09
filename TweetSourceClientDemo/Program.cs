using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSource.EventSource;
using TweetSource.OAuth;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace TweetSourceClientDemo
{
    class Program
    {
        protected static bool LoadFromConfigFile = true;

        static void Main(string[] args)
        {
            var source = TweetEventSource.CreateSampleStream();

            source.EventReceived += (s, e) =>
            {
                // JSON data from Twitter is in e.JsonData
                Console.WriteLine("New Data: Total length = {0}", e.JsonText.Length);
                var json = JObject.Parse(e.JsonText);
                Console.WriteLine(json.ToString());
                //Console.WriteLine("{0},{1}",json["text"], json["user"]["screen_name"]);
            };

            source.SourceUp += (s, e) =>
            {
                // Connection established succesfully
                Console.WriteLine("Source Up: " + e.InfoText);
            };

            source.SourceDown += (s, e) =>
            {
                // At this point, the connection thread ends
                Console.WriteLine("Source Down: " + e.InfoText);
                Console.WriteLine("===== Application Ends =====");
                Console.Read();
            };

            var config = source.AuthConfig ;

            if (LoadFromConfigFile)
            {
                var settings = System.Configuration.ConfigurationManager.AppSettings;
                config.ConsumerKey = settings["ConsumerKey"];
                config.ConsumerSecret = settings["ConsumerSecret"];
                config.Token = settings["Token"];
                config.TokenSecret = settings["TokenSecret"];
            }
            else
            {
                config.ConsumerKey = "your consumer key";
                config.ConsumerSecret = "your consumer secret";
                config.Token = "your access token";
                config.TokenSecret = "your access token secret";
            }

            // This starts another thread, openining connection to Twitter
            // tweetSource.StartUserStream();

            //source.Start(new StreamingAPIParameters()
            //{
            //    Track = new string[]{"Steve Jobs"}
            //});
            source.Start();
        }
    }
}
