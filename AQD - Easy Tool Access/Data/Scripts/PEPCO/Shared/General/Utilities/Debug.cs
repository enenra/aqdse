using System;

namespace RichHudFramework
{
    public static partial class Utils
    {
        public static class Debug
        {
            public static void AssertNotNull<T>(T obj, string message = "")
            {
                Assert(obj != null, $"Object of type {typeof(T).ToString()} is null. " + message);
            }

            public static void Assert(bool condition, string message = "")
            {
                if (!condition)
                    throw new Exception("Assertion failed. " + message);
            }
        }
    }
}
