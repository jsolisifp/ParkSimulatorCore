using System;
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
        public const string typeIdScene = "scene";
        public const string typeIdText = "txt";

        protected Dictionary<string, ResourceLoader> loaders;
        Dictionary<string, object> loadedResources;
        Dictionary<string, int> referenceCounters;

        public Storage()
        {
            loaders = new Dictionary<string, ResourceLoader>();
            loadedResources = new Dictionary<string, object>();
            referenceCounters = new Dictionary<string, int>();
        }

        public abstract void Init(Config config);
        public abstract void Finish();

        public void RegisterLoader(string typeId, ResourceLoader loader) { loaders[typeId] = loader; }
        public void UnregisterLoader(string typeId) { loaders.Remove(typeId); }

        public int GetReferenceCount(string resourceId)
        {
            int count = 0;
            if(referenceCounters.ContainsKey(resourceId)) { count = referenceCounters[resourceId]; }
            return count;
        }

        public void AddReference(string resourceId, string typeId)
        {
            if(!loadedResources.ContainsKey(resourceId))
            {
                // Load resource
                object? resource = loaders[typeId].Load(resourceId);
                Debug.Assert(resource != null, "Cannot load resource " + resourceId);
                loadedResources[resourceId] = resource;
                referenceCounters[resourceId] = 1;
            }
            else
            {
                // Add a new reference
                referenceCounters[resourceId] ++;
            }

        }

        public void RemoveReference(string resourceId, string typeId)
        {
            Debug.Assert(referenceCounters.ContainsKey(resourceId), "Unbalanced remove reference for resource " + resourceId);
            referenceCounters[resourceId] --;

            if(referenceCounters[resourceId] == 0)
            {
                // Unload resource
                loaders[typeId].Unload(resourceId);
                loadedResources.Remove(resourceId);
                referenceCounters.Remove(resourceId);
            }
        }

        public T? GetLoadedResource<T>(string resourceId)
        {
            return (T?)loadedResources[resourceId];
        }

        public void SaveResource(ref string resourceId, string typeId, object resource)
        {
            loaders[typeId].Save(ref resourceId, resource);
        }

        public abstract ReadOnlyCollection<ResourcePointer> GetResourcePointers();
    }
}
