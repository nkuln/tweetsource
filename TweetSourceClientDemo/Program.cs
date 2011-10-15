using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSource.EventSource;
using TweetSource.OAuth;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace TweetSourceClientDemo
{
    class Program
    {
        protected static bool LoadFromConfigFile = true;

        static void Main(string[] args)
        {
            // Create TweetEventSource and wire some event handlers.
            //
            var source = TweetEventSource.CreateFilterStream();

            source.EventReceived += new EventHandler<TweetEventArgs>(source_EventReceived);
            source.SourceUp += new EventHandler<TweetEventArgs>(source_SourceUp);
            source.SourceDown += new EventHandler<TweetEventArgs>(source_SourceDown);

            // Load the configuration.
            //
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

            // Print out config read
            //
            Console.WriteLine(config.ToString());

            // Call Start(). This starts background thread that use HTTPS to 
            // pull data from Twitter into internal event queue
            //
            source.Start(new StreamingAPIParameters()
            {
                Track = new string[] { "Thailand" }
            });

            // Dispatching events. This fires EventReceived callback.
            //
            while (true)
            {
                source.Dispatch();
            }
        }

        static void source_SourceDown(object sender, TweetEventArgs e)
        {
            // At this point, the connection thread ends
            //
            Console.WriteLine("Source Down: " + e.InfoText);
            Console.WriteLine("===== Application Ends =====");
            Console.Read();
        }

        static void source_SourceUp(object sender, TweetEventArgs e)
        {
            // Connection established succesfully
            //
            Console.WriteLine("Source Up: " + e.InfoText);
        }

        static void source_EventReceived(object sender, TweetEventArgs e)
        {
            try
            {
                // JSON data from Twitter is in e.JsonText
                //
                if (!string.IsNullOrEmpty(e.JsonText))
                {
                    var json = JObject.Parse(e.JsonText);
                    Console.WriteLine("{0,-15} => {1}",
                    json["user"]["screen_name"], json["text"]);
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: Parse failed for \"{0}\"", e.JsonText);
                Trace.WriteLine(ex.ToString());
            }
        }
    }
}
