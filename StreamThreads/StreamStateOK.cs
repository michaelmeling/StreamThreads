namespace StreamThreads
{
    public class StreamStateOK<T> : StreamStateOK, StreamState<T>
    {
        public StreamStateOK(T value)
        {
        }
    }
    public class StreamStateOK : StreamState
    {
        public StateTypes StateType { get; set; } = StateTypes.Normal;

        public bool Loop()
        {
            return true;
        }

        public void Terminate()
        {

        }
    }
}