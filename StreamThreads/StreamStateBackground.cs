namespace StreamThreads
{
    public class StreamStateBackground<T> : StreamStateBackground, StreamState<T>
    {
        public StreamStateBackground(StreamState<T> background) : base(background)
        {
        }

        public StreamStateBackground(Action lambda) : base(lambda)
        {
        }

        public StreamStateBackground(StreamState background) : base(background)
        {
        }

        public StreamStateBackground(StreamState<T> background, Action lambda) : base(background, lambda)
        {
        }
    }



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

        public StateTypes StateType { get; set; } = StateTypes.Background;

        public  bool Loop()
        {
            return true;
        }

        public  void Terminate()
        {
            Background?.Terminate();
        }
    }

}
