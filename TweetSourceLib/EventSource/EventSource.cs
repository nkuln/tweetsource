using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TweetSource.EventSource
{
    public abstract class EventSource<T>
        where T : EventArgs
    {
        public event EventHandler<T> EventReceived;

        public event EventHandler<T> SourceDown;

        public event EventHandler<T> SourceUp;

        public abstract void Dispatch(int timeOutInMs = 0);

        protected internal void FireEventReceived(T data)
        {
            if (EventReceived != null)
                EventReceived(this, data);
        }

        protected internal void FireSourceDown(T data)
        {
            if (SourceDown != null)
                SourceDown(this, data);
        }

        protected internal void FireSourceUp(T data)
        {
            if (SourceUp != null)
                SourceUp(this, data);
        }
    }

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
