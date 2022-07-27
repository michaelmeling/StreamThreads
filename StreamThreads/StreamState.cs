namespace StreamThreads
{
    public delegate bool Predicate();

    public class StreamState
    {
        internal enum StateTypes { Normal, Background, Error, Switch }
        public Predicate Loop;
        internal Action? Terminate;
        internal StateTypes StateType;
        private object? _state;

        internal StreamState? Background { get => _state as StreamState; set { _state = value; StateType = StateTypes.Background; } }
        internal IEnumerable<StreamState>? OnError { get => _state as IEnumerable<StreamState>; set => _state = value; }
        internal IEnumerable<StreamState>? StateSwitch { get => _state as IEnumerable<StreamState>; set => _state = value; }

        public StreamState(Predicate canRun)
        {
            Loop = canRun;
            Terminate = null;
            _state = null;
            StateType = StateTypes.Normal;
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
