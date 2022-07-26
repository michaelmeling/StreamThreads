
# StreamThreads
**Single Threaded Recursive Iterators for Parallel Execution**

Project URL: https://github.com/michaelmeling/StreamThreads
## Introduction
StreamThreads is a coroutine library that allows you to write clean and plain code that can execute processes in parallel, but using only a single thread. It is an alternative to async/await and Task.Run(), but without locks, concurrent collections and synchronization. In some sense, it is more like a game-loop where each object in the scene needs updating every few milliseconds before the screen refreshes. Unfortunately, game-loops often end up with significant amounts of global status variables and case-statements in complex scenes. StreamThreads helps by allowing a game-loop to be written as a "multi-threaded" application, where each function is executing independently.

### When to use Co-Routines instead of async/await and Task.Run
There are many ways to run tasks in parallel, and co-routines is just one of them. Each have their own strengths and weaknesses and choosing the right technology can simplify the structure of the code.

**Co-routines** work well in scenarios with lots of small parallel tasks working on shared data.
 - **use cases**: games, robotics,  industrial automation, animated user interfaces and communications

**Async/Await** was originally invented to allow single threaded user interfaces to process long duration tasks without "hanging" by blocking the main GUI thread. If GUIs were multi-threaded, async would not be needed. 
 - **use cases**: WPF, Windows Forms, communications, web pages

**Task** object is used to run a background thread and can be used with Async/Await. However, this often requires various locking mechanisms to synchronize access to shared data. For this reason, Task is mostly used for code modules that are decoupled and rarely interact.
 - **use cases**: Servers, data crunching, performance
 
### Difference from Unity Co-Routines
The Unity game engine also makes use of Co-Routines by calling the StartCoroutine() method, and StreamThreads basically provides the same functionality, albeit using a slightly different syntax.

 - **Error handling**. Using *ResumeOnError*, *RestartOnError*, *ExitOnError* and setting a custom *ErrorHandler*
 - **Scope Isolation**. Each function in StreamThreads has is own scope of background coroutines and error handlers that go out of scope when the function exits
 - **Return values**. By adding an output variable in *Await(out var returnvar)* or *Background(out var retval)*, and *using yield return Return("some return value")* it is possible to return results from coroutines.
 - **Generics**. StreamThreads provides generic versions for *IEnumerable<T\>*, such as *Await<T\>()* and *Background<T\>(out T retval)*.
 - **Chaining**. Because StreamThreads is based on extension methods, it uses IEnumerable chaining. An example could be *DoSomething().Until(()=>flag==true).ResumeOnError().Await()*

## Starting coroutines
Coroutines can be started in the foreground (normal serial excution) or in the background. Each function can spawn as many background "threads" as it wants, and these will terminate as soon as the function goes out of scope, either by simply exiting or calling **yield break**.

StreamThreads uses two methods for starting coroutines:
 - **Await()** - in the foreground (serial execution)
 - **Background()** - in the background (parallel execution)

Below is an example of a function that executes a function (PrintDots) in the background(with the extension **.Background()**), while executing GetReady() synchroneously (**.Await()**). In the mean time, if at any point an error happens, whether it is inside the background function or not, the HandleFault (**.OnError()**) will be called.

```cs
public IEnumerable<StreamState> StartupState()
{
    yield return HandleFault().OnError();

    yield return PrintDots(".").Until(() => readyforwork).Background();

    yield return GetReady().Await();

    yield return WaitForever;
}
```

Notice how the PrintDots function loops infinitely, and the **yield return**. This allows the "game-loop" to return and process some of the other running tasks. 

```cs
private IEnumerable<StreamState> PrintDots(string v)
{
    while (true)
    {
        Console.Write(v);
        yield return Sleep(new Random().Next(10, 100));
    }
}
```

## Iterators and yield return
StreamThreads is based on Iterators and Extension Methods. As such, yield return is essentially used every time a new function is called - either as a background worker thread, or inline with statements. It is also worth noting that all functions should have a return type of **IEnumerable<StreamState\>**. This allows for it to be interpreted as an iterator by the compiler.

The extension methods also allow for easy ways to control what happens to a function when certain conditions arise. This can be used both with background and synchroneous functions.

```cs
yield return DoSomething().Until(() => alertflag == true).Background();
yield return DoSomething().While(() => goingwell == true).Await();
yield return DoSomething().RestartOnError().Await();
yield return DoSomething().ResumeOnError().Await();
yield return ManualMode().SwitchOnCondition(() => auto == false);
yield return WaitFor(() => backgroundready == true);
```

**RestartOnError** causes the function being called to start over. This can be helpful if we know an error can be fixed by re-initializing some local variables at the top of the function. **ResumeOnError** simply ignores the error and continues the loop. This could cause some problematic overhead if the error is severe.

