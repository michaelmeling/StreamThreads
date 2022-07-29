namespace StreamThreads
{
    public class StreamStateReturn : StreamState
    {
        public virtual dynamic? Return { get => _value; set => _value = value; }
        private dynamic? _value;

        public StreamStateReturn()
        {

        }

        public StreamStateReturn(dynamic ret)
        {
            _value = ret;
        }

        internal override StateTypes StateType => StateTypes.Return;
    }

    public class StreamStateReturn<T> : StreamStateReturn
    {
        public override dynamic? Return { get => _value; set => _value = value; }
        private T? _value;
        public StreamStateReturn(T ret)
        {
            _value = ret;
        }
    }


}
