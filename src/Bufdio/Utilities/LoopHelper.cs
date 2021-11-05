using System;

namespace Bufdio.Utilities
{
    internal static class LoopHelper
    {
        public static void While(Func<bool> condition, Func<bool> breaker, Action action = default)
        {
            Ensure.NotNull(condition, nameof(condition));
            Ensure.NotNull(breaker, nameof(breaker));

            while (condition())
            {
                if (breaker())
                {
                    break;
                }

                action?.Invoke();
            }
        }
    }
}
