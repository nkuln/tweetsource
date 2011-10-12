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

        [Test]
        public void GetQueryStringBasicTest()
        {
            string test = "https://mail.google.com/mail/?shva=1#inbox";
            string expected = "shva=1#inbox";

            Assert.AreEqual(expected, HttpUtil.GetQueryString(test), 
                "Should get query string " + expected);
        }

        [Test]
        public void GetQueryStringMissingTest()
        {
            string test = "https://mail.google.com/mail/";

            Assert.AreEqual(string.Empty, HttpUtil.GetQueryString(test),
                "Should get empty string");
        }

        [Test]
        public void GetQueryStringNoDataTest()
        {
            string test = "https://mail.google.com/mail/?";

            Assert.AreEqual(string.Empty, HttpUtil.GetQueryString(test),
                "Should get empty string");
        }

        [Test]
        public void QueryStringToNameValueCollectionBasicTest()
        {
            string test = "id=1234&name=Gant%20Natthawut&status=%22%40m3rlinez%20is%20bored%22&key%20with%20space=value";

            var result = HttpUtil.QueryStringToNameValueCollection(test);
            Assert.AreEqual(4, result.Count, "Should parsed 4 pairs");
            Assert.AreEqual("1234", result["id"], "Should get 1234 as 'id'");
            Assert.AreEqual("Gant Natthawut", result["name"], "Should get 'Gant Natthawut' as value for 'name'");
            Assert.AreEqual("\"@m3rlinez is bored\"", result["status"], "Should get \"@m3rlinez is bored\" as 'status'");
            Assert.AreEqual("value", result["key with space"], "Should get 'value' for this 'key with space'");
        }

        [Test]
        public void QueryStringToNameValueCollectionBlankDataTest()
        {
            string test = "id=&name=&status=&key%20with%20space=";

            var result = HttpUtil.QueryStringToNameValueCollection(test);
            Assert.AreEqual(4, result.Count, "Should parsed 4 pairs");
            Assert.AreEqual(string.Empty, result["id"], "Should get 1234 as 'id'");
            Assert.AreEqual(string.Empty, result["name"], "Should get 'Gant Natthawut' as value for 'name'");
            Assert.AreEqual(string.Empty, result["status"], "Should get \"@m3rlinez is bored\" as 'status'");
            Assert.AreEqual(string.Empty, result["key with space"], "Should get 'value' for this 'key with space'");
        }

        [Test]
        public void QueryStringToNameValueCollectionNoDataTest()
        {
            string test = "id&name&status&key%20with%20space";

            var result = HttpUtil.QueryStringToNameValueCollection(test);
            Assert.AreEqual(0, result.Count, "Should parsed no pair");
        }

        [Test]
        public void RemoveQueryStringBasicTest()
        {
            Assert.AreEqual("http://www.solidskill.net/get",
                HttpUtil.RemoveQueryString("http://www.solidskill.net/get?junkgarbagejunk"),
                "Should removed query string");
        }

        [Test]
        public void RemoveQueryStringNoStringTest()
        {
            Assert.AreEqual("http://www.solidskill.net/get",
                HttpUtil.RemoveQueryString("http://www.solidskill.net/get"),
                "Should removed query string");
        }

        [Test]
        public void RemoveQueryStringMultipleQuestionTest()
        {
            Assert.AreEqual("http://www.solidskill.net/get",
                HttpUtil.RemoveQueryString("http://www.solidskill.net/get??????junkgarbagejunk"),
                "Should removed query string");
        }

    }
}
