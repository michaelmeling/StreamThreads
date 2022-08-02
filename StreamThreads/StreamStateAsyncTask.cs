namespace StreamThreads
{
    public class StreamStateAsyncTask<T, Q> : StreamStateAsyncTask<T>, StreamState<Q>
    {
        public StreamStateAsyncTask(Task<T> me, IteratorReturnVariable tmp) : base(me, tmp)
        {
        }
    }

    public class StreamStateAsyncTask<T> : StreamState
    {
        private readonly Task<T> me;
        private readonly IteratorReturnVariable tmp;

        public StreamStateAsyncTask(Task<T> me, IteratorReturnVariable tmp)
        {
            this.me = me;
            this.tmp = tmp;

            tmp.IteratorState = IteratorStates.Running;
        }

        public StateTypes StateType { get; set; } = StateTypes.Normal;

        public bool Loop()
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

        public void Terminate()
        {

        }
    }

}
