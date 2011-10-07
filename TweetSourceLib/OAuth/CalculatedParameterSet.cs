using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

namespace TweetSource.OAuth
{
    public abstract class SignedParameterSet : ParameterSet
    {
        public abstract string Nonce { get; }
        public abstract string Timestamp { get; }
        public abstract string Signature { get; }

        public SignedParameterSet(ParameterSet baseParams)
            : base(baseParams) { }
    }

    public class SignedParameterSet10Impl : SignedParameterSet
    {
        protected readonly DateTime BASE_DATE_TIME = 
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        protected const string NONCE_CHARS = 
            "ABCDEFGHIJKLMNOPQRSTUWXYZabcdefghijklmnopqrstuwxyz";

        protected const int NONCE_LENGTH = 11;

        protected readonly Random random = new Random();

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

        public SignedParameterSet10Impl(ParameterSet baseParams)
            : base(baseParams)
        {
            nonce = GetRandomString(NONCE_LENGTH);
            timeStamp = GetCurrentTimeStampString();
        }

        protected string GetRandomString(int length)
        {
            char[] buff = new char[length];

            for (int i = 0; i < length; ++i)
                buff[i] = NONCE_CHARS[random.Next(NONCE_CHARS.Length)];

            return new string(buff);
        }

        protected string GetCurrentTimeStampString()
        {
            var ts = DateTime.UtcNow - BASE_DATE_TIME;
            long seconds = (long)ts.TotalSeconds;
            return seconds.ToString();
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
                Uri.EscapeDataString(ConsumerSecret),
                Uri.EscapeDataString(TokenSecret));

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
            string absoluteUrl = RemoveQueryString(Url);

            string baseString = string.Format("{0}&{1}&{2}",
                requestMethod,
                Uri.EscapeDataString(absoluteUrl), 
                Uri.EscapeDataString(normalized));

            return baseString;
        }

        protected static string RemoveQueryString(string url)
        {
            int indexCut = url.IndexOf('?');

            if (indexCut < 0) return url;

            return url.Substring(0, indexCut);
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
            var postParams = PostRequestBody.Split(new char[] { '&' }, 
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in postParams)
                paramList.Add(p);
        }

        protected void AddGetParameters(List<string> paramList)
        {
            string queryString = GetQueryString(Url);

            var getParams = queryString.Split(new char[] { '&' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in getParams)
                paramList.Add(p);
        }

        protected static string GetQueryString(string url)
        {
            int indexCut = url.IndexOf('?');

            if (indexCut < 0) return "";

            return url.Substring(indexCut);
        }

    }
}
