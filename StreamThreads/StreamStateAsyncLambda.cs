namespace StreamThreads
{
    public class StreamStateAsyncLambda<T> : StreamStateAsyncLambda, StreamState<T>
    {
        public StreamStateAsyncLambda(Action<CancellationToken> lambda, Predicate cancel) : base(lambda, cancel)
        {
        }
    }
    public class StreamStateAsyncLambda : StreamState
    {
        private readonly CancellationTokenSource cts;
        private readonly CancellationToken token;
        private Task? _task;
        private readonly Predicate _cancel;

        public StateTypes StateType { get; set; } = StateTypes.Normal;

        public StreamStateAsyncLambda(Action<CancellationToken> lambda, Predicate cancel)
        {
            cts = new CancellationTokenSource();
            token = cts.Token;

            _cancel = cancel;

            _task = Task.Run(() => lambda(token), token);
        }

        public bool Loop()
        {
            if (_task == null) return true;

            if (_task.Status != TaskStatus.Running)
            {
                _task = null;
                return true;
            }

            if (!_cancel()) return false;

            cts.Cancel();
            return true;
        }

        public void Terminate()
        {
            cts.Cancel();
        }
    }

}
