namespace StreamThreads
{

    public class StreamStateError<T> : StreamStateError, StreamState<T>
    {
        public StreamStateError(IEnumerable<StreamState> onError) : base(onError)
        {
        }
    }
    public class StreamStateError : StreamState
    {
        public IEnumerable<StreamState> OnError;

        public StreamStateError(IEnumerable<StreamState> onError)
        {
            OnError = onError;
        }

        public StateTypes StateType { get; set; } = StateTypes.Error;

        public bool Loop()
        {
            return true;
        }

        public void Terminate()
        {
        }
    }
}
