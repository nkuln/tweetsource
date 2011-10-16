using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetSource.Util
{
    /// <summary>
    /// Generate random string
    /// </summary>
    public abstract class RandomString
    {
        protected const string CHARS =
            "ABCDEFGHIJKLMNOPQRSTUWXYZabcdefghijklmnopqrstuwxyz";

        /// <summary>
        /// Get next random string at desired length
        /// </summary>
        /// <param name="length">length</param>
        /// <returns>random string</returns>
        public abstract string NextRandomString(int length);
    }

    /// <summary>
    /// Implementation based on .NET's Random
    /// </summary>
    public class RandomStringImpl : RandomString
    {
        protected readonly Random random = new Random();

        public override string NextRandomString(int length)
        {
            char[] buff = new char[length];

            for (int i = 0; i < length; ++i)
                buff[i] = CHARS[random.Next(CHARS.Length)];

            return new string(buff);
        }
    }
}
