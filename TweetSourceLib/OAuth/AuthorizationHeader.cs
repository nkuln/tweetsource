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
            switch (parameters.Version)
            {
                case "1.0":
                    return new AuthorizationHeader10Impl(parameters);
                default:
                    string message = string.Format("Version {0} is not supported", parameters.Version);
                    throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Header string to be used in 'Authorization' part of HTTPWebRequest's header
        /// </summary>
        /// <returns></returns>
        public abstract string GetHeader();

        public class ParameterSet
        {
            public string Url { get; set; }
            public string Version { get; set; }
            public string ConsumerKey { get; set; }
            public string ConsumerSecret { get; set; }
            public string Token { get; set; }
            public string TokenSecret { get; set; }
            public string SignatureMethod { get; set; }

            public ParameterSet() { }

            /// <summary>
            /// A copy constructor
            /// </summary>
            /// <param name="another">Another instance</param>
            public ParameterSet(ParameterSet another)
            {
                Url = another.Url;
                Version = another.Version;
                ConsumerKey = another.ConsumerKey;
                ConsumerSecret = another.ConsumerSecret;
                Token = another.Token;
                TokenSecret = another.TokenSecret;
                SignatureMethod = another.SignatureMethod;
            }
        }
    }

    public class AuthorizationHeader10Impl : AuthorizationHeader
    {
        protected CalculatedParameterSet parameters;

        public AuthorizationHeader10Impl(ParameterSet parameters)
        {
            this.parameters = CreateCalculatedParameterSet(parameters);
        }

        protected virtual CalculatedParameterSet CreateCalculatedParameterSet(ParameterSet baseParams)
        {
            return new CalculatedParameterSet10Impl(baseParams);
        }

        public override string GetHeader()
        {
            var sb = new StringBuilder();

            sb.Append("OAuth ");

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

            sb.AppendFormat("oauth_signature=\"{0}\",",
                Uri.EscapeDataString(parameters.Signature));

            sb.AppendFormat("oauth_version=\"{0}\"",
                Uri.EscapeDataString(parameters.Version));

            return sb.ToString();
        }
    }
}
