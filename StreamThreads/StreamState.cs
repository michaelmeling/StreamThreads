namespace StreamThreads
{
    public delegate bool Predicate();
    public enum StateTypes
    {
        Normal, Background, Error, Switch, Return,
        Continue
    }

    public interface StreamState
    {
        StateTypes StateType { get; set; }
        bool Loop();
        void Terminate();
    }

    public interface StreamState<T> : StreamState
    {
    }

}
