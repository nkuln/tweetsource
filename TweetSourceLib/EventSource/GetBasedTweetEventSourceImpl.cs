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
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace TweetSource.EventSource
{
    /// <summary>
    /// Specialization of TweetEventSource that handles target URL that requires HTTP GET
    /// </summary>
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

            return url + (nv.Count == 0 ? string.Empty : "?" + nv.ToString());

        }

        protected override HttpWebRequest CreateWebRequest(StreamingAPIParameters p)
        {
            string url = ConstructUrlWithQueryString(StreamRequestUrl, p);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            var header = CreateAuthHeader(request);
            request.Headers.Add("Authorization", header.GetHeaderString());

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
