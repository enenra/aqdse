using VRageMath;

namespace RichHudFramework
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a <see cref="Vector2"/> to a <see cref="Vector2D"/>
        /// </summary>
        public static Vector2D ToDouble(this Vector2 vec) =>
            new Vector2D(vec.X, vec.Y);

        /// <summary>
        /// Converts a <see cref="Vector2D"/> to a <see cref="Vector2"/>
        /// </summary>
        public static Vector2 ToSingle(this Vector2D vec) =>
            new Vector2((float)vec.X, (float)vec.Y);

        /// <summary>
        /// Calculates the alpha of the color based on a float value between 0 and 1 and returns the new color.
        /// </summary>
        public static Color SetAlphaPct(this Color color, float alphaPercent) =>
            new Color(color.R, color.G, color.B, (byte)(alphaPercent * 255f));

        /// <summary>
        /// Retrieves the channel of a given <see cref="Color"/> by its index. R = 0, G = 1, B = 2, A = 3.
        /// </summary>
        public static byte GetChannel(this Color color, int channel)
        {
            switch (channel)
            {
                case 0:
                    return color.R;
                case 1:
                    return color.G;
                case 2:
                    return color.B;
                case 3:
                    return color.A;
            }

            return 0;
        }

        /// <summary>
        /// Sets the channel of a given <see cref="Color"/> by its index to the given value. R = 0, G = 1, B = 2, A = 3.
        /// </summary>
        public static Color SetChannel(this Color color, int channel, byte value)
        {
            switch(channel)
            {
                case 0:
                    color.R = value;
                    break;
                case 1:
                    color.G = value;
                    break;
                case 2:
                    color.B = value;
                    break;
                case 3:
                    color.A = value;
                    break;
            }

            return color;
        }
    }
}
