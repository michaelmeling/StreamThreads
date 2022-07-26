using System.Threading;

namespace StreamThreads
{
    public static class StreamExtensions
    {

        public static readonly StreamState OK = new (() => true);
        public static readonly StreamState WaitForever = new (() => false);

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

                            if (!running) return true;

                            if (itr.Current!.HasBackground())
                            {
                                var b = new BackgroundState()
                                {
                                    BackgroundLoop = itr.Current!.Backnew
                                    //,                                    UpdateIterator = itr.Current!.Background!.GetEnumerator()
                                };
                                BackgroundThreads.Add(b);
                                continue;
                            }

                            if (itr.Current!.HasOnError())
                            {
                                OnError = itr.Current.OnError;
                                continue;
                            }

                            if (itr.Current!.HasStateSwitch())
                            {
                                var sm = new BackgroundState()
                                {
                                    SwitchState = true,
                                    UpdateIterator = itr.Current.StateSwitch!.GetEnumerator(),
                                    Condition = itr.Current.Loop
                                };
                                BackgroundThreads.Add(sm);
                                continue;
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
                                        else if(item.BackgroundLoop != null)
                                        {
                                            if (item.BackgroundLoop.Loop())
                                            {
                                                BackgroundThreads.RemoveAt(i);
                                            }
                                        }
                                        else if (item.UpdateIterator != null)
                                        {
                                            if (item.UpdateIterator.Current == null
                                                || item.UpdateIterator.Current.Loop())
                                            {
                                                if (!item.UpdateIterator.MoveNext())
                                                {
                                                    BackgroundThreads.RemoveAt(i--);
                                                }
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
                        catch (Exception)
                        {
                            if (OnError != null)
                            {
                                itr = OnError.GetEnumerator();
                                BackgroundThreads.Clear();
                                OnError = null;
                            }
                            else
                                throw;
                        }
                    }
                }
            );
        }

        public static void SimulatedError(double probability = 0.1)
        {
            if (new Random().NextDouble() > 1 - probability) throw new Exception("Simulated Error");
        }

        public static StreamState Background(this IEnumerable<StreamState> c)
        {
            IEnumerator<StreamState> itr = c.GetEnumerator();

            return new StreamState(() => true)
            {
                Backnew = c.Await(),
                Background = c
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

        public static StreamState Sleep(int millis)
        {
            var t = DateTime.Now + TimeSpan.FromMilliseconds(millis);

            return new StreamState(() => DateTime.Now > t);
        }

        public static StreamState WaitFor(Predicate trigger)
        {
            return new StreamState(trigger);
        }

    }
}
