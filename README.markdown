#TweetSource
##Overview
TweetSource allows you to consume [Twitter's Streaming API](https://dev.twitter.com/docs/streaming-api) easily from .NET / C#.

What makes Streaming API different from [Twitter's REST API](https://dev.twitter.com/docs/api) is that consumer of streaming API needs to keep the HTTP connection open indefinitely. Twitter will continue to feed data down this connection as it updates in real-time. 

Hence Streaming API is more suitable for heavy tweet-processing application that needs to process large volume of data in real-time.

##Quick Start
This example uses TweetSource to consume Sample Stream from Twitter. 

Add reference to `TweetSource.dll` to your project. Also, you have to replace keys and secrets with ones you get from your application registered with Twitter

Note: you will want to handle `SourceDown` event as this can give some clues when thing go wrong.

```c#
    static void Main(string[] args)
    {
        var source = TweetEventSource.CreateSampleStream();
        source.EventReceived += 
            new EventHandler<TweetEventArgs>(source_EventReceived);
        source.SourceUp += 
            new EventHandler<TweetEventArgs>(source_SourceUp);
        source.SourceDown += 
            new EventHandler<TweetEventArgs>(source_SourceDown);

        var config = source.AuthConfig;
        config.ConsumerKey = "..consumer key..";
        config.ConsumerSecret = "..consumer secret..";
        config.Token = "..access token..";
        config.TokenSecret = "..access token secret..";

        source.Start();

        while (source.Active)
            source.Dispatch(5000);
    }

    static void source_SourceUp(object sender, TweetEventArgs e)
    {
        Console.WriteLine("Ready! " + e.InfoText);
    }

    static void source_SourceDown(object sender, TweetEventArgs e)
    {
        Console.WriteLine("Source Down: " + e.InfoText);
    }

    static void source_EventReceived(object sender, TweetEventArgs e)
    {
        Console.WriteLine(e.JsonText);
    }
```

##Stream with Parameters
Some types of stream in Twitter's Streaming API accept parameters. For example, Filter Stream accepts `track` parameter for tracking certain keywords, `follow` parameters to filter only updates from set of user IDs.

You can pass these parameters to `TweetEventSource.Start` method, specifying the desired values in `StreamingAPIParameters` instance. Example below tracks tweet about 'Steve Jobs'.

```c#
    var source = TweetEventSource.CreateFilterStream();

    // ..
    // Register event handlers
    // ..
    // Keys and secrets configuration
    // ..

    source.Start(new StreamingAPIParameters()
    {
        Track = new string[]{"Steve Jobs"}
    });
```

##OAuth Authentication
At the time of writing (16 Oct 2011), Twitter still supports both Basic Authentication and OAuth for Streaming API. However, they plan to migrate this to OAuth-only soon. So TweetSource decided to support only OAuth from the very start. 

OAuth is inherently complicated. But I am taking you through the shortest path here ..

In order to authenticate with OAuth, there are 4 values you need to obtain:

 1. Consumer Key
 2. Consumer Secret
 3. Access Token
 4. Access Token Secret

You can obtain Consumer Key and Consumer Secret easily by creating an application in [Twitter Developers](https://dev.twitter.com.)

The Access Token and Access Token Secret are normally obtained programmatically via sequence of OAuth calls. Luckily, the application page in Twitter Developers allow you to obtain this easily. Visit your [application page](https://dev.twitter.com/apps) and request the access token.

For example, this is taken from my application page. You have only to focus in the area in red.

![Twitter's Application Page](https://lh5.googleusercontent.com/-wTkjS71kolw/TprTnFgW7EI/AAAAAAAAF6g/6xJrbVzYO8s/s800/Twitter_OAuth.PNG)

##Access Limitations
Some parameters are there, but normal account type won't be able to use it. For example, you cannot consume from Retweet Stream, use `count` parameter, or track more than 5,000 user IDs using normal account type. These limitations change now and then so it's better to refer to Twitter's documentation on [Streaming API](https://dev.twitter.com/docs/streaming-api).

##Inner Workings
TweetSource library provides you the main `TweetEventSource` class. Internally, it makes authenticated request to URL providing Streaming API service, pulls down the data and put it in internal queue. This is done in an internal thread dedicated for this.

The user then call `TweetEventSource.Dispatch()` method which dequeue data from queue and calls the `EventReceived` callback on the user's thread.

Having queue in between does incur additional latency, but it provides the following benefits:

  - The queue serves as a buffering area in case the application cannot process Tweet fast enough.
  - User has full control over the thread that process the tweet. 

