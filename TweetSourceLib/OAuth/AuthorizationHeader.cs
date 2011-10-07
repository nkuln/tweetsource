using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics;

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
        public static AuthorizationHeader Create(ParameterSet parameters)
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

    public class ParameterSet
    {
        public string Url { get; set; }
        public string RequestMethod { get; set; }
        public string PostRequestBody { get; set; }
        public string OAuthVersion { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public string SignatureMethod { get; set; }

        public ParameterSet() 
        { 
            SetDefaultValue(); 
        }

        /// <summary>
        /// A copy constructor
        /// </summary>
        /// <param name="another">Another instance</param>
        public ParameterSet(ParameterSet another)
        {
            SetDefaultValue();

            Url = another.Url;
            RequestMethod = another.RequestMethod;
            PostRequestBody = another.PostRequestBody;
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
            PostRequestBody = "";
            OAuthVersion = "";
            ConsumerKey = "";
            ConsumerSecret = "";
            Token = "";
            TokenSecret = "";
            SignatureMethod = "";
        }

    }

    public class AuthorizationHeader10Impl : AuthorizationHeader
    {
        protected SignedParameterSet parameters;

        public AuthorizationHeader10Impl(ParameterSet parameters)
        {
            this.parameters = CreateSignedParameterSet(parameters);
        }

        public override string GetHeaderString()
        {
            var sb = new StringBuilder();

            sb.Append("OAuth realm=\"\",");

            sb.AppendFormat("oauth_signature=\"{0}\",",
                Uri.EscapeDataString(parameters.Signature));

            sb.AppendFormat("oauth_nonce=\"{0}\",",
                Uri.EscapeDataString(parameters.Nonce));

            sb.AppendFormat("oauth_signature_method=\"{0}\",",
                Uri.EscapeDataString(parameters.SignatureMethod));

            sb.AppendFormat("oauth_timestamp=\"{0}\",",
                Uri.EscapeDataString(parameters.Timestamp));

            sb.AppendFormat("oauth_consumer_key=\"{0}\",",
                Uri.EscapeDataString(parameters.ConsumerKey));

            sb.AppendFormat("oauth_token=\"{0}\",",
                Uri.EscapeDataString(parameters.Token));

            sb.AppendFormat("oauth_version=\"{0}\"",
                Uri.EscapeDataString(parameters.OAuthVersion));

            Debug.WriteLine("Authorization Header: " + sb.ToString());

            return sb.ToString();
        }

        protected virtual SignedParameterSet CreateSignedParameterSet(ParameterSet baseParams)
        {
            return new SignedParameterSet10Impl(baseParams);
        }
    }
}
