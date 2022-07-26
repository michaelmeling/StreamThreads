# StreamThread
**Single Threaded Recursive Iterators for Parallel Execution**

StreamThreads is a library that allows you to write clean and plain code that can execute processes in parallel, but using only a single thread. It is an alternative to async/await and Task.Run(), but without locks, concurrent collections and synchronization. In some sense, it is more like a game-loop where each object in the scene needs updating every few milliseconds before the screen refreshes. Unfortunately, game-loops often end up with significant amounts of global status variables and case-statements in complex scenes. StreamThread helps by allowing a game-loop to be written as a "multi-threaded" application, where each function is executing independently.

Example:

        public IEnumerable<StreamState> StartupState()
        {
            yield return HandleFault().OnError();

            yield return PrintDots(".").Until(() => readyforwork).Background();

            yield return GetReady().Await();

            yield return WaitForever;
        }
This is an example of a function that executes a function (PrintDots) in the background(with the extension **.Background()**), while executing GetReady() synchroneously (**.Await()**). If at any point an error happens, whether it is inside the background function or not, the HandleFault (**.OnError()**) will be called.

        private IEnumerable<StreamState> PrintDots(string v)
        {
            while (true)
            {
                Console.Write(v);
                yield return Sleep(new Random().Next(10, 100));
            }
        }
Notice how the PrintDots function loops infinitely, and the **yield return**. This allows the "game-loop" to return and process some of the other running tasks. 

StreamThreads is based on Iterators and Extension Methods. As such, yield return is essentially used every time a new function is called - either as a background worker thread, or inline with statements. It is also worth noting that all functions should have a return type of **IEnumerable<StreamState\>**. This allows for it to be interpreted as an iterator by the compiler.

The extension methods also allow for easy ways to control what happens to a function when certain conditions arise. This can be used both with background and synchroneous functions.

            yield return DoSomething().Until(() => alertflag == true).Background();
            yield return DoSomething().While(() => goingwell == true).Await();
            yield return DoSomething().RestartOnError().Await();
            yield return DoSomething().ResumeOnError().Await();
            yield return ManualMode().SwitchOnCondition(() => auto == false);
            yield return WaitFor(() => backgroundready == true);
**RestartOnError** causes the function being called to start over. This can be helpful if we know an error can be fixed by re-initializing some local variables at the top of the function. **ResumeOnError** simply ignores the error and continues the loop. This could cause some problematic overhead if the error is severe.

**SwitchOnCondition** causes the function to exit entirely and start a new function. The calling function (parent function) will have no knowledge of this. This can be viewed as "switching state". Since background threads are local to each function, they are all cancelled when this happens. This also includes error handlers.

Finally we have the main loop that makes it all happen. From your form, on a timer, or just straight in a loop as below, define a **StreamState** variable to your function and add **Await()**. Then put a call to Loop every few milliseconds, or however frequent you want your loop to run.

    using StreamThreads;
    using static StreamThreads.StreamExtensions;

    StreamState state = DoSomething().Await();

    while (true)
    {    
        state.Loop();
        SecondsSinceLast = 0;
        Thread.Sleep(10);
    }
Lastly, the **SecondsSinceLast** is a **[ThreadStatic]** property that makes it easier during varying loop times to size time-dependent calculations.

	double Delta = SecondsSinceLast * speed;
