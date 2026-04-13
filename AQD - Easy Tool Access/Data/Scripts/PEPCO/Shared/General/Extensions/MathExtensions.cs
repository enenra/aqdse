using System;

namespace RichHudFramework
{
    public static class MathExtensions
    {
        /// <summary>
        /// Rounds a double-precision floating-point value to a specified number of fractional 
        /// digits, and rounds midpoint values to the nearest even number.
        /// </summary>
        public static double Round(this double value, int digits = 0) =>
            Math.Round(value, digits);

        /// <summary>
        /// Rounds a single-precision floating-point value to a specified number of fractional 
        /// digits, and rounds midpoint values to the nearest even number.
        /// </summary>
        public static float Round(this float value, int digits = 0) =>
            (float)Math.Round(value, digits);

        /// <summary>
        /// Returns the absolute value of a single-precision floating-point number.
        /// </summary>
        public static float Abs(this float value) =>
            Math.Abs(value);

        /// <summary>
        /// Converts a floating point value given in radians to an fp value in degrees.
        /// </summary>
        public static float RadiansToDegrees(this float value) =>
            (value / (float)Math.PI) * 180f;

        /// <summary>
        /// Converts a floating point value given in degrees to an fp value in radians.
        /// </summary>
        public static float DegreesToRadians(this float value) =>
            (value * (float)Math.PI) / 180f;
    }
}
