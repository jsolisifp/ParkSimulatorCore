using System.Diagnostics;

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
            string serialized = File.ReadAllText(storage.BasePath + id + "." + Storage.typeIdScene);

            SimulatedScene scene = SceneSerializerUtility.Deserialize(serialized);

            return scene;
        }

        public override void Save(ref string id, object resource)
        {
            Debug.Assert(typeof(SimulatedScene) == resource.GetType());

            string serialized = SceneSerializerUtility.Serialize((SimulatedScene)resource);

            File.WriteAllText(storage.BasePath + id + "." + Storage.typeIdScene, serialized);
        }

        public override void Unload(string id)
        {
            // Nothing to do
        }

        public override void Delete(string id)
        {
            File.Delete(storage.BasePath + id + "." + Storage.typeIdScene);
        }
    }
}
