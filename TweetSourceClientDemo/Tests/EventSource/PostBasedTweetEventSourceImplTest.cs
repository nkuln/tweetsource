using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TweetSource.EventSource;

namespace TweetSourceClientDemo.Tests.EventSource
{
    [TestFixture]
    class PostBasedTweetEventSourceImplTest : PostBasedTweetEventSourceImpl
    {
        [Test]
        public void ConstructPostDataTest()
        {
            var result = PostBasedTweetEventSourceImpl.ConstructPostData(
                new StreamingAPIParameters()
                {
                    Track = new string[]{"track1", "track2"},
                    Count = 10
                });

            Assert.AreEqual(2, result.Count, "Should contain two entries");
            Assert.AreEqual("track1,track2", result["track"]);
            Assert.AreEqual("10", result["count"]);
        }
    }
}
