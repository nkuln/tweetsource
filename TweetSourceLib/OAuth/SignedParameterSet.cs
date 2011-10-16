//
// Copyright (C) 2011 by Natthawut Kulnirundorn

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
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TweetSource.Util;

namespace TweetSource.OAuth
{
    /// <summary>
    /// This facilitates the calculation of OAuth Signature and generation 
    /// of Nonce and Timestamp.
    /// </summary>
    public abstract class SignedParameterSet : HttpParameterSet
    {
        /// <summary>
        /// A random string that must be unique for each request of the same timestamp
        /// </summary>
        public abstract string Nonce { get; }
        
        /// <summary>
        /// Number of seconds since 1 January 1970
        /// </summary>
        public abstract string Timestamp { get; }

        /// <summary>
        /// OAuth Signature string calculated by method described in OAuth spec
        /// </summary>
        public abstract string Signature { get; }

        public SignedParameterSet(HttpParameterSet baseParams)
            : base(baseParams) { }
    }

    /// <summary>
    /// Implementation of SignedParameterSet for OAuth 1.0
    /// </summary>
    public class SignedParameterSet10Impl : SignedParameterSet
    {
        protected const int NONCE_LENGTH = 11;

        protected readonly RandomString random;

        protected readonly Clock clock;

        protected readonly string nonce;

        public override string Nonce
        {
            get { return nonce; }
        }

        protected readonly string timeStamp;

        public override string Timestamp
        {
            get { return timeStamp; }
        }

        public SignedParameterSet10Impl(HttpParameterSet baseParams, 
            RandomString random, Clock clock)
            : base(baseParams)
        {
            this.random = random;
            this.clock = clock;
            this.nonce = random.NextRandomString(NONCE_LENGTH);
            this.timeStamp = GetCurrentTimeStampString();
        }

        protected string GetCurrentTimeStampString()
        {
            return clock.EpochTotalSeconds().ToString();
        }

        public override string Signature
        {
            get
            {
                switch (SignatureMethod)
                {
                    case "HMAC-SHA1":
                        return GetSignatureHMACSHA1();
                    default:
                        throw new ApplicationException(string.Format(
                            "Signature method {0} is not supported", SignatureMethod));
                }
            }
        }

        private string GetSignatureHMACSHA1()
        {
            // Construct Base String
            string baseString = GetBaseString();
            Trace.WriteLine("Base string: " + baseString);

            // Create the key based on secrets
            string key = string.Format("{0}&{1}",
                HttpUtil.Esc(ConsumerSecret),
                HttpUtil.Esc(TokenSecret));

            // Create our hash generator with key
            var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(key));

            // Generate hashes and create signature string
            byte[] hashes = hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString));
            string signature = Convert.ToBase64String(hashes);
            Trace.WriteLine("Signature string: " + signature);

            return signature;
        }

        protected string GetBaseString()
        {
            string requestMethod = RequestMethod.ToUpper();
            string normalized = GetNormalizedRequestParameters();
            string absoluteUrl = HttpUtil.RemoveQueryString(Url);

            string baseString = string.Format("{0}&{1}&{2}",
                requestMethod, HttpUtil.Esc(absoluteUrl), HttpUtil.Esc(normalized));

            return baseString;
        }

        protected string GetNormalizedRequestParameters()
        {
            var paramList = new List<string>();

            // Collect parameters from various sources
            AddOAuthParameters(paramList);
            AddPostParameters(paramList);
            AddGetParameters(paramList);

            // Sort lexicographical order
            paramList.Sort((x, y) => string.Compare(x, y));

            // Concat
            return string.Join("&", paramList.ToArray());
        }

        protected void AddOAuthParameters(List<string> paramList)
        {
            paramList.Add("oauth_version=" + OAuthVersion);
            paramList.Add("oauth_consumer_key=" + ConsumerKey);
            paramList.Add("oauth_nonce=" + Nonce);
            paramList.Add("oauth_signature_method=" + SignatureMethod);
            paramList.Add("oauth_timestamp=" + Timestamp);
            paramList.Add("oauth_token=" + Token);
        }

        protected void AddPostParameters(List<string> paramList)
        {
            foreach (string key in PostData)
            {
                paramList.Add(string.Format("{0}={1}",
                    HttpUtil.Esc(key), HttpUtil.Esc(PostData[key])));
            }
        }

        protected void AddGetParameters(List<string> paramList)
        {
            string queryString = HttpUtil.GetQueryString(Url);

            var getParams = queryString.Split(new char[] { '&' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in getParams)
                paramList.Add(p);
        }
    }
}
