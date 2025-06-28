using System.Diagnostics;

namespace ParkSimulator
{
    public class DefaultTextLoader : ResourceLoader
    {
        DefaultStorage storage;

        public DefaultTextLoader(DefaultStorage _storage)
        {
            storage = _storage;
        }

        public override object? Load(string id)
        {
            string r = File.ReadAllText(storage.BasePath + id + "." + Storage.typeIdText);

            return r;
        }

        public override void Save(ref string id, object resource)
        {
            Debug.Assert(typeof(string) == resource.GetType());
        }

        public override void Unload(string id)
        {
            // Nothing to do
        }

        public override void Delete(string id)
        {
            File.Delete(storage.BasePath + id + "." + Storage.typeIdText);
        }

    }
}
