

namespace ParkSimulator
{
    public abstract class ResourceLoader
    {
        public abstract object? Load(string id);
        public abstract void Unload(string id);
        public abstract void Save(ref string id, object value);
        public abstract void Delete(string id);
    }
}
