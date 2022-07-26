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

StreamThreads is based on Iterators and Extension Methods. As such, yield return is essentially used every time a new function is called - either as a background worker thread, or inline with statements. It is also worth noting that all function should have return type of IEnumerable<StreamState>. This allows for it to be interpreted as an iterator by the compiler.