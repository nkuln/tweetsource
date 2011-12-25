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
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace TweetSource.Util
{
    /// <summary>
    /// Utility class that takes care of most HTTP encoding/decoding things
    /// </summary>
    public class HttpUtil
    {
        /// <summary>
        /// The set of characters that are unreserved in RFC 2396 but are NOT unreserved in RFC 3986.
        /// </summary>
        private static readonly string[] UriRfc3986CharsToEscape = new[] { "!", "*", "'", "(", ")" };

        /// <summary>
        /// Extract query string from a URL
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>query string</returns>
        public static string GetQueryString(string url)
        {
            int indexCut = url.IndexOf('?');

            if (indexCut < 0 || indexCut == url.Length) 
                return "";
            else
                return url.Substring(indexCut + 1);
        }

        /// <summary>
        /// Remove query string from a URL
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>URL without query string</returns>
        public static string RemoveQueryString(string url)
        {
            int indexCut = url.IndexOf('?');

            if (indexCut < 0) return url;

            return url.Substring(0, indexCut);
        }

        /// <summary>
        /// Note: This is not the same as the way we encode query string
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static string EncodeFormPostData(NameValueCollection postData)
        {
            var list = new List<string>();

            foreach (string key in postData.AllKeys)
                list.Add(string.Format("{0}={1}", Esc(key), Esc(postData[key])));

            return string.Join("&", list.ToArray());
        }

        /// <summary>
        /// Parse query string to NameValueCollection
        /// </summary>
        /// <param name="query">query string to parse</param>
        /// <returns>parsed data in NameValueCollection</returns>
        public static NameValueCollection QueryStringToNameValueCollection(string query)
        {
            var collection = new NameValueCollection();

            var pairs = query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                int indexEqual = pair.IndexOf('=');
                if (indexEqual >= 0 && indexEqual != pair.Length)
                {
                    string name = pair.Substring(0, indexEqual);
                    if (!string.IsNullOrEmpty(name)) name = Uri.UnescapeDataString(name);

                    string value = pair.Substring(indexEqual + 1);
                    if (!string.IsNullOrEmpty(value)) value = Uri.UnescapeDataString(value);
                    
                    collection.Add(name, value);
                }
                else
                {
                    collection.Add(pair, "");
                }
            }

            return collection;
        }

        /// <summary>
        /// Escape string according to RFC 3986 - 
        /// http://blog.nerdbank.net/2009/05/uriescapedatapath-and.html
        /// </summary>
        /// <param name="s">String to escape</param>
        /// <returns>Escaped string</returns>
        public static string Esc(string s)
        {
            return EscapeUriDataStringRfc3986(s);
        }

        /// <summary>
        /// Escapes a string according to the URI data string rules given in RFC 3986.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        /// <remarks>
        /// The <see cref="Uri.EscapeDataString"/> method is <i>supposed</i> to take on
        /// RFC 3986 behavior if certain elements are present in a .config file.  Even if this
        /// actually worked (which in my experiments it <i>doesn't</i>), we can't rely on every
        /// host actually having this configuration element present.
        /// </remarks>
        public static string EscapeUriDataStringRfc3986(string value)
        {
            // Start with RFC 2396 escaping by calling the .NET method to do the work.
            // This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
            // If it does, the escaping we do that follows it will be a no-op since the
            // characters we search for to replace can't possibly exist in the string.
            StringBuilder escaped = new StringBuilder(Uri.EscapeDataString(value));

            // Upgrade the escaping to RFC 3986, if necessary.
            for (int i = 0; i < UriRfc3986CharsToEscape.Length; i++)
            {
                escaped.Replace(UriRfc3986CharsToEscape[i], Uri.HexEscape(UriRfc3986CharsToEscape[i][0]));
            }

            // Return the fully-RFC3986-escaped string.
            return escaped.ToString();
        }
    }
}
