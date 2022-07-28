using StreamThreads;
using System.Threading;
using System.Threading.Channels;

namespace StreamThreads
{
    public static class StreamExtensions
    {

        public static readonly StreamState OK = new(() => true);
        public static readonly StreamState WaitForever = new(() => false);

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
            var wait = new StreamState(() => true);

            var itr = me.GetEnumerator();
            while (true)
            {
                if (!condition())
                    yield return wait;
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
        public static StreamState Await(this IEnumerable<StreamState> c)
        {
            return c.AwaitPrivate(null);
        }
        public static StreamState Await(Action<CancellationToken> me, Predicate cancel)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;

            if (cancel()) return new StreamState(() => true);

            Task? t = Task.Run(() => me(token), token);

            return new StreamState(() =>
            {
                if (t == null) return true;

                if (t.Status != TaskStatus.Running)
                {
                    t = null;
                    return true;
                }

                if (!cancel()) return false;

                cts.Cancel();
                return true;
            })
            {
                Terminate = () =>
                {
                    cts.Cancel();
                }
            };
        }
        public static StreamState Await(this IEnumerable<StreamState> c, out IteratorReturnVariable returnvalue)
        {
            returnvalue = new IteratorReturnVariable() { IteratorState = IteratorStates.Running };
            return c.AwaitPrivate(returnvalue);
        }
        public static StreamState Await<T>(this IEnumerable<StreamState> c, out IteratorReturnVariable<T> returnvalue)
        {
            returnvalue = new IteratorReturnVariable<T>() { IteratorState = IteratorStates.Running };
            return c.AwaitPrivate(returnvalue);
        }
        public static StreamState Await<T>(this Task<T> me, out IteratorReturnVariable<T>? retval)
        {
            var tmp = new IteratorReturnVariable<T>();
            retval = tmp;
            return me.AwaitPrivate(tmp);

        }
        private static StreamState AwaitPrivate<T>(this Task<T> me, IteratorReturnVariable<T> tmp)
        {
            return new StreamState(() =>
            {
                if (me.Status == TaskStatus.RanToCompletion
                || me.Status == TaskStatus.Faulted
                || me.Status == TaskStatus.Canceled)
                {
                    tmp.Value = me.Result;

                    return true;
                }
                else
                    return false;
            });
        }
        private static StreamState AwaitPrivate(this IEnumerable<StreamState> c, IteratorReturnVariable? returnvalue)
        {
            var itr = c.GetEnumerator();
            List<BackgroundState> BackgroundThreads = new();
            IEnumerable<StreamState>? OnError = null;

            return new StreamState(() =>
                {
                    while (true)
                    {
                        try
                        {
                            bool running = true;
                            if (itr.Current == null)
                            {
                                running = itr.MoveNext();
                            }
                            else if (itr.Current.Loop())
                            {
                                running = itr.MoveNext();
                            }

                        exitfunction:
                            if (!running)
                            {
                                itr.Current?.Terminate?.Invoke();

                                foreach (var item in BackgroundThreads)
                                {
                                    item.BackgroundLoop?.Terminate?.Invoke();
                                }

                                if (returnvalue != null)
                                    returnvalue.IteratorState = IteratorStates.Ended;
                                return true;
                            }

                            switch (itr.Current!.StateType)
                            {
                                case StateTypes.Background:
                                    var b = new BackgroundState()
                                    {
                                        BackgroundLoop = itr.Current!.Background
                                    };
                                    BackgroundThreads.Add(b);
                                    continue;

                                case StateTypes.Error:
                                    OnError = itr.Current.OnError;
                                    continue;

                                case StateTypes.Switch:
                                    var sm = new BackgroundState()
                                    {
                                        SwitchState = true,
                                        UpdateIterator = itr.Current.StateSwitch!.GetEnumerator(),
                                        Condition = itr.Current.Loop
                                    };
                                    BackgroundThreads.Add(sm);
                                    continue;

                                case StateTypes.Return:
                                    if (returnvalue != null)
                                        returnvalue.Value = itr.Current.ReturnValue;

                                    running = false;
                                    goto exitfunction;

                                case StateTypes.Normal:
                                default:
                                    break;
                            }

                            for (int i = 0; i < BackgroundThreads.Count; i++)
                            {
                                var item = BackgroundThreads[i];
                                try
                                {
                                    if (!item.Enabled) continue;

                                    if (item.Condition?.Invoke() ?? true)
                                    {
                                        item.UpdateAction?.Invoke();

                                        if (item.SwitchState)
                                        {
                                            itr = item.UpdateIterator;
                                            BackgroundThreads.Clear();
                                            OnError = null;
                                        }
                                        else if (item.BackgroundLoop != null)
                                        {
                                            if (item.BackgroundLoop.Loop())
                                            {
                                                BackgroundThreads.RemoveAt(i);
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    BackgroundThreads.RemoveAt(i--);
                                    throw;
                                }
                            }

                            return false;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.StackTrace);

                            if (OnError != null)
                            {
                                itr = OnError.GetEnumerator();
                                BackgroundThreads.Clear();
                                OnError = null;
                            }
                            else
                            {
                                if (returnvalue != null)
                                    returnvalue.IteratorState = IteratorStates.Faulted;
                                throw;
                            }
                        }
                    }
                }
            )
            {
                Terminate = () =>
                {
                    foreach (var item in BackgroundThreads)
                    {
                        item.BackgroundLoop?.Terminate?.Invoke();
                    }
                }
            };
        }
        public static StreamState Background(this IEnumerable<StreamState> c)
        {
            return c.Background(out var notused);
        }
        public static StreamState Background(this IEnumerable<StreamState> c, out IteratorReturnVariable returnvalue)
        {
            IEnumerator<StreamState> itr = c.GetEnumerator();

            return new StreamState(() => true)
            {
                Background = c.Await(out returnvalue)
            };
        }
        public static StreamState Background<T>(this IEnumerable<StreamState<T>> c, out IteratorReturnVariable<T> returnvalue)
        {
            IEnumerator<StreamState<T>> itr = c.GetEnumerator();

            return new StreamState<T>(() => true)
            {
                Background = c.Await<T>(out returnvalue)
            };
        }
        public static StreamState Background(Action<CancellationToken> me, Predicate cancel)
        {
            return new StreamState(() => true)
            {
                Background = Await(me, cancel)
            };
        }
        public static StreamState Background<T>(this Task<T> me, out IteratorReturnVariable<T>? retval)
        {
            return new StreamState(() => true)
            {
                Background = Await(me, out retval)
            };
        }
        public static StreamState OnError(this IEnumerable<StreamState> c)
        {
            IEnumerator<StreamState> itr = c.GetEnumerator();

            return new StreamState(() => true)
            {
                OnError = c
            };
        }
        public static StreamState SwitchOnCondition(this IEnumerable<StreamState> c, Predicate condition)
        {
            IEnumerator<StreamState> itr = c.GetEnumerator();

            return new StreamState(() => true)
            {
                Loop = condition,
                StateSwitch = c
            };
        }
        public static StreamState Return(dynamic returnvalue)
        {
            return new StreamState(() => true)
            {
                ReturnValue = returnvalue
            };
        }
        public static StreamState<T> Return<T>(T returnvalue)
        {
            return new StreamState<T>(() => true)
            {
                ReturnValue = returnvalue
            };
        }
        public static StreamState Sleep(int millis)
        {
            var t = DateTime.Now + TimeSpan.FromMilliseconds(millis);

            return new StreamState(() => DateTime.Now > t);
        }
        public static StreamState WaitFor(Predicate trigger)
        {
            return new StreamState(trigger);
        }

        public static void SimulatedError(double probability = 0.1)
        {
            if (new Random().NextDouble() > 1 - probability) throw new Exception("Simulated Error");
        }
    }
}