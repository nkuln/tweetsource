using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using TweetSource.Util;

namespace TweetSource.EventSource
{
    public class PostBasedTweetEventSourceImpl : TweetEventSourceBaseImpl
    {
        protected override HttpWebRequest CreateWebRequest(StreamingAPIParameters p)
        {
            PostData.Add(ConstructPostData(p));

            var request = (HttpWebRequest)WebRequest.Create(StreamRequestUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                string encoded = HttpUtil.EncodeFormPostData(PostData);
                if (!string.IsNullOrEmpty(encoded))
                    sw.Write(encoded);
            }

            return request;
        }

        protected static NameValueCollection ConstructPostData(StreamingAPIParameters p)
        {
            var postData = new NameValueCollection();

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
}
