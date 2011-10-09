using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TweetSource.EventSource;
using TweetSource.OAuth;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace TweetSourceClientDemo
{
    class Program
    {
        protected static bool LoadFromConfigFile = true;

        static void Main(string[] args)
        {
            var source = TweetEventSource.Create();

            source.EventReceived += (s, e) =>
            {
                // JSON data from Twitter is in e.JsonData
                Console.WriteLine("New Data: Total length = {0}", e.JsonText.Length);
                var json = JObject.Parse(e.JsonText);
                Console.WriteLine("{0},{1}",json["text"], json["user"]["screen_name"]);
            };

            source.SourceUp += (s, e) =>
            {
                // Connection established succesfully
                Console.WriteLine("Source Up: " + e.InfoText);
            };

            source.SourceDown += (s, e) =>
            {
                // At this point, the connection thread ends
                Console.WriteLine("Source Down: " + e.InfoText);
                Console.WriteLine("===== Application Ends =====");
                Console.Read();
            };

            var config = source.AuthConfig ;

            if (LoadFromConfigFile)
            {
                var settings = System.Configuration.ConfigurationManager.AppSettings;
                config.ConsumerKey = settings["ConsumerKey"];
                config.ConsumerSecret = settings["ConsumerSecret"];
                config.Token = settings["Token"];
                config.TokenSecret = settings["TokenSecret"];
            }
            else
            {
                config.ConsumerKey = "your consumer key";
                config.ConsumerSecret = "your consumer secret";
                config.Token = "your access token";
                config.TokenSecret = "your access token secret";
            }

            // This starts another thread, openining connection to Twitter
            // tweetSource.StartUserStream();

            source.StartFilterStream(new StreamingAPIParameters()
            {
                Track = new string[]{"Steve Jobs"}
            });
        }
    }

    /*
     * {"in_reply_to_user_id_str":"17518344",
     * "id_str":"122876205954375680",
     * "in_reply_to_user_id":17518344,
     * "text":"@keder I'll take a Romney w\/whom I have 80% agreement over Obama w\/10%.  My vote will count.",
     * "created_at":"Sun Oct 09 03:29:05 +0000 2011",
     * "contributors":null,"geo":null,"favorited":false,
     * "source":"\u003Ca href=\"http:\/\/twitter.com\/#!\/download\/ipad\" rel=\"nofollow\"\u003ETwitter for iPad\u003C\/a\u003E",
     * "retweet_count":0,
     * "in_reply_to_screen_name":"keder",
     * "coordinates":null,
     * "entities":{"hashtags":[],"urls":[],
     * "user_mentions":[{"id_str":"17518344","indices":[0,6],"name":"Kevin","id":17518344,"screen_name":"keder"}]},
     * "retweeted":false,
     * "in_reply_to_status_id":122871527229227008,
     * "in_reply_to_status_id_str":"122871527229227008",
     * "place":null,
     * "user":{"id_str":"24896276",
     *      "show_all_inline_media":true,
     *      "contributors_enabled":false,"following":null,"profile_background_image_url_https":"https:\/\/si0.twimg.com\/profile_background_images\/32506685\/070_a.jpg",
     * "created_at":"Tue Mar 17 15:17:40 +0000 2009","profile_background_color":"ff6f7d","profile_image_url":"http:\/\/a3.twimg.com\/profile_images\/106106062\/europe_2008_rebecca_194__600_x_450__normal.jpg","profile_background_tile":true,"favourites_count":7,"follow_request_sent":null,"time_zone":"Arizona","profile_sidebar_fill_color":"ffa9c1","url":"http:\/\/ramblinroseaz.blogspot.com\/","description":"\u271d Married mom to 1 in college & 1 home w\/autism\u2665Gluten free cook\u2665Tweet about politics\u2665Breast Ca 09\u2665\r\nWill follow if you interact & are civil\u2665","geo_enabled":false,"profile_sidebar_border_color":"C6E2EE","followers_count":335,"is_translator":false,"profile_image_url_https":"https:\/\/si0.twimg.com\/profile_images\/106106062\/europe_2008_rebecca_194__600_x_450__normal.jpg","listed_count":11,"profile_use_background_image":true,"friends_count":412,"location":"AZ Desert","default_profile":false,"profile_text_color":"387b16","protected":false,"lang":"en","verified":false,"profile_background_image_url":"http:\/\/a2.twimg.com\/profile_background_images\/32506685\/070_a.jpg","name":"Teresa Wendt","notifications":null,"profile_link_color":"1F98C7","id":24896276,"default_profile_image":false,"statuses_count":5810,"utc_offset":-25200,"screen_name":"projectmat"},"id":122876205954375680,"truncated":false}
     */
}
