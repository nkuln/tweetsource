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
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TweetSource.Util;

namespace TweetSource.OAuth
{
    /// <summary>
    /// Utility class for encoding 'Authorization' part in HTTP header to access 
    /// resource from URL that requires OAuth.
    /// 
    /// For our case, it is for Streaming API in Twitter. This is based from original source code 
    /// by Gary Short: http://garyshortblog.wordpress.com/2011/02/11/a-twitter-oauth-example-in-c/
    /// </summary>
    public abstract class AuthorizationHeader
    {
        /// <summary>
        /// Create a new AuthorizationHeader based on OAuth keys and some 
        /// HTTP request parameters required to generate correct OAuth signature.
        /// </summary>
        /// <param name="parameters">Parameters REQUIRED by OAuth</param>
        /// <returns>Instance of AuthorizationHeader</returns>
        public static AuthorizationHeader Create(HttpParameterSet parameters)
        {
            switch (parameters.OAuthVersion)
            {
                case "1.0":
                    return new OAuthAuthorizationHeader10(parameters);
                default:
                    string message = string.Format("Version {0} is not supported", parameters.OAuthVersion);
                    throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Header string to be used in 'Authorization' part of HTTPWebRequest's header.
        /// This can be added to request using HttpWebRequest.Headers.Add(..) method.
        /// </summary>
        /// <returns>Header string</returns>
        public abstract string GetHeaderString();
    }

    /// <summary>
    /// Implementation for OAuth version 1.0 of AuthorizationHeader
    /// </summary>
    class OAuthAuthorizationHeader10 : AuthorizationHeader
    {
        protected SignedParameterSet parameters;

        public OAuthAuthorizationHeader10(HttpParameterSet parameters)
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

            Trace.WriteLine("Authorization Header: " + sb.ToString());

            return sb.ToString();
        }

        /// <summary>
        /// Dependency on SignedParamterSet, RandomString, and Clock.
        /// </summary>
        /// <param name="baseParams">Parameters to pass to ctor of SignedParameterSet</param>
        /// <returns></returns>
        protected virtual SignedParameterSet CreateSignedParameterSet(HttpParameterSet baseParams)
        {
            return new OAuthSignedParameterSet10(baseParams, 
                new RandomStringGenerator(), new SystemClock());
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
            OAuthVersion = "1.0";
            SignatureMethod = "HMAC-SHA1";
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Consumer Key : {0}", ConsumerKey);
            sb.AppendLine();
            sb.AppendFormat("Consumer Secret : {0}", ConsumerSecret);
            sb.AppendLine();
            sb.AppendFormat("Token : {0}", Token);
            sb.AppendLine();
            sb.AppendFormat("Token Secret : {0}", TokenSecret);
            sb.AppendLine();
            sb.AppendFormat("OAuth Version : {0}", OAuthVersion);
            sb.AppendLine();
            sb.AppendFormat("Signature Method : {0}", SignatureMethod);
            sb.AppendLine();

            return sb.ToString();
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
