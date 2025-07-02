using Silk.NET.Assimp;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Loader;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace ParkSimulator
{

    public class OpenglRender : Render
    {
        Vector3 clearColor;

        Vector4 ambientLight = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
        Vector4 directionalLight;
        Vector3 directionalLightColor;

        Matrix4x4 viewMatrix;
        Matrix4x4 projectionMatrix;

        float opacity;

        bool transparentBlending;

        IWindow? window;

        Thread renderThread;

        public delegate void OnRenderOverlay(float deltaTime);
        public OnRenderOverlay onRenderOverlay;

        public delegate void OnOverrideView(ref Matrix4x4 viewMatrix, ref Matrix4x4 projectionMatrix);
        public OnOverrideView onOverrideView;

        GL context;

        Queue<Task> tasks;

        public override void Init(Config config)
        {
            //context.Enable(GLEnum.CullFace);

            viewMatrix = Matrix4x4.Identity;
            projectionMatrix = Matrix4x4.Identity;

            transparentBlending = false;
            opacity = 1;

            window = null;

            tasks = new Queue<Task>();

            renderThread = new(InitWindow);
            renderThread.IsBackground = true;

            // https://stackoverflow.com/questions/127188/could-you-explain-sta-and-mta
            // https://stackoverflow.com/questions/4154429/apartmentstate-for-dummies
            renderThread.SetApartmentState(ApartmentState.STA);

            renderThread.Start();

        }

        public void InitWindow()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1280, 720);
            options.Title = "OpenGL Renderer";
            window = Window.Create(options);

            window.FramebufferResize += OnFramebufferResize;

            window.Load += OnWindowLoaded;
            window.Update += OnUpdate;
            window.Render += OnRender;

            window.Run();

            window.FramebufferResize -= OnFramebufferResize;
            window.Load -= OnWindowLoaded;

            context.Dispose();

        }

        private void OnUpdate(double deltaTime)
        {
            lock(tasks)
            {
                while(tasks.Count > 0)
                {
                    Task t = tasks.Dequeue();
                    t.RunSynchronously();
                }
            }
        }

        private void OnWindowLoaded()
        {
            window.MakeCurrent();
            context = GL.GetApi(window);

            Simulation.Storage.RegisterLoader(OpenglTextureLoader.typeId, new OpenglTextureLoader(context));
            Simulation.Storage.RegisterLoader(OpenglModelLoader.typeId, new OpenglModelLoader(context));
            Simulation.Storage.RegisterLoader(OpenglShaderLoader.typeId, new OpenglShaderLoader(context));
        }

        public GL GetContext()
        {
            return context;
        }


        public override void Finish()
        {
            window?.Close();
        }

        public void DrawModel(Vector3 position, Vector3 rotation, Vector3 scale, Model model, Shader shader, Texture texture)
        {
            Shader.BlendType blendType = shader.GetBlendType();
            if (blendType == Shader.BlendType.transparent || blendType == Shader.BlendType.additive)
            {   context.Enable(GLEnum.Blend);

                if(blendType == Shader.BlendType.transparent)
                {
                    context.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
                    context.DepthMask(false);
                }
                else // Shader.BlendType.additive
                {
                    context.BlendFunc(GLEnum.SrcAlpha, GLEnum.One);
                    context.DepthMask(false);
                }
                context.Enable(GLEnum.PolygonOffsetFill);
                context.PolygonOffset(1f, -2f);
            }
            else
            {
                context.Disable(GLEnum.Blend);
                context.Disable(GLEnum.PolygonOffsetFill);
                context.DepthMask(true);
            }

            texture.Bind();
            shader.Use();
            shader.SetUniform("uTexture0", 0);
            shader.SetUniform("uView", viewMatrix);
            shader.SetUniform("uProjection", projectionMatrix);
            shader.SetUniform("uDirectionalLight", directionalLight);
            shader.SetUniform("uDirectionalLightColor", directionalLightColor);
            shader.SetUniform("uAmbientLight", ambientLight);
            if(shader.GetBlendType() != Shader.BlendType.opaque)
            {
                shader.SetUniform("uOpacity", opacity);
            }

            float rotX = RenderMathUtils.DegreesToRadians(rotation.X);
            float rotY = RenderMathUtils.DegreesToRadians(rotation.Y);
            float rotZ = RenderMathUtils.DegreesToRadians(rotation.Z);

            var modelMatrix =   Matrix4x4.CreateScale(scale) *
                                Matrix4x4.CreateRotationX(rotX) *
                                Matrix4x4.CreateRotationY(rotY) *
                                Matrix4x4.CreateRotationZ(rotZ) *
                                Matrix4x4.CreateTranslation(position);

            int c = model.meshes.Count;
            for (int i = 0; i < c; i ++)
            {
                Mesh m = model.meshes[i];
                m.Bind();
                shader.SetUniform("uModel", modelMatrix);

                context.DrawArrays(Silk.NET.OpenGL.PrimitiveType.Triangles, 0, (uint)m.vertices.Length);
            }

        }

        public override void RenderFrame()
        {
            // Render is done continously
        }

        public unsafe void OnRender(double deltaTime)
        {
            // Renderizar los objetos

            var simObjects = Simulation.Scene.TryLockSimulatedObjects();

            if(simObjects != null)
            {
                context.Enable(EnableCap.DepthTest);
                context.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, 1.0f);
                context.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Override view

                onOverrideView?.Invoke(ref viewMatrix, ref projectionMatrix);

                foreach(SimulatedObject o in simObjects)
                {
                    o.Pass(0, null);
                }

                // Renderizar los overlays

                onRenderOverlay?.Invoke((float)deltaTime);

                Simulation.Scene.UnlockSimulatedObjects();
            }

        }

        public void SetView(Vector3 position, Vector3 rotation, float fov, float zNear, float zFar)
        {
            float zRads = RenderMathUtils.DegreesToRadians(rotation.Z);
            float yRads = RenderMathUtils.DegreesToRadians(rotation.Y);
            float xRads = RenderMathUtils.DegreesToRadians(rotation.X);

            float fovRads = RenderMathUtils.DegreesToRadians(fov);

            Vector2D<int> size = window.FramebufferSize;

            Matrix4x4 temp = Matrix4x4.CreateRotationX(xRads) * 
                            Matrix4x4.CreateRotationY(yRads) *
                            Matrix4x4.CreateRotationZ(zRads) *
                            Matrix4x4.CreateTranslation(position);
            Matrix4x4.Invert(temp, out viewMatrix);
            projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fovRads, (float)size.X / size.Y, zNear, zFar);
        }

        public void ClearDepth()
        {
            context.Clear(ClearBufferMask.DepthBufferBit);
        }

        public void SetOpacity(float o)
        {
            opacity = o;
        }

        public void SetDirectionalLight(Vector3 direction, float intensity, Vector3 color)
        {
            directionalLight = new Vector4(direction, intensity);
            directionalLightColor = color;
        }

        public void SetClearColor(Vector3 color)
        {
            clearColor = color;
        }

        public void SetAmbientLight(Vector3 color, float intensity)
        {
            ambientLight = new Vector4(color, intensity);
        }


        void OnFramebufferResize(Vector2D<int> newSize)
        {
            context.Viewport(newSize);
        }

        internal void ScheduleTask(Task t)
        {
            lock(tasks)
            {
                tasks.Enqueue(t);
            }
        }

    }
}
