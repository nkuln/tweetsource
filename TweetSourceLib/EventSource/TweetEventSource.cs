using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Diagnostics;
using TweetSource.OAuth;

namespace TweetSource.EventSource
{
    public abstract class TweetEventSource : EventSource<TweetEventArgs>
    {
        public string UserStreamUrl { get; set; }

        protected const string DefaultUserStreamUrl = "https://userstream.twitter.com/2/user.json";

        public static TweetEventSource Create()
        {
            return new TweetEventSourceImpl();
        }

        public abstract void StartUserStream(string consumerKey, string consumerSecret,
            string accessToken, string accessTokenSecret);

        public abstract void StopAll();
    }

    public class TweetEventArgs : EventArgs
    {
        public string JsonData { get; set; }
        public string InfoText { get; set; }
    }

    public class TweetEventSourceImpl : TweetEventSource
    {
        protected AuthorizationHeader.ParameterSet parameters;
        protected Thread requestThread;

        public TweetEventSourceImpl()
        {
            UserStreamUrl = DefaultUserStreamUrl;
        }

        public override void StartUserStream(string consumerKey, string consumerSecret,
            string accessToken, string accessTokenSecret)
        {
            parameters = new AuthorizationHeader.ParameterSet()
            {
                Url = UserStreamUrl,
                Version = "1.0",
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                Token = accessToken,
                TokenSecret = accessTokenSecret,
                SignatureMethod = "HMAC-SHA1"
            };

            if (requestThread != null)
            {
                throw new ApplicationException(
                    "Cannot start user stream because another subscription is still running. Call StopAll() first");
            }

            requestThread = new Thread(new ThreadStart(RunThead));
            requestThread.Start();
        }

        public void RunThead()
        {
            try
            {
                RequestData();
            }
            catch (WebException wex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonData = "",
                    InfoText = "Connection down (web exception): " + wex.ToString()
                });
            }
            catch (ApplicationException aex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonData = "",
                    InfoText = "Connection down: " + aex.ToString()
                });
            }
            catch (Exception ex)
            {
                FireSourceDown(new TweetEventArgs()
                {
                    JsonData = "",
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
            var header = CreateAuthorizationHeader(parameters);
            string authHeaderString = header.GetHeader();

            var req = (HttpWebRequest)WebRequest.Create(UserStreamUrl);
            req.Headers["Authorization"] = authHeaderString;
            req.Method = "POST";

            var resp = req.GetResponse();
            FireSourceUp(new TweetEventArgs()
            {
                JsonData = "",
                InfoText = "Connection established"
            });

            using (StreamReader sr = new StreamReader(resp.GetResponseStream()))
            {
                string val;
                while ((val = sr.ReadLine()) != null)
                {
                    Debug.WriteLine(val);
                    FireDataArrived(new TweetEventArgs()
                    {
                        JsonData = val,
                        InfoText = "Got new data"
                    });
                }
            }
        }

        public override void StopAll()
        {
            throw new NotImplementedException();
        }

        protected virtual AuthorizationHeader CreateAuthorizationHeader(
            AuthorizationHeader.ParameterSet p)
        {
            return AuthorizationHeader.Create(p);
        }

    }

}
