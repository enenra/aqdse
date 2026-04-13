using System;
using System.Text.RegularExpressions;

namespace RichHudFramework
{
    public static partial class Utils
    {
        public static class Color
        {
            private static readonly Regex colorParser = new Regex(@"(\s*,?(\d{1,3})\s*,?){3,4}");

            /// <summary>
            /// Determines whether a string can be parsed into a <see cref="VRageMath.Color"/> and returns true if so.
            /// </summary>
            public static bool CanParseColor(string colorData)
            {
                Match match = colorParser.Match(colorData);
                CaptureCollection captures = match.Groups[2].Captures;
                byte r, g, b, a;

                if (captures.Count > 2)
                {
                    if (!byte.TryParse(captures[0].Value, out r))
                        return false;

                    if (!byte.TryParse(captures[1].Value, out g))
                        return false;

                    if (!byte.TryParse(captures[2].Value, out b))
                        return false;

                    if (captures.Count > 3)
                    {
                        if (!byte.TryParse(captures[3].Value, out a))
                            return false;
                    }

                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Tries to convert a string of color values to its <see cref="VRageMath.Color"/> equivalent.
            /// </summary>
            public static bool TryParseColor(string colorData, out VRageMath.Color value, bool ignoreAlpha = false)
            {
                bool successful;

                try
                {
                    value = ParseColor(colorData, ignoreAlpha);
                    successful = true;
                }
                catch
                {
                    value = VRageMath.Color.White;
                    successful = false;
                }

                return successful;
            }

            /// <summary>
            /// Converts a string of color values to its <see cref="VRageMath.Color"/> equivalent.
            /// </summary>
            public static VRageMath.Color ParseColor(string colorData, bool ignoreAlpha = false)
            {
                Match match = colorParser.Match(colorData);
                CaptureCollection captures = match.Groups[2].Captures;
                VRageMath.Color value = new VRageMath.Color();

                if (captures.Count > 2)
                {
                    value.R = byte.Parse(captures[0].Value);
                    value.G = byte.Parse(captures[1].Value);
                    value.B = byte.Parse(captures[2].Value);

                    if (captures.Count > 3 || ignoreAlpha)
                        value.A = byte.Parse(captures[3].Value);
                    else
                        value.A = 255;

                    return value;
                }
                else
                    throw new Exception("Color string must contain at least 3 values.");
            }

            public static string GetColorString(VRageMath.Color color, bool includeAlpha = true)
            {
                if (includeAlpha)
                    return $"{color.R},{color.G},{color.B},{color.A}";
                else
                    return $"{color.R},{color.G},{color.B}";
            }
        }
    }
}
