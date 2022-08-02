namespace StreamThreads
{
    public class StreamStateLambda<T> : StreamStateLambda, StreamState<T>
    {
        public StreamStateLambda(Predicate lambdaloop) : base(lambdaloop)
        {
        }
    }

    public class StreamStateLambda : StreamState
    {
        internal Action? TerminateLambda;
        internal Predicate Lambda;

        public StreamStateLambda(Predicate lambdaloop) : base()
        {
            Lambda = lambdaloop;
            TerminateLambda = null;
        }

        public StateTypes StateType { get; set; } = StateTypes.Normal;

        public bool Loop()
        {
            return Lambda();
        }

        public void Terminate()
        {
            TerminateLambda?.Invoke();
        }
    }


}
