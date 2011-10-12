using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using TweetSource.OAuth;
using TweetSource.Util;

namespace TweetSourceClientDemo.Tests.OAuth
{
    [TestFixture]
    class SignedParameterSet10ImplTest
    {

        class FixedClockImpl : Clock
        {
            public override long EpochTotalSeconds()
            {
                return 100;
            }
        }

        class FixedStringImpl : RandomString
        {
            public override string NextRandomString(int length)
            {
                return "ThisIsNotRandomString";
            }
        }
    }
}
