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

            // Order of parameter doesn't really matter ..
            string expected1 = "http://www.test.com/test?delimited=100&count=10";
            string expected2 = "http://www.test.com/test?count=10&delimited=100";

            Assert.True(result == expected1 || result == expected2,
                string.Format("Got '{0}'. Expected to get correct URL with query string: '{1}' or '{2}'",
                result, expected1, expected2));
        }
    }
}
