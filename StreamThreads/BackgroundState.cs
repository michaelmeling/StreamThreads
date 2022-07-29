namespace StreamThreads
{
    internal class BackgroundState
    {
        internal bool Enabled = true;
        internal bool SwitchState;
        internal Action? Lambda;
        internal Predicate? Condition;
        internal StreamState? BackgroundLoop;
        internal IEnumerator<StreamState>? SwitchFunction;
    }
}