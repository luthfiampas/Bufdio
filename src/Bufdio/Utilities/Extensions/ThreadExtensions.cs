using System.Threading;

namespace Bufdio.Utilities.Extensions
{
    internal static class ThreadExtensions
    {
        public static void EnsureThreadDone(this Thread thread)
        {
            while (thread.IsAlive)
            {
                Thread.Sleep(10);
            }
        }
    }
}
