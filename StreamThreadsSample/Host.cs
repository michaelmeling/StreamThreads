using StreamThreads;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StreamThreads.StreamExtensions;

namespace StripeSample
{
    internal class Host
    {
        private bool readyforwork;


        public IEnumerable<StreamState> Flash()
        {
            var secondlightison = false;
            while (true)
            {
                Console.Write("O");
                yield return MakeNoise().Until(() => secondlightison).Background();

                yield return Sleep(500);

                Console.Write("_");
                secondlightison = true;
                yield return Sleep(500);
            }
        }

        public IEnumerable<StreamState> MakeNoise()
        {
            while (true)
            {
                Console.Write("x");
                yield return OK;
            }
        }

        public IEnumerable<StreamState> StartupState()
        {
            yield return HandleFault().OnError();

            yield return PrintDots(".").Until(() => readyforwork).Background();

            yield return GetReady().Background(out var ready);

            yield return ReturnNumbers().Background(out var ret);


            yield return ready.Await();
            Console.WriteLine($"GetReady returned {ready.Value}");
            Console.WriteLine($"Random number {ret.Value}");

            yield return WaitForever;
        }

        private IEnumerable<StreamState> GetReady()
        {
            bool cancel = false;
            yield return Background(c => AnAsyncProcess(c), () => cancel);

            yield return AnotherAsyncProcess().Background(out var number);
            yield return number.Await();
            Console.WriteLine($"AnotherAsyncProcess said : {number}");

            yield return MakeStuff(2, '#').Await();

            yield return MakeStuff(22, '?').Until(() => readyforwork).Background();

            yield return MakeStuff(4, '@').Await();

            cancel = true;

            yield return MakeStuff(6, '!').Await();

            readyforwork = true;

            yield return Return(5000);
        }

        private IEnumerable<StreamState> MakeStuff(int strength, char product)
        {
            Console.WriteLine($"\r\nNow Making:'{product}'");

            yield return PrintDots(product.ToString()).Background();

            for (int i = 0; i < strength; i++)
            {
                yield return Sleep(300);
                Console.Write(new String(product, i));
            }
        }

        private IEnumerable<StreamState> PrintDots(string v)
        {
            while (true)
            {
                Console.Write(v);
                yield return Sleep(new Random().Next(10, 100));
            }
        }

        private IEnumerable<StreamState> HandleFault()
        {
            Console.Write("Crashed");
            yield break;
        }

        private IEnumerable<StreamStateReturn<int>> ReturnNumbers()
        {
            var rnd = new Random();
            while (true)
            {
                yield return Return(rnd.Next(0, 100));
            }
        }

        private void AnAsyncProcess(CancellationToken token)
        {
            var rnd = new Random();
            for (int i = 0; i < 100000; i++)
            {
                Console.ForegroundColor = (ConsoleColor)rnd.Next(1, 15);

                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("color change cancelled");
                    return;
                }
            }
        }

        private async Task<int> AnotherAsyncProcess()
        {
            await Task.Delay(2300);

            Console.WriteLine("waited 2.3 secs");
            return 5;
        }
    }
}
