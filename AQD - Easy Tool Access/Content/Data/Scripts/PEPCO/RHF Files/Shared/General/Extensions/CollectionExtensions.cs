using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Generates subarray that starts from a given index and continues to the end.
        /// </summary>
        public static T[] GetSubarray<T>(this T[] arr, int start)
        {
            T[] trimmed = new T[arr.Length - start];

            for (int n = start; n < arr.Length; n++)
                trimmed[n - start] = arr[n];

            return trimmed;
        }

        /// <summary>
        /// Generates subarray that starts from a given index and continues to the end.
        /// </summary>
        public static T[] GetSubarray<T>(this T[] arr, int start, int end)
        {
            T[] trimmed;

            end = MathHelper.Clamp(end, 0, arr.Length);
            trimmed = new T[end - start];

            for (int n = start; n < end; n++)
                trimmed[n - start] = arr[n];

            return trimmed;
        }

        /// <summary>
        /// Returns an array containing only unique entries from the collection.
        /// </summary>
        public static T[] GetUnique<T>(this IReadOnlyList<T> original)
        {
            var unique = new List<T>(original.Count);

            foreach (T item in original)
            {
                if (!unique.Contains(item))
                    unique.Add(item);
            }

            return unique.ToArray();
        }
    }
}
