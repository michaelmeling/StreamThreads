using StreamThreads;
using System.Threading;
using System.Threading.Channels;

namespace StreamThreads
{
    public static class StreamExtensions
    {

        public static readonly StreamState OK = new StreamStateOK();
        public static readonly StreamState WaitForever = new StreamStateWaitForever();

        [ThreadStatic]
        private static DateTime? _lastrun;
        public static double SecondsSinceLast
        {
            get
            {
                var now = DateTime.Now;
                return (now - (_lastrun ?? now)).TotalSeconds;
            }
            set
            {
                _lastrun = DateTime.Now - TimeSpan.FromSeconds(value);
            }
        }

        public static IEnumerable<StreamState> Until(this IEnumerable<StreamState> me, Predicate condition)
        {
            if (condition()) yield break;

            foreach (var item in me)
            {
                yield return item;

                if (condition()) yield break;
            }
        }
        public static IEnumerable<StreamState> While(this IEnumerable<StreamState> me, Predicate condition)
        {
            var itr = me.GetEnumerator();
            while (true)
            {
                if (!condition())
                    yield return OK;
                else
                {
                    if (!itr.MoveNext())
                        yield break;
                    else
                        yield return itr.Current;
                }
            }
        }
        public static IEnumerable<StreamState> ExitOnError(this IEnumerable<StreamState> me)
        {
            var itr = me.GetEnumerator();
            while (true)
            {
                try
                {
                    if (!itr.MoveNext()) yield break;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    yield break;
                }

                yield return itr.Current;

            }
        }
        public static IEnumerable<StreamState> ResumeOnError(this IEnumerable<StreamState> me)
        {
            var itr = me.GetEnumerator();
            while (true)
            {
                try
                {
                    if (!itr.MoveNext()) yield break;
                }
                catch (Exception) { }

                yield return itr.Current;
            }
        }
        public static IEnumerable<StreamState> RestartOnError(this IEnumerable<StreamState> me)
        {
            int maxretries = 1;
            var itr = me.GetEnumerator();
            while (true)
            {
                try
                {
                    if (!itr.MoveNext()) yield break;
                    maxretries = 1;

                }
                catch (Exception)
                {
                    if (--maxretries < 0)
                        throw;

                    itr = me.GetEnumerator();
                }

                if (itr.Current != null)
                    yield return itr.Current;

            }
        }
        
        public static StreamState OnError(this IEnumerable<StreamState> c)
        {
            return new StreamStateError(c);
        }
        public static StreamState<T> OnError<T>(this IEnumerable<StreamState> c)
        {
            return new StreamStateError<T>(c);
        }

        public static StreamState SwitchOnCondition(this IEnumerable<StreamState> c, Predicate condition)
        {
            return new StreamStateSwitch(c, condition);
        }
        public static StreamState<T> SwitchOnCondition<T>(this IEnumerable<StreamState<T>> c, Predicate condition)
        {
            return new StreamStateSwitch<T>(c, condition);
        }
        
        public static StreamState Return(dynamic returnvalue)
        {
            return new StreamStateReturn(returnvalue);
        }
        public static StreamState<T> Return<T>(T returnvalue)
        {
            return new StreamStateReturn<T>(returnvalue);
        }
        
        public static StreamState Sleep(int millis)
        {
            var t = DateTime.Now + TimeSpan.FromMilliseconds(millis);

            return new StreamStateLambda(() => DateTime.Now > t);
        }
        public static StreamState<T> Sleep<T>(int millis)
        {
            var t = DateTime.Now + TimeSpan.FromMilliseconds(millis);

            return new StreamStateLambda<T>(() => DateTime.Now > t);
        }
        
        public static StreamState WaitFor(Predicate trigger)
        {
            if (trigger())
                return new StreamStateContinue();
            else
                return new StreamStateLambda(trigger);
        }        
        public static StreamState<T> WaitFor<T>(Predicate trigger)
        {
            if (trigger())
                return new StreamStateContinue<T>();
            else
                return new StreamStateLambda<T>(trigger);
        }
        
