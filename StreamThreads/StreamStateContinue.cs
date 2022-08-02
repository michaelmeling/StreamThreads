namespace StreamThreads
{
    internal class StreamStateContinue<T> : StreamStateContinue, StreamState<T>
    {
    }

    internal class StreamStateContinue : StreamState
    {
        public StateTypes StateType { get; set; } = StateTypes.Continue;

        public bool Loop()
        {
            return true;
        }

        public void Terminate()
        {
        }
    }
}