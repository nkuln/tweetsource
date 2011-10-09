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
    public abstract class TweetEventSource : EventSourceBaseImpl<TweetEventArgs>
    {
        public string StreamRequestUrl { get; set; }

        public abstract AuthParameterSet AuthConfig { get; }

        public abstract NameValueCollection PostData { get; }

        public abstract void Start(StreamingAPIParameters p = null);

        public abstract void Stop();

        #region Factory for creating each type of stream

        public static TweetEventSource CreateFilterStream()
        {
            return new FilterStreamEventSourceImpl();
        }

        public static TweetEventSource CreateRetweetStream()
        {
            return new RetweetStreamEventSourceImpl();
        }

        public static TweetEventSource CreateLinkStream()
        {
            return new LinkStreamEventSourceInmpl();
        }

        public static TweetEventSource CreateSampleStream()
        {
            return new SampleStreamEventSourceImpl();
        }

        public static TweetEventSource CreateUserStrean()
        {
            return new UserStreamEventSourceImpl();
        }

        #endregion
    }

    public abstract class TweetEventSourceBaseImpl : TweetEventSource
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

        public TweetEventSourceBaseImpl()
        {
            this.config = new AuthParameterSet();
            this.postData = new NameValueCollection();
        }

        public sealed override void Start(StreamingAPIParameters p = null)
        {
            try
            {
                this.request = CreateWebRequest(p);
                AddAuthHeaderToRequest();

                StartThread();
            }
            catch (WebException wex)
            {
                throw new ApplicationException("Could not start: " + wex.Message, wex);
            }
        }

        private void AddAuthHeaderToRequest()
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
                Url = this.request.RequestUri.OriginalString,
                RequestMethod = this.request.Method,
                PostData = this.postData,
            };

            var header = AuthorizationHeader.Create(parameters);
            this.request.Headers["Authorization"] = header.GetHeaderString();
        }

        private void StartThread()
        {
            if (this.requestThread != null)
            {
                throw new ApplicationException(
                    "Cannot start user stream because another subscription " +
                    "is still running. Call StopAll() first");
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
                requestThread = null;
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
                    Debug.WriteLine(val);

                    EnqueueEvent(new TweetEventArgs()
                    {
                        JsonText = val,
                        InfoText = "Got new data"
                    });
                }
            }
        }

        public sealed override void Stop()
        {
            this.requestThread.Interrupt();
        }

        protected abstract HttpWebRequest CreateWebRequest(StreamingAPIParameters p);
    }

    public class StreamingAPIParameters
    {
        public int Count { get; set; }
        public int Delimited { get; set; }
        public int[] Follow { get; set; }
        public string[] Track { get; set; }
        public double[] Locations { get; set; }

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

    public class TweetEventArgs : EventArgs
    {
        public string JsonText { get; set; }
        public string InfoText { get; set; }
    }

}
