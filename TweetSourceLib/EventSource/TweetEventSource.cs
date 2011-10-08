using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;
using TweetSource.OAuth;
using System.Collections.Specialized;

namespace TweetSource.EventSource
{
    public abstract class TweetEventSource : EventSource<TweetEventArgs>
    {
        public string UserStreamUrl { get; set; }
        public string SampleStreamUrl { get; set; }
        public string FilterStreamUrl { get; set; }
        public string LinkStreamUrl { get; set; }
        public string RetweetStreamUrl { get; set; }

        public abstract AuthParameterSet AuthConfig { get; }

        public abstract NameValueCollection PostData { get; }

        public static TweetEventSource Create()
        {
            return new TweetEventSourceImpl();
        }

        public abstract void StartUserStream(StreamingAPIParameters p = null);
        public abstract void StartSampleStream(StreamingAPIParameters p = null);
        public abstract void StartFilterStream(StreamingAPIParameters p = null);

        public abstract void StopAll();
    }

    public class TweetEventSourceImpl : TweetEventSource
    {
        protected const string DefaultUserStreamUrl = "https://userstream.twitter.com/2/user.json";
        protected const string DefaultSampleStreamUrl = "https://stream.twitter.com/1/statuses/sample.json";
        protected const string DefaultFilterStreamUrl = "https://stream.twitter.com/1/statuses/filter.json";
        protected const string DefaultLinkStreamUrl = "https://stream.twitter.com/1/statuses/links.json";
        protected const string DefaultRetweetUrl = "https://stream.twitter.com/1/statuses/retweet.json";

        protected Thread requestThread;

        protected AuthParameterSet config;
        public override AuthParameterSet AuthConfig
        {
            get { return config; }
        }

        protected NameValueCollection postData;
        public override NameValueCollection PostData
        {
            get { return postData; }
        }

        protected HttpWebRequest request;

        public TweetEventSourceImpl()
        {
            config = new AuthParameterSet();
            postData = new NameValueCollection();

            UserStreamUrl = DefaultUserStreamUrl;
            SampleStreamUrl = DefaultSampleStreamUrl;
            FilterStreamUrl = DefaultFilterStreamUrl;
            LinkStreamUrl = DefaultLinkStreamUrl;
            RetweetStreamUrl = DefaultRetweetUrl;
        }

        public override void StartUserStream(StreamingAPIParameters p)
        {
            request = (HttpWebRequest)WebRequest.Create(DefaultUserStreamUrl);
            request.Method = "POST";

            StartThread();
        }

        public override void StartFilterStream(StreamingAPIParameters p)
        {
            request = (HttpWebRequest)WebRequest.Create(DefaultFilterStreamUrl);
            request.Method = "POST";

            StartThread();
        }

        public override void StartSampleStream(StreamingAPIParameters p)
        {
            request = (HttpWebRequest)WebRequest.Create(DefaultSampleStreamUrl);
            request.Method = "GET";

            StartThread();
        }

        private void StartThread()
        {
            if (requestThread != null)
            {
                throw new ApplicationException(
                    "Cannot start user stream because another subscription " +
                    "is still running. Call StopAll() first");
            }

            requestThread = new Thread(new ThreadStart(RunThead));
            requestThread.Start();
        }

        public void RunThead()
        {
            try
            {
                RequestData();
            }
            catch (WebException wex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonData = "",
                    InfoText = "Connection down (web exception): " + wex.ToString()
                });
            }
            catch (ApplicationException aex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonData = "",
                    InfoText = "Connection down: " + aex.ToString()
                });
            }
            catch (Exception ex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonData = "",
                    InfoText = "Connection down (unhandled exception): " + ex.ToString()
                });
            }
            finally
            {
                requestThread = null;
            }
        }

        private void RequestData()
        {
            var authRequest = CreateOAuthWebRequest(request, config);
            var resp = authRequest.GetResponse(postData);

            FireSourceUp(new TweetEventArgs()
            {
                JsonData = "",
                InfoText = "Connection established"
            });

            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                string val;
                while ((val = sr.ReadLine()) != null)
                {
                    Debug.WriteLine(val);

                    FireDataArrived(new TweetEventArgs()
                    {
                        JsonData = val,
                        InfoText = "Got new data"
                    });
                }
            }
        }

        public override void StopAll()
        {
            throw new NotImplementedException();
        }

        protected virtual OAuthWebRequest CreateOAuthWebRequest(HttpWebRequest baseRequest, 
            AuthParameterSet config)
        {
            return new OAuthWebRequestImpl(baseRequest, config);
        }

    }

    public class StreamingAPIParameters
    {
        public int Count { get; set; }
        public int Delimited { get; set; }
        public int[] Follow { get; set; }
        public string[] Track { get; set; }
        public double[] Locations { get; set; }

        public StreamingAPIParameters()
        {
            // Initialized to default values
            Count = 0;
            Delimited = 0;
            Follow = new int[] { };
            Track = new string[] { };
            Locations = new double[] { };
        }
    }

    public class TweetEventArgs : EventArgs
    {
        public string JsonData { get; set; }
        public string InfoText { get; set; }
    }

}
