//
// Copyright (C) 2011 by Natthawut Kulnirundorn <m3rlinez@gmail.com>

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//


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
    /// <summary>
    /// Specialization of TweetEventSource that handles target URL that requires HTTP GET
    /// </summary>
    public class PostBasedTweetEventSource : StreamingTweetEventSource
    {
        protected override HttpWebRequest CreateWebRequest(StreamingAPIParameters p)
        {
            PostData.Add(ConstructPostData(p));

            var request = (HttpWebRequest)WebRequest.Create(StreamRequestUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            var header = CreateAuthHeader(request);
            request.Headers.Add("Authorization", header.GetHeaderString());

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
                if (p.Count != 0) 
                    postData.Add("count", p.Count.ToString());
                
                if (p.Delimited != 0)
                    postData.Add("delimited", p.Delimited.ToString());

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

    public class UserStreamEventSource : PostBasedTweetEventSource
    {
        protected const string DefaultUserStreamUrl = "https://userstream.twitter.com/2/user.json";

        public UserStreamEventSource()
        {
            StreamRequestUrl = DefaultUserStreamUrl;
        }
    }

    public class FilterStreamEventSource : PostBasedTweetEventSource
    {
        protected const string DefaultFilterStreamUrl = "https://stream.twitter.com/1/statuses/filter.json";

        public FilterStreamEventSource()
        {
            StreamRequestUrl = DefaultFilterStreamUrl;
        }
    }
}
