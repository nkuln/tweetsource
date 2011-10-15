using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSource.EventSource;
using NUnit.Framework;

namespace TweetSourceClientDemo.Tests.EventSource
{
    [TestFixture]
    class GetBasedTweetEventSourceImplTest : GetBasedTweetEventSourceImpl
    {
        [Test]
        public void ConstructUrlWithQueryStringTest()
        {
            string result = GetBasedTweetEventSourceImpl.ConstructUrlWithQueryString("http://www.test.com/test",
                new StreamingAPIParameters()
                {
                    Count = 10,
                    Delimited = 100
                });

            string expected = "http://www.test.com/test?delimited=100&count=10";

            Assert.AreEqual(expected, result, "Expected to get correct URL with query string");
        }
    }
}
