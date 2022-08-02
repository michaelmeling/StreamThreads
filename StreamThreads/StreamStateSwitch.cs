namespace StreamThreads
{
    public class StreamStateSwitch<T> : StreamStateSwitch, StreamState<T>
    {
        public StreamStateSwitch(IEnumerable<StreamState<T>> onSwitch, Predicate condition) : base(onSwitch, condition)
        {
        }
    }

    public class StreamStateSwitch : StreamState
    {
        public IEnumerable<StreamState> OnSwitch;
        internal Predicate Condition;

        public StreamStateSwitch(IEnumerable<StreamState> onSwitch, Predicate condition)
        {
            OnSwitch = onSwitch;
            Condition = condition;
        }

        public StateTypes StateType { get; set; } = StateTypes.Switch;

        public bool Loop()
        {
            return true;
        }

        public void Terminate()
        {
        }
    }

}
