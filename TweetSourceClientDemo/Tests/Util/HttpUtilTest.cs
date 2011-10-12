using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TweetSource.Util;
using System.Collections.Specialized;

namespace TweetSourceClientDemo.Tests.Util
{
    [TestFixture]
    class HttpUtilTest
    {
        [Test]
        public void EncodeFormPostDataWithSpacesTest()
        {
            var pairs = new NameValueCollection();
            pairs.Add("test1", "value1");
            pairs.Add("test with spaces", "value with spaces");
            string expected = "test1=value1&test%20with%20spaces=value%20with%20spaces";

            string result = HttpUtil.EncodeFormPostData(pairs);

            Assert.AreEqual(expected, result, "Should encoded form data to " + expected);
        }

        [Test]
        public void EncodeFormPostDataWithNoKeyTest()
        {
            var pairs = new NameValueCollection();

            string result = HttpUtil.EncodeFormPostData(pairs);

            Assert.AreEqual(string.Empty, result, "Should encoded no data");
        }

        [Test]
        public void EncodeFormPostDataWithSpecialCharsTest()
        {
            var pairs = new NameValueCollection();
            pairs.Add("status", "i'm Tom's \"cat\"..");
            pairs.Add("special", "!@#$");
            string expected = "status=i%27m%20Tom%27s%20%22cat%22..&special=%21%40%23%24";

            string result = HttpUtil.EncodeFormPostData(pairs);

            Assert.AreEqual(expected, result, "Should encoded as " + expected);
        }
    }
}
