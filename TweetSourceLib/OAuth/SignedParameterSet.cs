using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TweetSource.Util;

namespace TweetSource.OAuth
{
    public abstract class SignedParameterSet : HttpParameterSet
    {
        public abstract string Nonce { get; }
        public abstract string Timestamp { get; }
        public abstract string Signature { get; }

        public SignedParameterSet(HttpParameterSet baseParams)
            : base(baseParams) { }
    }

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
            Debug.WriteLine("Base string: " + baseString);

            // Create the key based on secrets
            string key = string.Format("{0}&{1}",
                HttpUtil.Esc(ConsumerSecret),
                HttpUtil.Esc(TokenSecret));

            // Create our hash generator with key
            var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(key));

            // Generate hashes and create signature string
            byte[] hashes = hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString));
            string signature = Convert.ToBase64String(hashes);
            Debug.WriteLine("Signature string: " + signature);

            return signature;
        }

        protected string GetBaseString()
        {
            string requestMethod = RequestMethod.ToUpper();
            string normalized = GetNormalizedRequestParameters();
            string absoluteUrl = HttpUtil.RemoveQueryString(Url);

            string baseString = string.Format("{0}&{1}&{2}",
                requestMethod,
                HttpUtil.Esc(absoluteUrl),
                HttpUtil.Esc(normalized));

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
