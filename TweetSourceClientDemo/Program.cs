using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSource.EventSource;
using TweetSource.OAuth;

namespace TweetSourceClientDemo
{
    class Program
    {
        protected static bool LoadFromConfigFile = true;

        static void Main(string[] args)
        {
            var source = TweetEventSource.Create();

            source.DataArrived += (s, e) =>
            {
                // JSON data from Twitter is in e.JsonData
                Console.WriteLine("New Data: " + e.JsonData);
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
            source.StartSampleStream();
        }
    }
}
