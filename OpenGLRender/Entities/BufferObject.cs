using Silk.NET.OpenGL;
using System;

namespace ParkSimulator
{
    public class BufferObject<TDataType> : IDisposable where TDataType : unmanaged
    {
        uint handle;
        BufferTargetARB type;
        GL openGL;

        public unsafe BufferObject(GL gl, Span<TDataType> data, BufferTargetARB bufferType)
        {
            openGL = gl;
            type = bufferType;

            handle = openGL.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                openGL.BufferData(type, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
        }

        public void Bind()
        {
            openGL.BindBuffer(type, handle);
        }

        public void Dispose()
        {
            openGL.DeleteBuffer(handle);
        }
    }
}
