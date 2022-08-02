namespace StreamThreads
{
    public class StreamStateReturn<T> : StreamStateReturn, StreamState<T>
    {
        public StreamStateReturn(T ret) : base(ret)
        {
        }
    }

    public class StreamStateReturn : StreamState
    {
        public StateTypes StateType { get; set; } = StateTypes.Return;

        public dynamic? Return;

        public StreamStateReturn()
        {

        }

        public StreamStateReturn(dynamic? ret)
        {
            Return = ret;
        }

        public bool Loop()
        {
            return true;
        }

        public void Terminate()
        {
        }
    }
}
