using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetSourceLib.Util
{
    public abstract class Clock
    {
        protected readonly DateTime BASE_DATE_TIME =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public abstract long EpochTotalSeconds();
    }

    public class ClockImpl : Clock
    {
        public override long EpochTotalSeconds()
        {
            var ts = DateTime.UtcNow - BASE_DATE_TIME;
            return (long)ts.TotalSeconds;
        }
    }
}
