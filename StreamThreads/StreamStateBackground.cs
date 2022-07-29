namespace StreamThreads
{
    public class StreamStateBackground : StreamState
    {
        public StreamState? Background;
        public Action? Lambda;

        public StreamStateBackground(StreamState background)
        {
            Background = background;
            Lambda = null;
        }

        public StreamStateBackground(Action lambda)
        {
            Background = null;
            Lambda = lambda;
        }

        public StreamStateBackground(StreamState background, Action lambda)
        {
            Background = background;
            Lambda = lambda;
        }

        internal override StateTypes StateType => StateTypes.Background;
    }

    public class StreamStateBackground<T> : StreamStateBackground
    {
        public StreamStateBackground(StreamState<T> background) : base(background)
        {
        }
    }

}
