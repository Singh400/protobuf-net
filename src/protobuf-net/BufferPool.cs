using System;
using System.Buffers;

namespace ProtoBuf
{
    internal sealed class BufferPool
    {
        private const int PoolSize = 20;
        private const int BufferLength = 1024;
        private static readonly ArrayPool<byte> _arrayPool;

        static BufferPool()
        {
            _arrayPool = ArrayPool<byte>.Create(maxArrayLength: BufferLength * BufferLength * BufferLength, maxArraysPerBucket: PoolSize);
        }

        internal static void Flush()
        {
            throw new NotImplementedException(); // Not sure how to flush the existing _arrayPool - overwrite reference?
        }

        internal static byte[] GetBuffer()
        {
            return _arrayPool.Rent(BufferLength);
        }

        internal static void ResizeAndFlushLeft(ref byte[] buffer, int toFitAtLeastBytes, int copyFromIndex, int copyBytes)
        {
            Helpers.DebugAssert(buffer != null);
            Helpers.DebugAssert(toFitAtLeastBytes > buffer.Length);
            Helpers.DebugAssert(copyFromIndex >= 0);
            Helpers.DebugAssert(copyBytes >= 0);

            // try doubling, else match
            int newLength = buffer.Length * 2;

            if (newLength < toFitAtLeastBytes)
            {
                newLength = toFitAtLeastBytes;
            }

            if (copyBytes == 0)
            {
                ReleaseBufferToPool(ref buffer); // No need to copy, we can release immediately
            }

            byte[] newBuffer = _arrayPool.Rent(newLength);

            if (copyBytes > 0)
            {
                Helpers.BlockCopy(buffer, copyFromIndex, newBuffer, 0, copyBytes);
                ReleaseBufferToPool(ref buffer);
            }

            buffer = newBuffer;
        }

        internal static void ReleaseBufferToPool(ref byte[] buffer)
        {
            _arrayPool.Return(array: buffer, clearArray: true);
        }
    }
}