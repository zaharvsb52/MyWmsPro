using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace wmsMLC.Business.Workflow.Helpers
{
    // BigBallOfMud by Microsoft
    abstract class AsyncResult : IAsyncResult
    {
        static AsyncCallback _asyncCompletionWrapperCallback;
        AsyncCallback callback;
        bool _completedSynchronously;
        bool _endCalled;
        Exception _exception;
        bool _isCompleted;
        ManualResetEvent _manualResetEvent;
        AsyncCompletion _nextAsyncCompletion;
        object state;
        object thisLock;

        protected AsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            this.thisLock = new object();
        }

        public object AsyncState
        {
            get
            {
                return state;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_manualResetEvent != null)
                {
                    return _manualResetEvent;
                }

                lock (ThisLock)
                {
                    if (_manualResetEvent == null)
                    {
                        _manualResetEvent = new ManualResetEvent(_isCompleted);
                    }
                }

                return _manualResetEvent;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return _completedSynchronously;
            }
        }

        public bool HasCallback
        {
            get
            {
                return this.callback != null;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
        }

        object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        protected void Complete(bool completedSynchronously)
        {
            if (_isCompleted)
            {
                // It's a bug to call Complete twice.
                throw new InvalidProgramException();
            }

            this._completedSynchronously = completedSynchronously;

            if (completedSynchronously)
            {
                // If we completedSynchronously, then there's no chance that the manualResetEvent was created so
                // we don't need to worry about a race
                this._isCompleted = true;
            }
            else
            {
                lock (ThisLock)
                {
                    this._isCompleted = true;
                    if (this._manualResetEvent != null)
                    {
                        this._manualResetEvent.Set();
                    }
                }
            }

            if (callback != null)
            {
                try
                {
                    callback(this);
                }
                catch (Exception e)
                {
                    throw new InvalidProgramException("Async callback threw an Exception", e);
                }
            }
        }

        protected void Complete(bool completedSynchronously, Exception exception)
        {
            this._exception = exception;
            Complete(completedSynchronously);
        }

        static void AsyncCompletionWrapperCallback(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            AsyncResult thisPtr = (AsyncResult)result.AsyncState;
            AsyncCompletion callback = thisPtr.GetNextCompletion();

            bool completeSelf = false;
            Exception completionException = null;
            try
            {
                completeSelf = callback(result);
            }
            catch (Exception e)
            {
                if (IsFatal(e))
                {
                    throw;
                }
                completeSelf = true;
                completionException = e;
            }

            if (completeSelf)
            {
                thisPtr.Complete(false, completionException);
            }
        }

        public static bool IsFatal(Exception exception)
        {
            while (exception != null)
            {
                if ((exception is OutOfMemoryException && !(exception is InsufficientMemoryException)) ||
                    exception is ThreadAbortException ||
                    exception is AccessViolationException ||
                    exception is SEHException)
                {
                    return true;
                }

                // These exceptions aren't themselves fatal, but since the CLR uses them to wrap other exceptions,
                // we want to check to see whether they've been used to wrap a fatal exception.  If so, then they
                // count as fatal.
                if (exception is TypeInitializationException ||
                    exception is TargetInvocationException)
                {
                    exception = exception.InnerException;
                }
                else
                {
                    break;
                }
            }

            return false;
        }

        protected AsyncCallback PrepareAsyncCompletion(AsyncCompletion callback)
        {
            this._nextAsyncCompletion = callback;
            if (AsyncResult._asyncCompletionWrapperCallback == null)
            {
                AsyncResult._asyncCompletionWrapperCallback = new AsyncCallback(AsyncCompletionWrapperCallback);
            }
            return AsyncResult._asyncCompletionWrapperCallback;
        }

        AsyncCompletion GetNextCompletion()
        {
            AsyncCompletion result = this._nextAsyncCompletion;
            this._nextAsyncCompletion = null;
            return result;
        }

        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result)
            where TAsyncResult : AsyncResult
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            TAsyncResult asyncResult = result as TAsyncResult;

            if (asyncResult == null)
            {
                throw new ArgumentException("Invalid AsyncResult", "result");
            }

            if (asyncResult._endCalled)
            {
                throw new InvalidOperationException("AsyncResult already ended");
            }

            asyncResult._endCalled = true;

            if (!asyncResult._isCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }

            if (asyncResult._manualResetEvent != null)
            {
                asyncResult._manualResetEvent.Close();
            }

            if (asyncResult._exception != null)
            {
                throw asyncResult._exception;
            }

            return asyncResult;
        }

        // can be utilized by subclasses to write core completion code for both the sync and async paths
        // in one location, signaling chainable synchronous completion with the boolean result,
        // and leveraging PrepareAsyncCompletion for conversion to an AsyncCallback.
        // NOTE: requires that "this" is passed in as the state object to the asynchronous sub-call being used with a completion routine.
        protected delegate bool AsyncCompletion(IAsyncResult result);
    }
}
