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
        private const int RECONNECT_BASE_TIME_MS = 10000;
        private const int RECONNECT_MAX_TIME_MS = 240000;
        private static int waitReconectTime = 0;
        
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("===== Application Started =====");

                // Step 1: Create TweetEventSource and wire some event handlers.
                var source = TweetEventSource.CreateFilterStream();
                source.EventReceived += new EventHandler<TweetEventArgs>(source_EventReceived);
                source.SourceUp += new EventHandler<TweetEventArgs>(source_SourceUp);
                source.SourceDown += new EventHandler<TweetEventArgs>(source_SourceDown);

                // Step 2: Load the configuration into event source
                LoadTwitterKeysFromConfig(source);

                // Step 3: Main loop, e.g. retries 5 times at most
                int retryCount = 0;
                while (retryCount++ < 5)
                {
                    // Step 4: Starts the event source. This starts another thread that pulls data from Twitter to our queue.
                    source.Start(new StreamingAPIParameters()
                    {
                        Track = new string[] { "Thailand" }
                    });

                    // Step 5: While our event source is Active, dispatches events
                    while (source.Active)
                    {
                        source.Dispatch(1000); // This fires EventReceived callback on this thread
                    }

                    // Step 6: Source is inactive. Ensure stop and cleanup things
                    source.Stop();

                    // Step 7: Wait for some time before attempt reconnect
                    Console.WriteLine("=== Disconnected, wait for {0} ms before reconnect ===", Program.waitReconectTime);
                    Thread.Sleep(Program.waitReconectTime);
                }

                Console.WriteLine("===== Application Ended =====");
            }
            catch (ConfigurationErrorsException cex)
            {
                Console.Error.WriteLine(@"Error reading config: If you're running this for the first time, " +
                    "please make sure you have your version of Twitter.config at application's " +
                    "working directory - " + cex.Message);

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

            // Calculate new wait time exponetially
            Program.waitReconectTime = Program.waitReconectTime > 0 ?
                Program.waitReconectTime * 2 : RECONNECT_BASE_TIME_MS;
            Program.waitReconectTime = Program.waitReconectTime > RECONNECT_MAX_TIME_MS ?
                RECONNECT_MAX_TIME_MS : Program.waitReconectTime;
        }

        static void source_SourceUp(object sender, TweetEventArgs e)
        {
            // Connection established succesfully
            Console.WriteLine("Source is now ready: " + e.InfoText);
            Trace.TraceInformation("Source is now ready: " + e.InfoText);

            // Reset wait time
            Program.waitReconectTime = 0;
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
