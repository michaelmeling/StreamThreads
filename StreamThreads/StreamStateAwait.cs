namespace StreamThreads
{

    public class StreamStateAwait<T> : StreamStateAwait, StreamState<T>
    {
        public StreamStateAwait(IEnumerable<StreamState> c, IteratorReturnVariable? returnvalue) : base(c, returnvalue)
        {
        }
    }

    public class StreamStateAwait : StreamState
    {
        public StateTypes StateType { get; set; } = StateTypes.Continue;

        public IteratorReturnVariable? ReturnValue;
        internal IEnumerator<StreamState> Iterator;
        internal IEnumerable<StreamState>? ErrorHandler;
        internal List<BackgroundState> BackgroundThreads = new();


        public StreamStateAwait(IEnumerable<StreamState> c, IteratorReturnVariable? returnvalue) : base()
        {
            Iterator = c.GetEnumerator();
            ErrorHandler = null;
            ReturnValue = returnvalue;

            if (returnvalue != null)
                ReturnValue!.IteratorState = IteratorStates.Running;
        }

        public bool Loop()
        {
            while (true)
            {
                try
                {
                    bool continueonce = false;
                    bool running = true;
                    if (Iterator.Current == null)
                    {
                        running = Iterator.MoveNext();
                        continueonce = running && Iterator.Current!.StateType == StateTypes.Continue;
                    }
                    else if (Iterator.Current.Loop())
                    {
                        running = Iterator.MoveNext();
                        continueonce = running && Iterator.Current!.StateType == StateTypes.Continue;
                    }

                exitfunction:
                    if (!running)
                    {
                        Iterator.Current?.Terminate();

                        foreach (BackgroundState item in BackgroundThreads)
                        {
                            item.BackgroundLoop?.Terminate();
                        }

                        if (ReturnValue != null)
                            ReturnValue.IteratorState = IteratorStates.Ended;
                        return true;
                    }

                    switch (Iterator.Current!.StateType)
                    {
                        case StateTypes.Continue:
                            if (continueonce)
                                continue;
                            else
                                break;
                        case StateTypes.Error:
                            ErrorHandler = ((StreamStateError)Iterator.Current).OnError;
                            continue;

                        case StateTypes.Switch:
                            BackgroundState sm = new BackgroundState()
                            {
                                SwitchState = true,
                                SwitchFunction = ((StreamStateSwitch)Iterator.Current).OnSwitch.GetEnumerator(),
                                Condition = ((StreamStateSwitch)Iterator.Current).Condition
                            };
                            BackgroundThreads.Add(sm);
                            continue;

                        case StateTypes.Background:
                            BackgroundState bgs = new BackgroundState()
                            {
                                BackgroundLoop = ((StreamStateBackground)Iterator.Current).Background,
                                Lambda = ((StreamStateBackground)Iterator.Current).Lambda
                            };
                            BackgroundThreads.Add(bgs);
                            continue;

                        case StateTypes.Return:
                            if (ReturnValue != null)

                                ReturnValue.Value = ((StreamStateReturn)Iterator.Current).Return;

                            running = false;
                            goto exitfunction;

                        default:
                            break;
                    }

                    for (int i = 0; i < BackgroundThreads.Count; i++)
                    {
                        BackgroundState item = BackgroundThreads[i];
                        try
                        {
                            if (!item.Enabled) continue;

                            if (item.Condition?.Invoke() ?? true)
                            {
                                item.Lambda?.Invoke();

                                if (item.SwitchState)
                                {
                                    Iterator = ((BackgroundState)item).SwitchFunction!;
                                    BackgroundThreads.Clear();
                                    ErrorHandler = null;
                                }
                                else if (item.BackgroundLoop != null)
                                {
                                    if (item.BackgroundLoop.Loop())
                                    {
                                        BackgroundThreads.RemoveAt(i--);
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

                    if (ErrorHandler != null)
                    {
                        Iterator = ErrorHandler.GetEnumerator();
                        BackgroundThreads.Clear();
                        ErrorHandler = null;
                    }
                    else
                    {
                        if (ReturnValue != null)
                            ReturnValue.IteratorState = IteratorStates.Faulted;
                        throw;
                    }
                }
            }

        }
        public void Terminate()
        {
            foreach (BackgroundState item in BackgroundThreads)
            {
                item.BackgroundLoop?.Terminate();
            }
        }
    }
}
