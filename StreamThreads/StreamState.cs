namespace StreamThreads
{
    public delegate bool Predicate();

    public class StreamState 
    {
        public Predicate Loop;
        public bool IsRunning;
        public StreamState? Backnew;
        public IEnumerable<StreamState>? Background;
        public IEnumerable<StreamState>? OnError;
        public IEnumerable<StreamState>? StateSwitch;

        public StreamState(Predicate canRun)
        {
            Loop = canRun;
            IsRunning = false;
        }

        internal bool HasBackground()
        {
            return Background != null;
        }

        internal bool HasOnError()
        {
            return OnError != null;
        }

        internal bool HasStateSwitch()
        {
            return StateSwitch != null;
        }
    }
}
