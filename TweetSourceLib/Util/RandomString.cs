//
// Copyright (C) 2011 by Natthawut Kulnirundorn <m3rlinez@gmail.com>

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//


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
