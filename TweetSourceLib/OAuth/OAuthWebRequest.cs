using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization;
using TweetSource.OAuth;
using System.IO;
using System.Collections.Specialized;
using TweetSourceLib.Util;

namespace TweetSource.OAuth
{
    public abstract class OAuthWebRequest
    {
        public abstract WebResponse GetResponse();

        public abstract WebResponse GetResponse(string postData);

        public abstract WebResponse GetResponse(NameValueCollection postData);
    }

    public class OAuthWebRequestImpl : OAuthWebRequest
    {
        protected HttpWebRequest req;
        protected AuthParameterSet config;

        public OAuthWebRequestImpl(HttpWebRequest req, AuthParameterSet config)
        {
            this.req = req;
            this.config = config;
        }

        public override WebResponse GetResponse(NameValueCollection postData)
        {
            var parameters = new HttpParameterSet()
            {
                // From AuthConfig
                ConsumerKey = config.ConsumerKey,
                ConsumerSecret = config.ConsumerSecret,
                Token = config.Token,
                TokenSecret = config.TokenSecret,
                OAuthVersion = config.OAuthVersion,
                SignatureMethod = config.SignatureMethod,

                // Derived from HTTP Web Request
                Url = req.RequestUri.OriginalString,
                PostData = postData,
                RequestMethod = req.Method,
            };

            var header = AuthorizationHeader.Create(parameters);
            req.Headers["Authorization"] = header.GetHeaderString();

            if (req.Method == "POST")
            {
                req.ContentType = "application/x-www-form-urlencoded";
                using (var sw = new StreamWriter(req.GetRequestStream()))
                {
                    string queryString = HttpUtil.NameValueCollectionToQueryString(postData);
                    if (!string.IsNullOrEmpty(queryString))
                        sw.Write(queryString);
                }
            }
            
            return req.GetResponse();
        }

        public override WebResponse GetResponse(string postData)
        {
            var collection = HttpUtil.QueryStringToNameValueCollection(postData);
            return GetResponse(collection);
        }

        public override WebResponse GetResponse()
        {
            return GetResponse("");
        }
    }
}
