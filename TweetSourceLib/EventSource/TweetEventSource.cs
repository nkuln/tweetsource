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
using System.Web;
using TweetSourceLib.Util;

namespace TweetSource.EventSource
{
    public abstract class TweetEventSource : EventSource<TweetEventArgs>
    {
        public string StreamRequestUrl { get; set; }

        public abstract AuthParameterSet AuthConfig { get; }

        public abstract NameValueCollection PostData { get; }

        public static TweetEventSource CreateFilterStream()
        {
            return new FilterStreamEventSourceImpl();
        }

        public static TweetEventSource CreateRetweetStream()
        {
            return new RetweetStreamEventSourceImpl();
        }

        public static TweetEventSource CreateLinkStream()
        {
            return new LinkStreamEventSourceInmpl();
        }

        public static TweetEventSource CreateSampleStream()
        {
            return new SampleStreamEventSourceImpl();
        }

        public static TweetEventSource CreateUserStrean()
        {
            return new UserStreamEventSourceImpl();
        }

        public abstract void Start(StreamingAPIParameters p = null);

        public abstract void Stop();
    }

    public abstract class TweetEventSourceBaseImpl : TweetEventSource
    {
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

        public TweetEventSourceBaseImpl()
        {
            config = new AuthParameterSet();
            postData = new NameValueCollection();
        }

        public sealed override void Start(StreamingAPIParameters p = null)
        {
            request = CreateHttpRequestWithParameters(p);

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
                    JsonText = "",
                    InfoText = "Connection down (web exception): " + wex.ToString()
                });
            }
            catch (ApplicationException aex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonText = "",
                    InfoText = "Connection down: " + aex.ToString()
                });
            }
            catch (Exception ex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonText = "",
                    InfoText = "Connection down (unhandled exception): " + ex.ToString()
                });
            }
            finally
            {
                requestThread = null;
            }
        }

        protected void RequestData()
        {
            var authRequest = CreateOAuthWebRequest(request, config);
            var resp = authRequest.GetResponse(postData);

            FireSourceUp(new TweetEventArgs()
            {
                JsonText = string.Empty,
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
                        JsonText = val,
                        InfoText = "Got new data"
                    });
                }
            }
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        protected virtual OAuthWebRequest CreateOAuthWebRequest(HttpWebRequest baseRequest,
            AuthParameterSet config)
        {
            return new OAuthWebRequestImpl(baseRequest, config);
        }

        protected abstract HttpWebRequest CreateHttpRequestWithParameters(StreamingAPIParameters p);
    }

    public class PostBasedTweetEventSourceImpl : TweetEventSourceBaseImpl
    {
        protected override HttpWebRequest CreateHttpRequestWithParameters(StreamingAPIParameters p)
        {
            var postData = ConstructPostData(p);
            var request = (HttpWebRequest)WebRequest.Create(StreamRequestUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                string encoded = HttpUtil.EncodeFormPostData(postData);
                if (!string.IsNullOrEmpty(encoded))
                    sw.Write(encoded);
            }

            return request;
        }

        protected static NameValueCollection ConstructPostData(StreamingAPIParameters p)
        {
            var postData = HttpUtility.ParseQueryString(string.Empty);

            if (p != null)
            {
                if (p.Count != 0) postData.Add("count", p.Count.ToString());
                if (p.Delimited != 0) postData.Add("delimited", p.Delimited.ToString());
                if (p.Follow.Length != 0)
                    postData.Add("follow", string.Join(",",
                        p.Follow.Select(x => x.ToString()).ToArray()));

                if (p.Locations.Length != 0)
                    postData.Add("locations", string.Join(",",
                        p.Locations.Select(x => x.ToString()).ToArray()));

                if (p.Track.Length != 0)
                    postData.Add("track", string.Join(",", p.Track));
            }

            return postData;
        }

    }

    public class UserStreamEventSourceImpl : PostBasedTweetEventSourceImpl
    {
        protected const string DefaultUserStreamUrl = "https://userstream.twitter.com/2/user.json";

        public UserStreamEventSourceImpl()
        {
            StreamRequestUrl = DefaultUserStreamUrl;
        }
    }

    public class FilterStreamEventSourceImpl : PostBasedTweetEventSourceImpl
    {
        protected const string DefaultFilterStreamUrl = "https://stream.twitter.com/1/statuses/filter.json";

        public FilterStreamEventSourceImpl()
        {
            StreamRequestUrl = DefaultFilterStreamUrl;
        }
    }

    public class GetBasedTweetEventSourceImpl : TweetEventSourceBaseImpl
    {
        protected static string ConstructUrlWithQueryString(string url, StreamingAPIParameters p)
        {
            var nv = HttpUtility.ParseQueryString(string.Empty);

            if (p != null)
            {
                if (p.Count != 0)
                    nv.Add("count", p.Count.ToString());
                if (p.Delimited != 0)
                    nv.Add("delimited", p.Delimited.ToString());
            }

            return url + (nv.Count == 0 ? string.Empty : nv.ToString());

        }

        protected override HttpWebRequest CreateHttpRequestWithParameters(StreamingAPIParameters p)
        {
            string url = ConstructUrlWithQueryString(StreamRequestUrl, p);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            return request;
        }
    }


    public class RetweetStreamEventSourceImpl : GetBasedTweetEventSourceImpl
    {
        protected const string DefaultRetweetUrl = "https://stream.twitter.com/1/statuses/retweet.json";

        public RetweetStreamEventSourceImpl()
        {
            StreamRequestUrl = DefaultRetweetUrl;
        }
    }

    public class LinkStreamEventSourceInmpl : GetBasedTweetEventSourceImpl
    {
        protected const string DefaultLinkStreamUrl = "https://stream.twitter.com/1/statuses/links.json";

        public LinkStreamEventSourceInmpl()
        {
            StreamRequestUrl = DefaultLinkStreamUrl;
        }
    }

    public class SampleStreamEventSourceImpl : GetBasedTweetEventSourceImpl
    {
        protected const string DefaultSampleStreamUrl = "https://stream.twitter.com/1/statuses/sample.json";

        public SampleStreamEventSourceImpl()
        {
            StreamRequestUrl = DefaultSampleStreamUrl;
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
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            Count = 0;
            Delimited = 0;
            Follow = new int[] { };
            Track = new string[] { };
            Locations = new double[] { };
        }
    }

    public class TweetEventArgs : EventArgs
    {
        public string JsonText { get; set; }
        public string InfoText { get; set; }
    }

}
