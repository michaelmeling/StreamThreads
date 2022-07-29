namespace StreamThreads
{
    public class StreamStateError : StreamState
    {
        public IEnumerable<StreamState> OnError;

        public StreamStateError(IEnumerable<StreamState> onError)
        {
            OnError = onError;
        }

        internal override StateTypes StateType => StateTypes.Error;
    }

}
