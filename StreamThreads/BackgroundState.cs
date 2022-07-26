namespace StreamThreads
{
    public class BackgroundState
    {
        public bool Enabled = true;
        public Predicate? Condition;
        public Action? UpdateAction;
        public IEnumerator<StreamState>? UpdateIterator;
        public bool KeepSubStates;
        public bool Unchecked;
        internal bool SwitchState;
        internal StreamState? BackgroundLoop;
    }
}