using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public class PictureRender : Render
    {
        int outputWidth;
        int outputHeight;
        string outputPath;

        public struct Color24
        {
            public byte r;
            public byte b;
            public byte g;

            public Color24(Vector3 v)
            {
                r = (byte)(v.X * 255);
                g = (byte)(v.Y * 255);
                b = (byte)(v.Z * 255);
            }
        }

        Color24 [,]? outputBuffer;
        Color24 clearColor;

        int frameNumber = 0;

        public PictureRender()
        {
            outputPath = "";
            clearColor = new Color24();
        }

        public override void Init(Config config)
        {
            outputWidth = config.GetIntValue("renderOuputWidth", 320);
            outputHeight = config.GetIntValue("renderOutputHeight", 200);
            outputPath = config.GetTextValue("renderOutputPath", "Output\\");

            clearColor.r = (byte)config.GetIntValue("renderClearColorR", 120);
            clearColor.g = (byte)config.GetIntValue("renderClearColorG", 255);
            clearColor.b = (byte)config.GetIntValue("renderClearColorB", 120);

            outputBuffer = new Color24[outputWidth, outputHeight];

            Clear();

        }

        public override void Finish()
        {
        }

        public void DrawRect(Vector2 position, Vector2 size, Color24 color)
        {
            Debug.Assert(outputBuffer != null, "Subsistema no inicializado");

            for(int y = 0; y < (int)size.Y; y ++)
            {
                for(int x = 0; x < (int)size.X; x ++)
                {
                    int absX = outputWidth / 2 + (int)position.X + x;
                    int absY = outputHeight / 2 - ((int)position.Y + y);

                    if(absX >= 0 && absX <= outputBuffer.GetLength(0) - 1 &&
                       absY >= 0 && absY <= outputBuffer.GetLength(1) - 1)
                    {
                        outputBuffer[absX, absY] = color;
                    }
                }

            }
        }

        public void DrawLine(Vector2 position1, Vector2 position2, Color24 color)
        {
            Debug.Assert(outputBuffer != null, "Subsistema no inicializado");

            float width = MathF.Abs(position2.X - position1.X);
            float height = MathF.Abs(position2.Y - position1.Y);
            int steps = (int)MathF.Max(width, height);
            float stepXWidth = width / steps;
            float stepYHeight = height / steps;
            float stepXSign = MathF.Sign(position2.X - position1.X);
            float stepYSign = MathF.Sign(position2.Y - position1.Y);

            for(int i = 0; i < steps; i ++)
            {
                int absX = (int)(outputWidth / 2 + (int)position1.X + i * stepXWidth * stepXSign);
                int absY = (int)(outputHeight / 2 - ((int)position1.Y + i * stepYHeight * stepYSign));

                if(absX >= 0 && absX <= outputBuffer.GetLength(0) - 1 &&
                    absY >= 0 && absY <= outputBuffer.GetLength(1) - 1)
                {
                    outputBuffer[absX, absY] = color;
                }

            }
        }

        public override void RenderFrame()
        {
            Debug.Assert(outputBuffer != null, "Subsistema no inicializado");
            Debug.Assert(Simulation.Scene != null, "Simulación no iniciada");

            Clear();
            var simObjects = Simulation.Scene.GetSimulatedObjects();
            foreach(SimulatedObject simObject in simObjects)
            {
                simObject.Pass(0);
            }

            int pixelDataRowSize = (int)MathF.Ceiling(3.0f * outputWidth / 4.0f) * 4;
            int pixelDataSize = pixelDataRowSize * outputHeight;

            int headersSize = 54;
            int fileSize =  headersSize + pixelDataSize;
            
            byte[] fileBuffer = new byte[fileSize];

            // BITMAP HEADER

            fileBuffer[0] = (byte)'B';
            fileBuffer[1] = (byte)'M';
            AddInt32(fileBuffer, 2, fileSize);
            AddInt16(fileBuffer, 6, 0);
            AddInt16(fileBuffer, 8, 0);
            AddInt32(fileBuffer, 10, headersSize); // Pixel offset

            // BITMAPINFOHEADER

            AddInt32(fileBuffer, 14, 40);
            AddInt32(fileBuffer, 18, outputWidth);
            AddInt32(fileBuffer, 22, outputHeight);
            AddInt16(fileBuffer, 26, 1);
            AddInt16(fileBuffer, 28, 24);
            AddInt32(fileBuffer, 30, 0);
            AddInt32(fileBuffer, 34, 0);
            AddInt32(fileBuffer, 38, 1200);
            AddInt32(fileBuffer, 42, 1200);
            AddInt32(fileBuffer, 46, 0);
            AddInt32(fileBuffer, 50, 0);

            for(int y = 0; y < outputHeight; y ++)
            {
                for(int x = 0; x < outputWidth; x ++)
                {
                    AddColor24(fileBuffer, headersSize + y * pixelDataRowSize + x * 3, outputBuffer[x, y]);
                }
            }

            string filePath = String.Format(outputPath + "{0:000000}.bmp", frameNumber);
            File.WriteAllBytes(filePath, fileBuffer);

            frameNumber ++;
        }

        void AddInt32(byte[] buffer, int offset, int value)
        {
            buffer[offset + 0] = (byte)((value >> 0) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        void AddInt16(byte[] buffer, int offset, int value)
        {
            buffer[offset + 0] = (byte)((value >> 0) & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        void AddColor24(byte[] buffer, int offset, Color24 value)
        {
            buffer[offset + 0] = value.b;
            buffer[offset + 1] = value.g;
            buffer[offset + 2] = value.r;
        }

        void Clear()
        {
            Debug.Assert(outputBuffer != null, "Subsistema no inicializado");

            for(int y = 0; y < outputBuffer.GetLength(1); y ++)
            {
                for(int x = 0; x < outputBuffer.GetLength(0); x ++)
                {
                    outputBuffer[x,y] = clearColor;
                }
            }
        }
    }
}