**SwitchOnCondition** causes the function to exit entirely and start a new function. The calling function (parent function) will have no knowledge of this. This can be viewed as "switching state". Since background threads are local to each function, they are all cancelled when this happens. This also includes error handlers.

## async / await
It is also possible to call **async** functions, as if they were part of the normal StreamThreads execution. Be aware, that these calls will not be terminated when they go out of scope. They live forever, or at least until they stop running by themselves. However, they do run in a separate thread, and so will be "true" multi-threading. Again, they can be called either as **Background()** or **Await()**, but lack the ability to use **Until()**, **While()** or any other iterator based chaining.

```cs
yield return DelayForSomeTimeAsync().Await(out var number);
Console.WriteLine($"Returned value was {number.Value}");
    
private async Task<int> DelayForSomeTimeAsync()
{
    await Task.Delay(2300);

    Console.WriteLine("waited 2.3 secs");
    return 5;
}
```

Alternatively, there is also a **Lambda** version available. which includes a **CancellationToken**.

```cs
yield return Background(c => AnAsyncProcess(c), () => cancel);
    
private void AnAsyncProcess(CancellationToken token)
{
        ... do some time consuming stuff
            
        if (token.IsCancellationRequested)
        {
            return;
        }
}
```

## Calling the loop
Finally we have the main loop that makes it all happen. From your form, on a timer, or just straight in a loop as below, define a **StreamState** variable to your function and add **Await()**. Then put a call to **Loop** every few milliseconds, or however frequent you want your loop to run. Notice how **Loop** somewhat counter-intuitively returns true when it should no longer be called.

```cs
using StreamThreads;
using static StreamThreads.StreamExtensions;

StreamState state = DoSomething().Await();

while (true)
{    
    if(state.Loop()) break;
    SecondsSinceLast = 0;
    Thread.Sleep(10);
}
```

## Timing
Lastly, the **SecondsSinceLast** is a **[ThreadStatic]** property that makes it easier during varying loop times to size time-dependent calculations.

```cs
using static StreamThreads.StreamExtensions;
double Delta = SecondsSinceLast * speed;
```

Don't forget to include the **using static** statement if you want to use **SecondsSinceLast**, **WaitForever**, **OK**, **WaitFor** and other static properties and methods from the StreamThreads library.

## Return values
Return variables and ref parameters are not allowed for iterators, so as a solution StreamThreads contains a small wrapper class **IteratorReturnVariable**, that can be passed as a parameter and populated by the called function.

```cs
IteratorReturnVariable<int> myrefvar = new ();
yield return DoSomething(myrefvar).Await();
Console.WriteLine(myrefvar);
```

It is also possible for a function to return a value.

```cs
yield return GetReady().Background(out var ready);
...
Console.WriteLine(ready.Value);
            
private IEnumerable<StreamState> GetReady()
{
	yield return Return(5000);
}
```

## Generic functions
StreamThreads includes generic versions of Await() and Background() which allows for easier type casting.

```cs
...
yield return ReturnNumbers().Background(out var ret);
...
            
private IEnumerable<StreamState<int>> ReturnNumbers()
{
	var rnd = new Random();
   while (true)
   {
	    yield return Return(rnd.Next(0, 100));
   }
}
```

Notice the IEnumerable<StreamState<**int**\>>.

## Return variable as status monitor
In addition to the return value itself, the **IteratorReturnVariable** contains a few fields for monitoring a background tasks status, such as **Await()**, **HasEnded**, **WasTerminated**, **IsRunning** and **Faulted**. 

```cs
yield return ready.Await();

// alternatively
yield return WaitFor(() => ready.HasValue());

yield return WaitFor(() => !ready.IsRunning());
```

## Performance
At a very rough estimate, each function takes 1 tick, or in other words 1ms per 10,000 threads. This corresponds to a frame rate of appx 20fps for 500,000 threads. This was tested on a recursive  function with background processes, similar to a tree structure with a 1000 layers. The deeper the call hierarchy, the lower the performance due to StreamThreads having to recurse the call stack on every loop. Bear in mind, this does not include your code, it only includes the overhead of StreamThreads' internal house keeping.

## Samples
This screen shows the sample program running a number of threads simultaneously, where some threads print a different character, others change the color, and a few print out computational results.
![This screen shows the sample program running a number of threads simultaneously, where each thread prints a different character.](https://github.com/michaelmeling/StreamThreads/blob/master/sampleimg.PNG?raw=true)

This image is from the WPF sample of recursive coroutines, showing small boxes slowly spawning on the screen, each doing their own thing.
![This image is from the WPF sample of recursive coroutines, showing small boxes slowly spawning on the screen, each doing their own thing.](https://github.com/michaelmeling/StreamThreads/blob/master/sampleimgwpf.PNG?raw=true)
