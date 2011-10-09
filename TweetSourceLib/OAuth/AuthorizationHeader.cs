using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TweetSource.Util;

namespace TweetSource.OAuth
{
    /// <summary>
    /// Utility class for encoding 'Authorization' part in HTTP header to access 
    /// resource from URL that requires OAuth, e.g. Streaming API in Twitter.
    /// 
    /// This is based from original source code by Gary Short:
    /// http://garyshortblog.wordpress.com/2011/02/11/a-twitter-oauth-example-in-c/
    /// </summary>
    public abstract class AuthorizationHeader
    {
        /// <summary>
        /// Factory method for creating an AuthorizationHeader
        /// </summary>
        /// <param name="parameters">Parameters REQUIRED by OAuth</param>
        /// <returns>Instance of AuthorizationHeader</returns>
        public static AuthorizationHeader Create(HttpParameterSet parameters)
        {
            switch (parameters.OAuthVersion)
            {
                case "1.0":
                    return new AuthorizationHeader10Impl(parameters);
                case "1.0a":
                    return new AuthorizationHeader10Impl(parameters);
                default:
                    string message = string.Format("Version {0} is not supported", parameters.OAuthVersion);
                    throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Header string to be used in 'Authorization' part of HTTPWebRequest's header
        /// </summary>
        /// <returns></returns>
        public abstract string GetHeaderString();
    }

    public class AuthorizationHeader10Impl : AuthorizationHeader
    {
        protected SignedParameterSet parameters;

        public AuthorizationHeader10Impl(HttpParameterSet parameters)
        {
            this.parameters = CreateSignedParameterSet(parameters);
        }

        public override string GetHeaderString()
        {
            var sb = new StringBuilder();

            sb.Append("OAuth realm=\"\",");

            sb.AppendFormat("oauth_signature=\"{0}\",",
                HttpUtil.Esc(parameters.Signature));

            sb.AppendFormat("oauth_nonce=\"{0}\",",
                HttpUtil.Esc(parameters.Nonce));

            sb.AppendFormat("oauth_signature_method=\"{0}\",",
                HttpUtil.Esc(parameters.SignatureMethod));

            sb.AppendFormat("oauth_timestamp=\"{0}\",",
                HttpUtil.Esc(parameters.Timestamp));

            sb.AppendFormat("oauth_consumer_key=\"{0}\",",
                HttpUtil.Esc(parameters.ConsumerKey));

            sb.AppendFormat("oauth_token=\"{0}\",",
                HttpUtil.Esc(parameters.Token));

            sb.AppendFormat("oauth_version=\"{0}\"",
                HttpUtil.Esc(parameters.OAuthVersion));

            Debug.WriteLine("Authorization Header: " + sb.ToString());

            return sb.ToString();
        }

        protected virtual SignedParameterSet CreateSignedParameterSet(HttpParameterSet baseParams)
        {
            return new SignedParameterSet10Impl(baseParams, 
                new RandomStringImpl(), new ClockImpl());
        }
    }

    public class AuthParameterSet
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string OAuthVersion { get; set; }
        public string SignatureMethod { get; set; }

        public AuthParameterSet()
        {
            SetDefaultValue();
        }

        private void SetDefaultValue()
        {
            OAuthVersion = "1.0a";
            SignatureMethod = "HMAC-SHA1";
        }
    }

    public class HttpParameterSet : AuthParameterSet
    {
        public string Url { get; set; }
        public string RequestMethod { get; set; }
        public NameValueCollection PostData { get; set; }

        public HttpParameterSet()
        {
            SetDefaultValue();
        }

        /// <summary>
        /// A copy constructor
        /// </summary>
        /// <param name="another">Another instance</param>
        public HttpParameterSet(HttpParameterSet another)
        {
            SetDefaultValue();

            Url = another.Url;
            RequestMethod = another.RequestMethod;
            PostData = another.PostData;
            OAuthVersion = another.OAuthVersion;
            ConsumerKey = another.ConsumerKey;
            ConsumerSecret = another.ConsumerSecret;
            Token = another.Token;
            TokenSecret = another.TokenSecret;
            SignatureMethod = another.SignatureMethod;
        }

        private void SetDefaultValue()
        {
            Url = "";
            RequestMethod = "";
            PostData = new NameValueCollection();
            OAuthVersion = "";
            ConsumerKey = "";
            ConsumerSecret = "";
            Token = "";
            TokenSecret = "";
            SignatureMethod = "";
        }

    }
}
