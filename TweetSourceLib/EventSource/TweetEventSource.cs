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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using TweetSource.OAuth;
using TweetSource.Util;

namespace TweetSource.EventSource
{
    /// <summary>
    /// Event source that provides Tweets
    /// </summary>
    public abstract class TweetEventSource : QueueBasedEventSource<TweetEventArgs>
    {
        /// <summary>
        /// Target URL for the TweetEventSource
        /// </summary>
        public string StreamRequestUrl { get; set; }

        /// <summary>
        /// Required config parameters, i.e. OAuth keys, version
        /// </summary>
        public abstract AuthParameterSet AuthConfig { get; }

        /// <summary>
        /// Posting parameter to used in HTTP POST
        /// </summary>
        public abstract NameValueCollection PostData { get; }

        /// <summary>
        /// This starts the internal thread that pulls tweets from Twitter via Streaming API.
        /// The tweets are put into internal queue. User has to call Dispatch() in order to fire 
        /// EventReceived event that let him/her process tweets.
        /// </summary>
        /// <param name="p"></param>
        public abstract void Start(StreamingAPIParameters p = null);

        /// <summary>
        /// Stop this EventSource. This will make the event source inactive (Active returns false.)
        /// The internal thread will be exited.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Cleanup and make the instance ready for next Start()
        /// </summary>
        public abstract void Cleanup();

        #region Factory for creating each type of stream

        /// <summary>
        /// Create TweetEventSource for Filter Stream
        /// </summary>
        /// <returns>Event source</returns>
        public static TweetEventSource CreateFilterStream()
        {
            return new FilterStreamEventSource();
        }

        /// <summary>
        /// Create TweetEventSource for Retweet Stream
        /// </summary>
        /// <returns></returns>
        public static TweetEventSource CreateRetweetStream()
        {
            return new RetweetStreamEventSource();
        }

        /// <summary>
        /// Create TweetEventSource for Link Stream
        /// </summary>
        /// <returns></returns>
        public static TweetEventSource CreateLinkStream()
        {
            return new LinkStreamEventSource();
        }

        /// <summary>
        /// Create TweetEventSource for Sample Stream
        /// </summary>
        /// <returns></returns>
        public static TweetEventSource CreateSampleStream()
        {
            return new SampleStreamEventSource();
        }

        /// <summary>
        /// Create TweetEventSource for User Stream
        /// </summary>
        /// <returns></returns>
        public static TweetEventSource CreateUserStrean()
        {
            return new UserStreamEventSource();
        }

        #endregion
    }

    public abstract class StreamingTweetEventSource : TweetEventSource
    {
        protected Thread requestThread;

        protected AuthParameterSet config;
        public override AuthParameterSet AuthConfig
        {
            get { return this.config; }
        }

        protected NameValueCollection postData;
        public override NameValueCollection PostData
        {
            get { return this.postData; }
        }

        protected HttpWebRequest request;

        public StreamingTweetEventSource()
        {
            this.config = new AuthParameterSet();
            this.postData = new NameValueCollection();
        }

        public sealed override void Start(StreamingAPIParameters p = null)
        {
            try
            {
                this.request = CreateWebRequest(p);
                StartThread();
            }
            catch (WebException wex)
            {
                throw new ApplicationException("Could not start: " + wex.Message, wex);
            }
        }

        private void StartThread()
        {
            if (this.requestThread != null)
            {
                throw new ApplicationException(
                    "Cannot start new stream because another stream " +
                    "is still active. Call Stop() first");
            }

            this.requestThread = new Thread(new ThreadStart(RunThead));
            this.requestThread.Start();
        }

        private void RunThead()
        {
            try
            {
                RequestData();
            }
            catch (ThreadInterruptedException)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonText = "",
                    InfoText = "Connection down: Request thread ended due to interrupt"
                });
            }
            catch (WebException wex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonText = "",
                    InfoText = "Connection down (web exception): " + wex.ToString()
                });
            }
            catch (ApplicationException aex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonText = "",
                    InfoText = "Connection down: " + aex.ToString()
                });
            }
            catch (Exception ex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonText = "",
                    InfoText = "Connection down (unhandled exception): " + ex.ToString()
                });
            }
            finally
            {
                Cleanup();
            }
        }

        private void RequestData()
        {
            var response = this.request.GetResponse();

            FireSourceUp(new TweetEventArgs()
            {
                JsonText = string.Empty,
                InfoText = "Connection established"
            });

            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                string val;
                while ((val = sr.ReadLine()) != null)
                {
                    Debug.WriteLine("Read from stream: " + val);

                    EnqueueEvent(new TweetEventArgs()
                    {
                        JsonText = val,
                        InfoText = "Got new data"
                    });
                }
            }
        }

        public override bool Active
        {
            get { return requestThread != null; }
        }

        public sealed override void Stop()
        {
            if (requestThread != null)
            {
                this.requestThread.Interrupt();
            }

            Cleanup();
        }

        public override void Cleanup()
        {
            this.requestThread = null;
            this.postData.Clear();
        }

        protected AuthorizationHeader CreateAuthHeader(HttpWebRequest request)
        {
            var parameters = new HttpParameterSet()
            {
                // From AuthConfig
                ConsumerKey = config.ConsumerKey,
                ConsumerSecret = config.ConsumerSecret,
                Token = config.Token,
                TokenSecret = config.TokenSecret,
                OAuthVersion = config.OAuthVersion,
                SignatureMethod = config.SignatureMethod,

                // Derived from HTTP Web Request
                Url = request.RequestUri.OriginalString,
                RequestMethod = request.Method,
                PostData = postData,
            };

            return AuthorizationHeader.Create(parameters);
        }

        protected abstract HttpWebRequest CreateWebRequest(StreamingAPIParameters p);
    }

    /// <summary>
    /// Common parameters for Twitter's Streaming API
    /// </summary>
    public class StreamingAPIParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public int Count { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int Delimited { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int[] Follow { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string[] Track { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public double[] Locations { get; set; }

        /// <summary>
        /// Default constructor. Create with default values.
        /// </summary>
        public StreamingAPIParameters()
        {
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            Count = 0;
            Delimited = 0;
            Follow = new int[] { };
            Track = new string[] { };
            Locations = new double[] { };
        }
    }

    /// <summary>
    /// Tweet event containing JsonText that can be parsed by Json processor.
    /// </summary>
    public class TweetEventArgs : EventArgs
    {
        public string JsonText { get; set; }
        public string InfoText { get; set; }
    }

}
