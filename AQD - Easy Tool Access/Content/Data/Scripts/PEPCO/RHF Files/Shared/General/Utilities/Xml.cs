using Sandbox.ModAPI;
using System;

namespace RichHudFramework
{
    public static partial class Utils
    {
        public static class Xml
        {
            /// <summary>
            /// Attempts to serialize an object to an Xml string.
            /// </summary>
            public static KnownException TrySerialize<T>(T obj, out string xmlOut)
            {
                KnownException exception = null;
                xmlOut = null;

                try
                {
                    xmlOut = MyAPIGateway.Utilities.SerializeToXML(obj);
                }
                catch (Exception e)
                {
                    exception = new KnownException("IO Error. Failed to generate XML.", e);
                }

                return exception;
            }

            /// <summary>
            /// Attempts to deserialize an Xml string to an object of a given type.
            /// </summary>
            public static KnownException TryDeserialize<T>(string xmlIn, out T obj)
            {
                KnownException exception = null;
                obj = default(T);

                try
                {
                    obj = MyAPIGateway.Utilities.SerializeFromXML<T>(xmlIn);
                }
                catch (Exception e)
                {
                    exception = new KnownException("IO Error. Unable to interpret XML.", e);
                }

                return exception;
            }
        }
    }
}
