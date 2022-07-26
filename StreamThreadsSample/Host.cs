using StreamThreads;
using System;
using System.Collections.Generic;
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
                yield return MakeNoise().Until(()=> secondlightison).Background();

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

            yield return GetReady().Await();

            yield return WaitForever;
        }

        private IEnumerable<StreamState> GetReady()
        {
            yield return MakeStuff(2, '#').Await();

            yield return MakeStuff(22, '?').Until(() => readyforwork).Background();

            yield return MakeStuff(4, '@').Await();

            yield return MakeStuff(6, '!').Await();

            readyforwork = true;
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
    }
}
