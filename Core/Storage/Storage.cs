using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ParkSimulator
{
    public struct ResourcePointer
    {
        public string? resourceId;
        public string? typeId;

        public object? resource;

        public ResourcePointer() { resourceId = null; typeId = null; }
        public ResourcePointer(string _resourceId, string _typeId) { resourceId = _resourceId; typeId = _typeId; }

        public T? Get<T>()
        {
            return (T?) resource;
        }

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

        public bool IsResourceLoaded(string resourceId) { return loadedResources.ContainsKey(resourceId); }
        public T? LoadResourceIfNeeded<T>(string resourceId, string typeId)
        {
            if(!loadedResources.ContainsKey(resourceId))
            {
                object? resource = loaders[typeId].Load(resourceId);
                if(resource != null) { loadedResources[resourceId] = resource; } 
            }

            return (T?)loadedResources[resourceId];        
        }

        public void UnloadResourceIfNeeded(string resourceId, string typeId)
        {
            if(loadedResources.ContainsKey(resourceId))
            {
                loaders[typeId].Unload(resourceId);
                loadedResources.Remove(resourceId);
            }

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

        public abstract ReadOnlyCollection<ResourcePointer> GetResourcePointers();
    }
}