        public static StreamState Await(Action<CancellationToken> me, Predicate cancel)
        {
            if (cancel()) return OK;

            return new StreamStateAsyncLambda(me, cancel);
        }
        public static StreamState<T> Await<T>(Action<CancellationToken> me, Predicate cancel)
        {
            if (cancel()) return new StreamStateOK<T>(default);

            return new StreamStateAsyncLambda<T>(me, cancel);
        }
        public static StreamState Await<T>(this Task<T> me, out IteratorReturnVariable<T> retval)
        {
            return new StreamStateAsyncTask<T>(me, retval = new IteratorReturnVariable<T>());
        }
        public static StreamState<T> Await<T, R>(this Task<R> me, out IteratorReturnVariable<R> retval)
        {
            return new StreamStateAsyncTask<R, T>(me, retval = new IteratorReturnVariable<R>());
        }

        public static StreamState Await(this IEnumerable<StreamState> me)
        {
            return new StreamStateAwait(me, null);
        }
        public static StreamState Await(this IEnumerable<StreamState> me, out IteratorReturnVariable<dynamic> returnvalue)
        {
            return new StreamStateAwait(me, returnvalue = new IteratorReturnVariable<dynamic>());
        }
        public static StreamState Await<T>(this IEnumerable<StreamState<T>> me, out IteratorReturnVariable<T> returnvalue)
        {
            return new StreamStateAwait<T>(me, returnvalue = new IteratorReturnVariable<T>());
        }
        public static StreamState<T> Await<T>(this IEnumerable<StreamState> me)
        {
            return new StreamStateAwait<T>(me, null);
        }
        public static StreamState<T> Await<T>(this IEnumerable<StreamState> me, out IteratorReturnVariable<dynamic> returnvalue)
        {
            return new StreamStateAwait<T>(me, returnvalue = new IteratorReturnVariable<dynamic>());
        }
        public static StreamState<T> Await<T,R>(this IEnumerable<StreamState<R>> me, out IteratorReturnVariable<R> returnvalue)
        {
            return new StreamStateAwait<T>(me, returnvalue = new IteratorReturnVariable<R>());
        }

        public static StreamState Background(Action<CancellationToken> me, Predicate cancel)
        {
            return new StreamStateBackground(Await(me, cancel));
        }
        public static StreamState Background(Action lambda)
        {
            return new StreamStateBackground(lambda);
        }
        public static StreamState Background<T>(this Task<T> me, out IteratorReturnVariable<T> retval)
        {
            return new StreamStateBackground(Await<T>(me, out retval));
        }
        public static StreamState<T> Background<T>(Action<CancellationToken> me, Predicate cancel)
        {
            return new StreamStateBackground<T>(Await(me, cancel));
        }
        public static StreamState<T> Background<T, R>(this Task<R> me, out IteratorReturnVariable<R> retval)
        {
            return new StreamStateBackground<T>(Await<R>(me, out retval));
        }


        public static StreamState Background(this IEnumerable<StreamState> me)
        {
            return new StreamStateBackground(me.Await());
        }
        public static StreamState Background(this IEnumerable<StreamState> me, out IteratorReturnVariable<dynamic> returnvalue)
        {
            return new StreamStateBackground(me.Await(out returnvalue));
        }
        public static StreamState Background<T>(this IEnumerable<StreamState<T>> me, out IteratorReturnVariable<T> returnvalue)
        {
            return new StreamStateBackground(me.Await<T>(out returnvalue));
        }
        public static StreamState<T> Background<T>(this IEnumerable<StreamState> me)
        {
            return new StreamStateBackground<T>(me.Await());
        }
        public static StreamState<T> Background<T>(this IEnumerable<StreamState> me, out IteratorReturnVariable<dynamic> returnvalue)
        {
            return new StreamStateBackground<T>(me.Await(out returnvalue));
        }
        public static StreamState<T> Background<T,R>(this IEnumerable<StreamState<R>> me, out IteratorReturnVariable<R> returnvalue)
        {
            return new StreamStateBackground<T>(me.Await<R>(out returnvalue));
        }

        public static void SimulatedError(double probability = 0.1)
        {
            if (new Random().NextDouble() > 1 - probability) throw new Exception("Simulated Error");
        }
    }
}