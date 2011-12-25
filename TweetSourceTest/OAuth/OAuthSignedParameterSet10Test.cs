using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TweetSource.OAuth;
using TweetSource.Util;

namespace TweetSource.Tests.OAuth
{
    [TestFixture]
    class OAuthSignedParameterSet10Test
    {
        [SetUp]
        public void SetUp()
        {
            SetUpGet();
        }

        #region Prepare data for GET Test

        private const string GET_NONCE = "fixedString";
        private const int GET_TIMESTAMP = 100000;

        private HttpParameterSet httpGetParam;
        private OpenedSignedParameterSet10 signedGetRequest;
        private FixedStringGenerator fixedStringGet;
        private FixedClock fixedClockGet;

        private void SetUpGet()
        {
            httpGetParam = new HttpParameterSet()
            {
                ConsumerKey = "consumerKey",
                ConsumerSecret = "consumerSecret",
                Token = "token",
                TokenSecret = "tokenSecret",
                OAuthVersion = "1.0",
                RequestMethod = "GET",
                SignatureMethod = "HMAC-SHA1",
                Url = "http://www.gant.com/test?first=value1&second=value2"
            };

            fixedStringGet = new FixedStringGenerator(GET_NONCE);
            fixedClockGet = new FixedClock(GET_TIMESTAMP);

            signedGetRequest = new OpenedSignedParameterSet10(httpGetParam, fixedStringGet, fixedClockGet);
        }

        // Expected result
        //
        private const string GET_EXPECTED_NORM_PARAM =
            "first=value1&oauth_consumer_key=consumerKey&oauth_nonce=fixedString&oauth_signature_method=HMAC-SHA1&oauth_timestamp=100000&oauth_token=token&oauth_version=1.0&second=value2";

        private const string GET_EXPECTED_BASE_STRING = 
            "GET&http%3A%2F%2Fwww.gant.com%2Ftest&first%3Dvalue1%26oauth_consumer_key%3DconsumerKey%26oauth_nonce%3DfixedString%26oauth_signature_method%3DHMAC-SHA1%26oauth_timestamp%3D100000%26oauth_token%3Dtoken%26oauth_version%3D1.0%26second%3Dvalue2";

        private const string GET_EXPECTED_SIGNATURE = "1597pqb8c9xEm3kkGX/fGc+TbHU=";

        private const string GET_EXPECTED_AUTH_HEADER = 
            "OAuth realm=\"\",oauth_version=\"1.0\",oauth_consumer_key=\"consumerKey\",oauth_token=\"token\",oauth_timestamp=\"100000\",oauth_nonce=\"fixedString\",oauth_signature_method=\"HMAC-SHA1\",oauth_signature=\"1597pqb8c9xEm3kkGX%2FfGc%2BTbHU%3D\"";

        #endregion

        #region Test cases for GET

        [Test]
        public void Get_NonceTest()
        {
            string nonce = signedGetRequest.Nonce;
            Assert.AreEqual(GET_NONCE, nonce, "Expected it to be our fixed string");
        }

        [Test]
        public void Get_TimestampTest()
        {
            string timestamp = signedGetRequest.Timestamp;
            Assert.AreEqual(GET_TIMESTAMP + "", timestamp, "Expected it to be our fixed clock");
        }

        [Test]
        public void Get_SignatureTest()
        {
            Assert.AreEqual(GET_EXPECTED_SIGNATURE, signedGetRequest.Signature,
                "Expected the signature to equal from one we get from Google test page");
        }

        [Test]
        public void Get_GetBaseStringTest()
        {
            Assert.AreEqual(GET_EXPECTED_BASE_STRING, signedGetRequest.GetBaseString(),
                "Expected the signature base string to equal from one we get from Google test page");
        }

        [Test]
        public void Get_GetNormalizedParametersTest()
        {
            Assert.AreEqual(GET_EXPECTED_NORM_PARAM, signedGetRequest.GetNormalizedParameters(),
                "Expected the normalized parameters to equal from one we get from Google test page");
        }

        #endregion

        #region Prepare data for POST test

        #endregion

        #region Test cases for POST

        #endregion

        /// <summary>
        /// Clock that does not really return current time, but fixed time
        /// </summary>
        class FixedClock : Clock
        {
            private int epoch;

            public FixedClock(int epoch)
            {
                this.epoch = epoch;
            }

            public override long EpochTotalSeconds()
            {
                return epoch;
            }
        }

        /// <summary>
        /// Random string class that does not return random string, but a fixed string
        /// </summary>
        class FixedStringGenerator : StringGenerator
        {
            private string fixedString;

            public FixedStringGenerator(string fixedString)
            {
                this.fixedString = fixedString;
            }

            public override string NextRandomString(int length)
            {
                return fixedString;
            }
        }

        /// <summary>
        /// Opened up some protected methods so that we can test them
        /// </summary>
        class OpenedSignedParameterSet10 : OAuthSignedParameterSet10
        {
            public OpenedSignedParameterSet10(HttpParameterSet http, StringGenerator str, Clock clock)
                : base(http, str, clock) { }

            public new string GetBaseString()
            {
                return base.GetBaseString();
            }

            public string GetNormalizedParameters()
            {
                return base.GetNormalizedRequestParameters();
            }
        }
    }
}
