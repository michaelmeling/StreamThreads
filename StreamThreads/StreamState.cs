namespace StreamThreads
{
    public delegate bool Predicate();
    internal enum StateTypes { Normal, Background, Error, Switch, Return,
        Continue
    }

    public class StreamState
    {
        public virtual bool Loop() => true;

        public virtual void Terminate()
        {

        }

        internal virtual StateTypes StateType => StateTypes.Normal;
    }

    public class StreamState<T> : StreamState
    {

    }

    public class StreamStateAsyncTask<T> : StreamState
    {
        private readonly Task<T> me;
        private readonly IteratorReturnVariable<T> tmp;

        public StreamStateAsyncTask(Task<T> me, IteratorReturnVariable<T> tmp)
        {
            this.me = me;
            this.tmp = tmp;

            tmp.IteratorState = IteratorStates.Running;
        }

        public override bool Loop()
        {
            switch (me.Status)
            {
                case TaskStatus.RanToCompletion:
                    tmp.IteratorState = IteratorStates.Ended;
                    break;
                case TaskStatus.Canceled:
                    tmp.IteratorState = IteratorStates.Terminated;
                    break;
                case TaskStatus.Faulted:
                    tmp.IteratorState = IteratorStates.Faulted;
                    break;
                case TaskStatus.Created:
                case TaskStatus.WaitingForActivation:
                case TaskStatus.WaitingToRun:
                case TaskStatus.Running:
                case TaskStatus.WaitingForChildrenToComplete:
                default:
                    break;
            }

            if (tmp.IteratorState == IteratorStates.Running)
                return false;

            tmp.Value = me.Result;
            return true;

        }

    }

}
