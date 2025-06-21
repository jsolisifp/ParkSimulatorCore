using System.Collections.ObjectModel;

namespace ParkSimulator
{
    public struct ResourceInfo
    {
        public string id;
        public string typeId;
    };

    public abstract class Storage
    {
        protected Dictionary<string, ResourceLoader> loaders;
        Dictionary<string, object> loadedResources;

        public Storage()
        {
            loaders = new Dictionary<string, ResourceLoader>();
            loadedResources = new Dictionary<string, object>();
        }

        public abstract void Init(Config config);
        public abstract void Finish();

        public void RegisterLoader(string typeId, ResourceLoader loader) { loaders[typeId] = loader; }
        public void UnregisterLoader(string typeId) { loaders.Remove(typeId); }

        public T? GetResource<T>(string resourceId) { return (T?)loadedResources[resourceId];  }

        public void LoadResource(string resourceId, string typeId)
        {
            object? resource = loaders[typeId].Load(resourceId);
            if(resource != null) { loadedResources[resourceId] = resource; }            
        }

        public void UnloadResource(string resourceId, string typeId)
        {
            loaders[typeId].Unload(resourceId);
            loadedResources.Remove(resourceId);

        }

        public void SaveResource(ref string resourceId, string typeId, object resource)
        {
            if(loadedResources.ContainsKey(resourceId))
            {
                loaders[typeId].Unload(resourceId);
                loadedResources.Remove(resourceId);
            }

            loaders[typeId].Save(ref resourceId, resource);

            object? reloadedResource = loaders[typeId].Load(resourceId);
            if(reloadedResource != null) { loadedResources[resourceId] = reloadedResource; }
        }

        public abstract ReadOnlyCollection<ResourceInfo> GetResourcesInfo();
    }
}
