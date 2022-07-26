// See https://aka.ms/new-console-template for more information
    using StreamThreads;
    using StripeSample;
    using static StreamThreads.StreamExtensions;

    Console.WriteLine("Hello, World!");

    Host host = new Host();
    var state = host.StartupState().Await();

    while (true)
    {
    
        state.Loop();
        SecondsSinceLast = 0;

        Thread.Sleep(10);
}