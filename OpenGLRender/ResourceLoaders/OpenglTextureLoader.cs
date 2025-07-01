using System.IO;
using Silk.NET.OpenGL;

namespace ParkSimulator
{
    internal class OpenglTextureLoader : ResourceLoader
    {
        public const string typeId = "png";

        GL openGL;
        DefaultStorage storage;
        OpenglRender render;

        public OpenglTextureLoader(GL _gl)
        {
            openGL = _gl;

            storage = (DefaultStorage)Simulation.Storage;
            render = (OpenglRender)Simulation.Render;

        }

        public override object Load(string id)
        {
            Task<Texture> t = new( () => { return new Texture(openGL, storage.BasePath + id + "." + typeId); });
            render.ScheduleTask(t);

            while(!t.IsCompleted) { }

            return t.Result;


        }

        public override void Unload(string id, object resource)
        {
            Task t = new( () => { 
                Texture t = (Texture)resource;
                t.Dispose();
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
