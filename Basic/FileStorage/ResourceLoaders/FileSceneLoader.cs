using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public class FileSceneLoader : ResourceLoader
    {
        FileStorage storage;

        public FileSceneLoader(FileStorage _storage)
        {
            storage = _storage;
        }

        public override object? Load(string id)
        {
            string serialized = File.ReadAllText(storage.BasePath + id);

            SimulatedScene scene = SceneSerializer.Deserialize(serialized);

            return scene;
        }

        public override void Save(ref string id, object resource)
        {
            Debug.Assert(typeof(SimulatedScene) == resource.GetType());

            string serialized = SceneSerializer.Serialize((SimulatedScene)resource);

            File.WriteAllText(storage.BasePath + id, serialized);
        }

        public override void Unload(string id)
        {
            // Nothing to do
        }
    }
}
