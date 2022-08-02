namespace StreamThreads
{
    public class StreamStateWaitForever<T> : StreamStateWaitForever, StreamState<T>
    {

    }
    public class StreamStateWaitForever : StreamState
    {
        public StateTypes StateType { get; set; } = StateTypes.Normal;

        public bool Loop() => false;

        public void Terminate()
        {

        }
    }
}
