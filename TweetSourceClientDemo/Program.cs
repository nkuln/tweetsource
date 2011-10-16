using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSource.EventSource;
using TweetSource.OAuth;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Configuration;
using Newtonsoft.Json;

namespace TweetSourceClientDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("===== Application Started =====");

                // Create TweetEventSource and wire some event handlers.
                var source = TweetEventSource.CreateFilterStream();
                source.EventReceived += new EventHandler<TweetEventArgs>(source_EventReceived);
                source.SourceUp += new EventHandler<TweetEventArgs>(source_SourceUp);
                source.SourceDown += new EventHandler<TweetEventArgs>(source_SourceDown);

                // Load the configuration.
                LoadTwitterKeysFromConfig(source);

                // This starts another thread that pulls data from Twitter to our queue
                source.Start(new StreamingAPIParameters()
                {
                    Track = new string[] { "Thailand" }
                });

                // Dispatching events from queue. 
                while (source.Active)
                {
                    // This fires EventReceived callback on this thread
                    source.Dispatch();
                }

                Console.WriteLine("===== Application Ended =====");
            }
            catch (ConfigurationErrorsException cex)
            {
                Console.Error.WriteLine("Error reading config for Twitter's OAuth keys: " + cex.Message);
                Trace.TraceError("Read config failed: " + cex.ToString());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unknown error: " + ex.Message);
                Trace.TraceError("Unknown error: " + ex.ToString());
            }
        }

        private static void LoadTwitterKeysFromConfig(TweetEventSource source)
        {
            var settings = System.Configuration.ConfigurationManager.AppSettings;
            var config = source.AuthConfig;

            config.ConsumerKey = settings["ConsumerKey"];
            config.ConsumerSecret = settings["ConsumerSecret"];
            config.Token = settings["Token"];
            config.TokenSecret = settings["TokenSecret"];

            // These are default values:
            // config.OAuthVersion = "1.0";
            // config.SignatureMethod = "HMAC-SHA1";

            Console.WriteLine(config.ToString());
        }

        static void source_SourceDown(object sender, TweetEventArgs e)
        {
            // At this point, the connection thread ends
            Console.WriteLine("Source is down: " + e.InfoText);
            Trace.TraceInformation("Source is down: " + e.InfoText);
        }

        static void source_SourceUp(object sender, TweetEventArgs e)
        {
            // Connection established succesfully
            Console.WriteLine("Source is now ready: " + e.InfoText);
            Trace.TraceInformation("Source is now ready: " + e.InfoText);
        }

        static void source_EventReceived(object sender, TweetEventArgs e)
        {
            try
            {
                // JSON data from Twitter is in e.JsonText.
                // We parse data using Json.NET by James Newton-King 
                // http://james.newtonking.com/pages/json-net.aspx.
                //
                if (!string.IsNullOrEmpty(e.JsonText))
                {
                    
                    var tweet = JObject.Parse(e.JsonText);
                    string screenName = tweet["user"]["screen_name"].ToString();
                    string text = tweet["text"].ToString();

                    Console.WriteLine("{0,-15} => {1}", screenName, text);
                    Console.WriteLine();
                }
            }
            catch (JsonReaderException jex)
            {
                Console.Error.WriteLine("Error JSON read failed: " + jex.Message);
                Trace.TraceError("JSON read failed for text: " + e.JsonText);
                Trace.TraceError("JSON read failed exception: " + jex.ToString());
            }
        }
    }
}
