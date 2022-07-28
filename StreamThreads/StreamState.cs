namespace StreamThreads
{
    public delegate bool Predicate();
    internal enum StateTypes { Normal, Background, Error, Switch, Return }

    public class StreamState
    {
        public Predicate Loop;
        internal Action? Terminate;
        internal StateTypes StateType;
        private object? _state;

        internal StreamState? Background { get => _state as StreamState; set { _state = value; StateType = StateTypes.Background; } }
        internal IEnumerable<StreamState>? OnError { get => _state as IEnumerable<StreamState>; set { _state = value; StateType = StateTypes.Error; } }
        internal IEnumerable<StreamState>? StateSwitch { get => _state as IEnumerable<StreamState>; set { _state = value; StateType = StateTypes.Switch; } }
        internal dynamic? ReturnValue { get => _state; set { _state = value; StateType = StateTypes.Return; } }
        public StreamState(Predicate canRun)
        {
            Loop = canRun;
            Terminate = null;
            _state = null;
            StateType = StateTypes.Normal;
        }

    }

    public class StreamState<T> : StreamState
    {
        public StreamState(Predicate canRun) : base(canRun)
        {
        }
    }

}
