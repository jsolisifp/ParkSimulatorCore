using System.IO;
using System.Threading.Tasks;
using Silk.NET.OpenGL;

namespace ParkSimulator
{
    internal class OpenglModelLoader : ResourceLoader
    {
        public const string typeId = "obj";

        GL openGL;

        DefaultStorage storage;
        OpenglRender render;

        public OpenglModelLoader(GL _gl)
        {
            openGL = _gl;

            storage = (DefaultStorage)Simulation.Storage;
            render = (OpenglRender)Simulation.Render;

        }

        public override object Load(string id)
        {
            Task<Model> task = new Task<Model>(() => { return new Model(openGL, storage.BasePath + id + "." + typeId); });

            render.ScheduleTask(task);

            while(!task.IsCompleted) { }

            return task.Result;
            
        }

        public override void Unload(string id, object resource)
        {
            Task task = new Task(
                () =>
                {
                    Model m = (Model)resource;
                    m.Dispose();
                });

            render.ScheduleTask(task);

            while(!task.IsCompleted) { }

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
