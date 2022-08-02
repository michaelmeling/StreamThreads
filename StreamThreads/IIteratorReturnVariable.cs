namespace StreamThreads
{
    public interface IIteratorReturnVariable
    {
        bool Faulted { get; }
        bool HasEnded { get; }
        bool IsRunning { get; }
        dynamic? Value { get; set; }
        bool WasTerminated { get; }

        StreamState Await();
        bool HasValue();
        string ToString();
    }

    public interface IIteratorReturnVariable<T> : IIteratorReturnVariable
    {

    }
}