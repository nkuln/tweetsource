using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace TweetSource.EventSource
{
    public class GetBasedTweetEventSourceImpl : TweetEventSourceBaseImpl
    {
        protected static string ConstructUrlWithQueryString(string url, StreamingAPIParameters p)
        {
            // This technique create a specialized version of NameValueCollection.
            // Calling its ToString() method yields a correctly encoded querystring.
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

        protected override HttpWebRequest CreateWebRequest(StreamingAPIParameters p)
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
}
