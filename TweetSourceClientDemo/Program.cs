using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSource.EventSource;

namespace TweetSourceClientDemo
{
    class Program
    {
        protected static bool LoadFromConfigFile = true;

        static void Main(string[] args)
        {
            string consumerKey, consumerSecret, token, tokenSecret;

            if (LoadFromConfigFile)
            {
                var settings = System.Configuration.ConfigurationManager.AppSettings;
                consumerKey = settings["ConsumerKey"];
                consumerSecret = settings["ConsumerSecret"];
                token = settings["Token"];
                tokenSecret = settings["TokenSecret"];
            }
            else
            {
                consumerKey = "your consumer key";
                consumerSecret = "your consumer secret";
                token = "your access token";
                tokenSecret = "your access token secret";
            }

            var tweetSource = TweetEventSource.Create();

            tweetSource.DataArrived += (s, e) =>
            {
                // JSON data from Twitter is in e.JsonData
                Console.WriteLine("New Data: " + e.JsonData);
            };
            tweetSource.SourceUp += (s, e) =>
            {
                // Connection established succesfully
                Console.WriteLine("Source Up: " + e.InfoText);
            };
            tweetSource.SourceDown += (s, e) =>
            {
                // At this point, the connection thread ends
                Console.WriteLine("Source Down: " + e.InfoText);
                Console.WriteLine("===== Application Ends =====");
                Console.Read();
            };

            // This starts another thread, openining connection to Twitter
            tweetSource.StartUserStream(consumerKey, consumerSecret, token, tokenSecret);
        }
    }
}
