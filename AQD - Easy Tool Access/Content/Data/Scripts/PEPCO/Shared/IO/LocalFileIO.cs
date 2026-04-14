using Sandbox.ModAPI;
using System;
using System.IO;

namespace RichHudFramework.IO
{
    /// <summary>
    /// Handles basic file I/O operations in local storage. Will not allow multiple threads to operate on the same file object concurrently.
    /// </summary>
    public class LocalFileIO
    {
        public bool FileExists => MyAPIGateway.Utilities.FileExistsInLocalStorage(file, typeof(LocalFileIO));
        public readonly string file;
        private readonly object fileLock;

        public LocalFileIO(string file)
        {
            this.file = file;
            fileLock = new object();
        }

        /// <summary>
        /// Creates a local duplicate of a file with a given name.
        /// </summary>
        public KnownException TryDuplicate(string newName)
        {
            string data;
            KnownException exception = TryRead(out data);
            LocalFileIO newFile;

            if (exception == null && data != null)
            {
                newFile = new LocalFileIO(newName);
                exception = newFile.TryWrite(data);
            }

            return exception;
        }

        /// <summary>
        /// Attempts to append string to an existing local file.
        /// </summary>
        public KnownException TryAppend(string data)
        {
            string current;
            KnownException exception = TryRead(out current);

            if (exception == null && current != null)
            {
                current += data;
                exception = TryWrite(current);
            }
            else
                exception = TryWrite(data);

            return exception;
        }

        /// <summary>
        /// Attempts to retrieve the file data as a byte array. Requires data stream to begin with array size.
        /// </summary>
        public KnownException TryRead(out byte[] stream)
        {
            KnownException exception = null;
            BinaryReader reader = null;
            
            lock (fileLock)
            {
                try
                {
                    reader = MyAPIGateway.Utilities.ReadBinaryFileInLocalStorage(file, typeof(LocalFileIO));
                    stream = reader.ReadBytes(reader.ReadInt32());
                }
                catch (Exception e)
                {
                    stream = null;
                    exception = new KnownException($"IO Error. Unable to read from {file}.", e);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }

            return exception;
        }

        /// <summary>
        /// Attempts to retrieve the file data as a string.
        /// </summary>
        public KnownException TryRead(out string data)
        {
            KnownException exception = null;
            TextReader reader = null;
            data = null;

            lock (fileLock)
            {
                try
                {
                    reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(file, typeof(LocalFileIO));
                    data = reader.ReadToEnd();
                }
                catch (Exception e)
                {
                    data = null;
                    exception = new KnownException($"IO Error. Unable to read from {file}.", e);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }

            return exception;
        }

        /// <summary>
        /// Attempts to write a byte array to the file. Byte array size will be prepended to the data stream.
        /// </summary>
        public KnownException TryWrite(byte[] stream)
        {
            KnownException exception = null;
            BinaryWriter writer = null;

            lock (fileLock)
            {
                try
                {
                    writer = MyAPIGateway.Utilities.WriteBinaryFileInLocalStorage(file, typeof(LocalFileIO));
                    writer.Write(stream.Length);
                    writer.Write(stream);
                    writer.Flush();
                }
                catch (Exception e)
                {
                    exception = new KnownException($"IO Error. Unable to write to {file}.", e);
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }
            }

            return exception;
        }

        /// <summary>
        /// Attempts to write a string to the file.
        /// </summary>
        public KnownException TryWrite(string data)
        {
            KnownException exception = null;
            TextWriter writer = null;

            lock (fileLock)
            {
                try
                {
                    writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(file, typeof(LocalFileIO));
                    writer.Write(data);
                    writer.Flush();
                }
                catch (Exception e)
                {
                    exception = new KnownException($"IO Error. Unable to write to {file}.", e);
                }
                finally
                {
                    if (writer != null)
                        writer.Close();
                }
            }

            return exception;
        }
    }
}
