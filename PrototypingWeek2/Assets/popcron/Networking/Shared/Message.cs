using System;
using System.IO;
using System.Net;

namespace Popcron.Networking
{
    public class Message
    {
        public ushort Type
        {
            get
            {
                return type;
            }
        }

        private byte reads;
        private byte writes;
        private readonly ushort type;
        private readonly byte[] data;
        private readonly bool canWrite = false;
        private readonly bool canRead = false;
        private MemoryStream streamReader;
        private BinaryReader binaryReader;
        private MemoryStream streamWriter;
        private BinaryWriter binaryWriter;

        public byte[] Data
        {
            get
            {
                if (canWrite)
                {
                    return streamWriter.ToArray();
                }
                else if (canRead)
                {
                    return streamReader.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }

        private ushort GetTypeFromEnum(Enum e)
        {
            object val = Convert.ChangeType(e, e.GetTypeCode());
            try
            {
                if (val is byte byteValue) return byteValue;
                if (val is int intValue) return (ushort)intValue;
                if (val is uint uIntValue) return (ushort)uIntValue;
                if (val is short shortValue) return (ushort)shortValue;
                if (val is ushort uShortValue) return uShortValue;
            }
            catch (Exception exception)
            {
                throw exception;
            }

            throw new Exception("Could not convert " + e + " to ushort integral type.");
        }

        private Message()
        {

        }

        public Message(Enum e)
        {
            canWrite = true;
            canRead = false;
            
            this.type = GetTypeFromEnum(e);

            streamWriter = new MemoryStream();
            binaryWriter = new BinaryWriter(streamWriter);
        }

        public Message(ushort type)
        {
            canWrite = true;
            canRead = false;

            this.type = type;

            streamWriter = new MemoryStream();
            binaryWriter = new BinaryWriter(streamWriter);
        }

        public Message(Enum e, byte[] data)
        {
            canWrite = false;
            canRead = true;
            
            this.type = GetTypeFromEnum(e);
            this.data = data;

            streamReader = new MemoryStream(data);
            binaryReader = new BinaryReader(streamReader);
        }

        public Message(ushort type, byte[] data)
        {
            canWrite = false;
            canRead = true;

            this.type = type;
            this.data = data;

            streamReader = new MemoryStream(data);
            binaryReader = new BinaryReader(streamReader);
        }

        public byte[] ReadBytes(int count)
        {
            if (!canRead) throw new NoReadAccessException("Message was not initialized to be read from.");
            
            reads++;
            return binaryReader.ReadBytes(count);
        }

        public T Read<T>()
        {
            if (!canRead) throw new NoReadAccessException("Message was not initialized to be read from.");

            object value = null;
            if (typeof(T) == typeof(string))
            {
                reads++;
                value = binaryReader.ReadString();
            }
            else if (typeof(T) == typeof(float))
            {
                reads++;
                value = binaryReader.ReadSingle();
            }
            else if (typeof(T) == typeof(double))
            {
                reads++;
                value = binaryReader.ReadDouble();
            }
            else if (typeof(T) == typeof(short))
            {
                reads++;
                value = binaryReader.ReadInt16();
            }
            else if (typeof(T) == typeof(ushort))
            {
                reads++;
                value = binaryReader.ReadUInt16();
            }
            else if (typeof(T) == typeof(int))
            {
                reads++;
                value = binaryReader.ReadInt32();
            }
            else if (typeof(T) == typeof(uint))
            {
                reads++;
                value = binaryReader.ReadUInt32();
            }
            else if (typeof(T) == typeof(long))
            {
                reads++;
                value = binaryReader.ReadInt64();
            }
            else if (typeof(T) == typeof(ulong))
            {
                reads++;
                value = binaryReader.ReadUInt64();
            }
            else if (typeof(T) == typeof(byte))
            {
                reads++;
                value = binaryReader.ReadByte();
            }
            else if (typeof(T) == typeof(bool))
            {
                reads++;
                value = binaryReader.ReadBoolean();
            }
            else if (typeof(T).IsEnum)
            {
                reads++;
                value = Enum.ToObject(typeof(T), binaryReader.ReadByte());
            }

            if (value != null)
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            else
            {
                throw new Exception(typeof(T) + " is not implemented to be read.");
            }
        }

        public void Write(float value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(ushort value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(short value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(int value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(uint value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(long value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(ulong value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(byte value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(bool value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(string value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Write(byte[] value)
        {
            if (!canWrite) throw new NoWriteAccessException("Message was not initialized to be written to.");

            writes++;
            binaryWriter.Write(value);
        }

        public void Rewind()
        {
            reads = 0;

            streamReader = new MemoryStream(data);
            binaryReader = new BinaryReader(streamReader);
        }

        public void Send()
        {
            Network.Send(this);
        }

        public void Send(NetworkConnection connection)
        {
            if (connection == null)
            {
                Network.Send(this);
            }
            else
            {
                Network.Send(connection, this);
            }
        }
    }
}