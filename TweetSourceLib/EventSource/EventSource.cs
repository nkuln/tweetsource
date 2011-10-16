//
// Copyright (C) 2011 by Natthawut Kulnirundorn

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TweetSource.EventSource
{
    /// <summary>
    /// Generic implementation of Event Source concept.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    public abstract class EventSource<T>
        where T : EventArgs
    {
        /// <summary>
        /// This is fired when there is new event in the queue. 
        /// It is processed on the thread that calls Dispatch().
        /// </summary>
        public event EventHandler<T> EventReceived;

        /// <summary>
        /// This is fired when EventSource is unable to provide events any more. 
        /// For example, when connection down, exception occurred. 
        /// 
        /// This is called from internal thread and not the one that calls Dispatch()
        /// </summary>
        public event EventHandler<T> SourceDown;

        /// <summary>
        /// This is fired when EventSource is ready to provide events. 
        /// For example, when connection established.
        /// 
        /// This is called from internal thread and not the one that calls Dispatch()
        /// </summary>
        public event EventHandler<T> SourceUp;

        /// <summary>
        /// Check if the event source is active. If not, user can stop dispatching events.
        /// </summary>
        public abstract bool Active { get; }

        /// <summary>
        /// When this is called and there is some data in queue, EventSource's EventReceived event will 
        /// be fired on the thread that calls Dispatch().
        /// 
        /// If timeOutInMs is not set, the call blocks until there is new event. With timeOutInMs set, 
        /// the call returns after timeout.
        /// </summary>
        /// <param name="timeOutInMs">Time to wait for event, 0 to wait indefinitely</param>
        public abstract void Dispatch(int timeOutInMs = 0);

        /// <summary>
        /// Helper for firing EventReceived from child classes
        /// </summary>
        /// <param name="data">Event to fire</param>
        protected internal void FireEventReceived(T data)
        {
            if (EventReceived != null)
                EventReceived(this, data);
        }

        /// <summary>
        /// Helper for firing SourceDown from child classes
        /// </summary>
        /// <param name="data">Event to fire</param>
        protected internal void FireSourceDown(T data)
        {
            if (SourceDown != null)
                SourceDown(this, data);
        }

        /// <summary>
        /// Helper for firing SourceUp from child classes
        /// </summary>
        /// <param name="data">Event to fire</param>
        protected internal void FireSourceUp(T data)
        {
            if (SourceUp != null)
                SourceUp(this, data);
        }
    }

    /// <summary>
    /// Base implementation for EventSource that implementation the thread-safe queuing.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    public abstract class EventSourceBaseImpl<T> : EventSource<T>
        where T : EventArgs
    {
        private const int INIT_QUEUE_SIZE = 1000;
        private const int MAX_QUEUE_SIZE = 100000;

        private Queue<T> eventQueue = new Queue<T>(INIT_QUEUE_SIZE);

        private object queueLock = new object();

        private AutoResetEvent newEventSignal = new AutoResetEvent(false);

        /// <summary>
        /// Current number of event wait in queue to be dispatched.
        /// </summary>
        public int NumberOfEventInQueue
        {
            get
            {
                lock (queueLock)
                {
                    return eventQueue.Count;
                }
            }
        }

        /// <summary>
        /// (Thread-safe) Enqueue new event.
        /// </summary>
        /// <param name="newEvent">Event to queue</param>
        protected internal void EnqueueEvent(T newEvent)
        {
            lock (queueLock)
            {
                eventQueue.Enqueue(newEvent);
                newEventSignal.Set();
            }
        }

        /// <summary>
        /// (Thread-safe) Dispatch event, fires callbacks.
        /// </summary>
        /// <param name="timeOutInMs">Time to wait for new event in milliseconds (0 = wait forever)</param>
        public sealed override void Dispatch(int timeOutInMs = 0)
        {
            if (timeOutInMs > 0)
                this.newEventSignal.WaitOne(TimeSpan.FromMilliseconds(timeOutInMs));
            else
                this.newEventSignal.WaitOne();

            lock (queueLock)
            {
                while (this.eventQueue.Count != 0)
                    FireEventReceived(eventQueue.Dequeue());
            }
        }
    }
}
