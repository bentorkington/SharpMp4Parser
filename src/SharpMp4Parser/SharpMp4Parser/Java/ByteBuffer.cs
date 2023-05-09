﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SharpMp4Parser.Java
{
    public class Buffer : Closeable
    {
        protected int _capacity = -1;

        protected MemoryStream _ms = null;

        public Buffer()
        {
            _ms = new MemoryStream();
            _capacity = _ms.Capacity;
        }

        public Buffer(int capacity)
        {
            _ms = new MemoryStream(capacity);
            _capacity = _ms.Capacity;
        }

        public Buffer(Buffer input)
        {
            _ms = input._ms;
            _capacity = _ms.Capacity;
        }

        public Buffer(MemoryStream input)
        {
            _ms = input;
            _capacity = _ms.Capacity;
        }

        public Buffer(byte[] input)
        {
            _ms = new MemoryStream(input);
            _capacity = _ms.Capacity;
        }

        public virtual Buffer duplicate()
        {
            return new Buffer(_ms.ToArray());
        }

        public int capacity()
        {
            return _capacity;
        }

        public byte[] array()
        {
            return _ms.ToArray();
        }

        public int remaining()
        {
            return (int)(_ms.Capacity - _ms.Position);
        }

        public int position()
        {
            return (int)_ms.Position;
        }

        public Buffer position(long position)
        {
            _ms.Position = Math.Min(position, _ms.Capacity);
            return this;
        }

        public void put(byte[] data)
        {
            int oldCapacity = _ms.Capacity;

            _ms.Write(data, 0, data.Length);

            if (_ms.Capacity > oldCapacity)
            {
                limit(oldCapacity);
            }
        }

        public void put(Buffer buffer)
        {
            byte[] data = new byte[buffer._ms.Capacity - buffer._ms.Position];
            buffer.read(data, 0, data.Length);
            put(data);
        }

        public virtual int write(Buffer value)
        {
            put(value);
            return (int)(value.toByteArray()).Length;
        }

        public virtual int write(byte[] value)
        {
            put(value);
            return (int)value.Length;
        }

        public virtual void write(byte[] value, int offset, int length)
        {
            put(value, offset, length);
        }

        public virtual void write(int value)
        {
            putInt(value);
        }

        public virtual void write(byte value)
        {
            put(value);
        }

        public virtual int read()
        {
            return get();
        }

        public virtual byte[] toByteArray()
        {
            return _ms.ToArray();
        }

        public int hashCode()
        {
            return GetHashCode();
        }

        public Buffer rewind()
        {
            _ms.Position = 0;
            return this;
        }

        public virtual long available()
        {
            // https://docs.oracle.com/javase/8/docs/api/java/io/InputStream.html
            // Returns an estimate of the number of bytes that can be read (or skipped over) from this input stream without blocking by the next invocation of a method for this input stream.
            return remaining();
        }

        internal bool hasRemaining()
        {
            return remaining() > 0;
        }

        public virtual void close()
        {
            _ms.Close();
        }

        public virtual bool isOpen()
        {
            return _ms.CanRead || _ms.CanWrite;
        }

        public int arrayOffset()
        {
            return position();
        }

        public string readLine(string encoding)
        {
            if ("UTF-8".CompareTo(encoding) == 0)
            {
                // utf8
                using (var sr = new StreamReader(_ms, Encoding.UTF8, false, 1, true))
                {
                    return sr.ReadLine();
                }
            }
            else
            {
                throw new NotSupportedException(encoding);
            }
        }

        public void writeShort(short value)
        {
            putShort(value);
        }

        public byte get()
        {
            return (byte)_ms.ReadByte();
        }

        public void put(byte value)
        {
            _ms.WriteByte(value);
        }

        public int limit()
        {
            return _ms.Capacity;
        }

        private byte[] _original = null;

        public Buffer limit(int limit)
        {
            if (this._original == null && this._ms.Length > 0)
            {
                this._original = this._ms.ToArray();
            }

            var oldpos = this._ms.Position;
            var oldMs = this._ms.ToArray();
            
            if(limit == this._capacity && this._original != null)
            {
                oldMs = this._original;
                this._original = null;
            }

            this._ms = new MemoryStream(limit);
            this._ms.Write(oldMs, 0, Math.Min(oldMs.Length, limit));

            this._ms.Position = Math.Min(oldpos, limit);
            return this;
        }

        public byte get(int i)
        {
            return _ms.ToArray()[i];
        }

        public virtual int read(byte[] bb)
        {
            return get(bb, 0, bb.Length);
        }

        public virtual int read(byte[] bytes, int offset, int length)
        {
            return get(bytes, offset, length);
        }

        public void get(byte[] bytes)
        {
            get(bytes, 0, bytes.Length);
        }

        internal int get(byte[] bytes, int offset, int length)
        {
            return _ms.Read(bytes, offset, length);
        }

        internal bool hasArray()
        {
            return true;
        }

        public void put(byte[] bytes, int offset, int length)
        {
            _ms.Write(bytes, offset, length);
        }

        public void put(int index, byte value)
        {
            _ms.Position = 0;
            _ms.WriteByte(value);
        }

        public char getChar()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return br.ReadChar();
            }
        }

        public void putChar(char value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms, Encoding.UTF8, true))
            {
                br.Write(value);
            }
        }

        public int getInt()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToInt32(br.ReadBytes(sizeof(Int32)).Reverse().ToArray(), 0);
                //return br.ReadInt32();
            }
        }

        public uint getUInt()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToUInt32(br.ReadBytes(sizeof(UInt32)).Reverse().ToArray(), 0);
            }
        }

        public uint getUIntLE()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return br.ReadUInt32();
            }
        }

        public void putInt(int value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms, Encoding.UTF8, true))
            {
                br.Write(value);
            }
        }

        public short getShort()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToInt16(br.ReadBytes(sizeof(Int16)).Reverse().ToArray(), 0);
                //return br.ReadInt16();
            }
        }

        public short getShortLE()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return br.ReadInt16();
            }
        }

        public ushort getUShort()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToUInt16(br.ReadBytes(sizeof(UInt16)).Reverse().ToArray(), 0);
                //return br.ReadInt16();
            }
        }

        public ushort getUShortLE()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return br.ReadUInt16();
            }
        }

        public void putShort(short value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms, Encoding.UTF8, true))
            {
                br.Write(value);
            }
        }

        public long getLong()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToInt64(br.ReadBytes(sizeof(Int64)).Reverse().ToArray(), 0);
                //return br.ReadInt64();
            }
        }

        public ulong getULong()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToUInt64(br.ReadBytes(sizeof(UInt64)).Reverse().ToArray(), 0);
                //return br.ReadInt64();
            }
        }

        public ulong getULongLE()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return br.ReadUInt64();
            }
        }

        public void putLong(long value)
        {
            using (BinaryWriter br = new BinaryWriter(_ms, Encoding.UTF8, true))
            {
                br.Write(value);
            }
        }

        public uint getUInt24()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToUInt32(br.ReadBytes(3).Reverse().ToArray(), 0);
                //return br.ReadInt64();
            }
        }

        public uint getUInt24LE()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToUInt32(br.ReadBytes(3).ToArray(), 0);
                //return br.ReadInt64();
            }
        }

        internal ulong getUInt48()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToUInt64(br.ReadBytes(8).Reverse().ToArray(), 0);
                //return br.ReadInt64();
            }
        }

        internal ulong getUInt48LE()
        {
            using (BinaryReader br = new BinaryReader(_ms, Encoding.UTF8, true))
            {
                return BitConverter.ToUInt64(br.ReadBytes(8).ToArray(), 0);
                //return br.ReadInt64();
            }
        }

        internal void order(ByteOrder endian)
        {
            if(endian == ByteOrder.BIG_ENDIAN)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class ByteBuffer : Buffer
    {
        public ByteBuffer() : base()
        { }

        public ByteBuffer(int capacity) : base(capacity)
        {  }

        public ByteBuffer(Java.Buffer input): base(input)
        {  }

        public ByteBuffer(MemoryStream input) : base(input)
        { }

        public ByteBuffer(byte[] input) : base(input)
        {  }

        public override Buffer duplicate()
        {
            return new ByteBuffer(new MemoryStream(_ms.ToArray()));
        }

        public ByteBuffer slice()
        {
            return new ByteBuffer(this._ms);
        }

        public static ByteBuffer wrap(byte[] bytes)
        {
            return wrap(bytes, 0, bytes.Length);
        }

        public static ByteBuffer wrap(byte[] bytes, int offset, int length)
        {
            return new ByteBuffer(new MemoryStream(bytes, offset, length));
        }

        public static ByteBuffer allocate(int bufferCapacity)
        {
            return new ByteBuffer(bufferCapacity);
        }

        public virtual int read(ByteBuffer bb)
        {
            byte[] rm = new byte[Math.Min(bb._ms.Capacity, bb._capacity) - bb.position()];
            int ret = read(rm, 0, rm.Length);
            bb.write(rm, 0, ret);
            if (ret == 0)
                return -1;
            else
                return ret;
        }

        public ByteBuffer reset()
        {
            position(0);
            return this;
        }

        public ByteBuffer position(int nextBufferWritePosition)
        {
            base.position(nextBufferWritePosition);
            return this;
        }
    }

    public class BufferedReader
    {
        private InputStreamReader inputStreamReader;

        public BufferedReader(InputStreamReader inputStreamReader)
        {
            this.inputStreamReader = inputStreamReader;
        }

        internal string readLine()
        {
            return inputStreamReader.readLine();
        }
    }

    public class InputStreamReader
    {
        private ByteArrayInputStream input;
        private string encoding;

        public InputStreamReader(ByteArrayInputStream input, string encoding)
        {
            this.input = input;
            this.encoding = encoding;
        }

        internal string readLine()
        {
            return input.readLine(encoding);
        }
    }

    public static class Channels
    {
        public static WritableByteChannel newChannel(ByteArrayOutputStream outputStream)
        {
            return outputStream;
        }

        public static ReadableByteChannel newChannel(ByteArrayInputStream inputStream)
        {
            return inputStream;
        }

        public static ByteArrayInputStream newInputStream(ReadableByteChannel dataSource)
        {
            return new ByteArrayInputStream(dataSource);
        }
    }

    public class InputStream : ByteBuffer
    {
        public InputStream()
        { }

        public InputStream(byte[] input) : base(input)
        {

        }

        public InputStream(Java.Buffer input) : base(input)
        { }
    }

    public class OutputStream : ByteBuffer
    {
        public OutputStream()
        { }

        public OutputStream(byte[] input) : base(input)
        {

        }

        public OutputStream(Java.Buffer input) : base(input)
        { }
    }

    public class MappedByteBuffer : ByteBuffer
    {

    }

    public class WritableByteChannel : OutputStream
    {

        public WritableByteChannel()
        {
            
        }

        public WritableByteChannel(ByteBuffer output) : base(output)
        {
            
        }
    }

    public class ReadableByteChannel : InputStream
    {
        public ReadableByteChannel()
        {
            
        }

        public ReadableByteChannel(byte[] input) : base(input)
        {

        }

        public ReadableByteChannel(Java.Buffer input) : base(input)
        {
            
        }
    }

    public class ByteArrayOutputStream : WritableByteChannel
    {
        public ByteArrayOutputStream()
        {

        }

        public ByteArrayOutputStream(ByteArrayOutputStream output) : base(output)
        {

        }
    }

    public class DataOutputStream : ByteArrayOutputStream
    {
        public DataOutputStream(ByteArrayOutputStream output) : base(output)
        {

        }
    }

    public class ByteArrayInputStream : ReadableByteChannel
    {
        public ByteArrayInputStream(byte[] input) : base(input)
        {
            
        }

        public ByteArrayInputStream(Java.Buffer input) : base(input)
        {
        }
    }


    public class FilterInputStream : ByteArrayInputStream
    {
        public FilterInputStream(byte[] input) : base(input)
        {
        }

        public FilterInputStream(Java.Buffer input) : base(input)
        {
        }
    }

}
