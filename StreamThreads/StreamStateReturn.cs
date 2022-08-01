namespace StreamThreads
{
    public interface IStreamStateReturn
    {
        dynamic? GetValue();
    }

    public class StreamStateReturn : StreamState, IStreamStateReturn
    {
        internal override StateTypes StateType => StateTypes.Return;

        public dynamic? Return;

        public StreamStateReturn()
        {

        }

        public StreamStateReturn(dynamic ret)
        {
            Return = ret;
        }

        public dynamic? GetValue()
        {
            return Return;
        }
    }

    public class StreamStateReturn<T> : StreamState<T>, IStreamStateReturn
    {
        internal override StateTypes StateType => StateTypes.Return;

        public T? Return;

        public StreamStateReturn()
        {
        }

        public StreamStateReturn(T ret)
        {
            Return = ret;
        }

        public dynamic? GetValue()
        {
            return Return;
        }
    }


}
