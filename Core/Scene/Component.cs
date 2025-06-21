using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace ParkSimulator
{ 
    public struct ComponentFieldInfo
    {
        public string name;
        public string type;
        public bool isResourcePointer;
        public bool isComponent;
        public bool isWritable;
    };

    public abstract class Component
    {
        public bool Active { get; set; } = false;

        protected SimulatedObject? simulatedObject;

        Dictionary<string, PropertyInfo> propertiesByName;
        PropertyInfo[] properties;

        public Component()
        {
            propertiesByName = new();

            Type t = GetType();
            properties = t.GetProperties();
            for (int i = 0; i < properties.Length; i++) { propertiesByName[properties[i].Name] = properties[i]; }

            Active = true;

        }

        public ReadOnlyCollection<ComponentFieldInfo> GetFieldsInfo()
        {
            List<ComponentFieldInfo> fields = new();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo p = properties[i];
                ComponentFieldInfo f = new();
                f.name = p.Name;
                f.type = p.PropertyType.Name;
                f.isResourcePointer = (p.PropertyType.Name == typeof(ResourcePointer).Name);
                f.isComponent = p.PropertyType.IsSubclassOf(typeof(Component));
                f.isWritable = p.CanWrite;
                fields.Add(f);
            }

            return fields.AsReadOnly<ComponentFieldInfo>();
        }

        public T? GetFieldValue<T>(string name)
        {
            return (T?)propertiesByName[name].GetValue(this);
        }

        public void SetFieldValue<T>(string name, T? value)
        {
            propertiesByName[name].SetValue(this, value);
        }

        public virtual void Start() { }
        public virtual void Step() { }
        public virtual void Pass(int passId) { }
        public virtual void Stop() { }


        public void AttachToSimulatedObject(SimulatedObject? so)
        {
            simulatedObject = so;
        }

        public SimulatedObject? GetAttachedSimulatedObject()
        {
            return simulatedObject;
        }

    }
}
