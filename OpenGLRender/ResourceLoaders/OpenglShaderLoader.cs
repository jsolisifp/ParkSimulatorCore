using System.IO;
using Silk.NET.OpenGL;
using static ParkSimulator.Shader;

namespace ParkSimulator
{
    internal class OpenglShaderLoader : ResourceLoader
    {
        public const string typeId = "shader";

        GL openGL;
        DefaultStorage storage;
        OpenglRender render;

        public OpenglShaderLoader(GL _gl)
        {
            openGL = _gl;

            storage = (DefaultStorage)Simulation.Storage;
            render = (OpenglRender)Simulation.Render;

        }

        public override object Load(string id)
        {
            Task<Shader> t = new( () => { 
                string shader = File.ReadAllText(storage.BasePath + id + "." + typeId);
                string[] areas = shader.Split("====");
                string blending = areas[0].Split(" ")[1].Trim();
                Shader.BlendType blendType;
                if (blending == "transparent") { blendType = Shader.BlendType.transparent; }
                else if (blending == "additive") { blendType = Shader.BlendType.additive; }
                else { blendType = BlendType.opaque; }
                return new Shader(openGL, areas[1], areas[2], blendType);
            });

            render.ScheduleTask(t);

            while(!t.IsCompleted) { }

            return t.Result;
        }

        public override void Unload(string id, object asset)
        {
            Task t = new( () => { 
                Shader s = (Shader)asset;
                s.Dispose();
            });

            render.ScheduleTask(t);

            while(!t.IsCompleted) { }
        }

        public override void Save(ref string id, object value)
        {
            // Not supported
        }

        public override void Delete(string id)
        {
            File.Delete(storage.BasePath + id + "." + typeId);
        }
    }
}
