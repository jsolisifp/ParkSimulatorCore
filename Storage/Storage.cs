using System.Collections.ObjectModel;

namespace ParkSimulatorStorage
{
    public abstract class Storage
    {
        public struct ResourceInfo
        {
            public string id;
            public string typeId;
        };

        Dictionary<string, ResourceLoader> loaders;
        Dictionary<string, object> resources;

        public Storage()
        {
            loaders = new Dictionary<string, ResourceLoader>();
            resources = new Dictionary<string, object>();
        }

        public abstract void Open(string path);
        public abstract void Close();

        public void RegisterLoader(string typeId, ResourceLoader loader) { loaders[typeId] = loader; }
        public void UnregisterLoader(string typeId) { loaders.Remove(typeId); }

        public T? GetResource<T>(string resourceId) { return (T?)resources[resourceId];  }

        public void LoadResource(string resourceId, string typeId) { resources[resourceId] = loaders[typeId].Load(resourceId);  }

        public void UnloadResource(string resourceId, string typeId)
        {
            loaders[typeId].Unload(resourceId);
            resources.Remove(resourceId);

        }

        public abstract ReadOnlyCollection<ResourceInfo> GetResourcesInfo();
    }
}
