using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TweetSourceLib.Util
{
    public abstract class RandomString
    {
        protected const string CHARS =
            "ABCDEFGHIJKLMNOPQRSTUWXYZabcdefghijklmnopqrstuwxyz";

        public abstract string NextRandomString(int length);
    }

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
