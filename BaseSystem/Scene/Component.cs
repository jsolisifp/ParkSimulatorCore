using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace ParkSimulator
{
    public struct ComponentFieldInfo
    {
        public string name;
        public string type;
        public bool isEnum;
        public bool isResourcePointer;
        public bool isComponent;
        public bool isWritable;
    };

    public struct ComponentEnumInfo
    {
        public ReadOnlyCollection<string> names;
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



        public static ComponentEnumInfo GetEnumInfo(string typeName)
        {
            ComponentEnumInfo r = new();

            List<string> names = new();

            Type? type = null;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            int k = 0;
            while(k < assemblies.Length && type == null)
            {
                Assembly? a = assemblies[k];

                Debug.Assert(a != null, "No se encuentra el ensamblado");

                Type[] types = a.GetTypes();

                int i = 0;
                while(i < types.Length && type == null)
                {
                    Type t = types[i];
                    if (t.IsEnum && t.Name == typeName) { type = t; }
                    else { i++; }
                }

                if (type == null) { k++; }

            }


            Debug.Assert(type != null, "Enumerated type " + typeName + " not found");

            r.names = Enum.GetNames(type).AsReadOnly<string>();
            return r;

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
                f.isEnum = p.PropertyType.IsEnum;
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

        public void SetFieldValue<T>(string name, T? value, bool manageResourceLinking = true)
        {
            SimulatedScene? simScene = simulatedObject?.GetAttachedScene();

            if(simScene != null && manageResourceLinking)
            {
                if(simScene.State != SimulatedSceneState.unlinked)
                {
                    if(typeof(T).Name == "ResourcePointer")
                    {
                        ResourcePointer p = GetFieldValue<ResourcePointer>(name);
                        p.UnlinkResource();                                        
                    }
                }
            }

            propertiesByName[name].SetValue(this, value);

            if(simScene != null && manageResourceLinking)
            {
                if(simScene.State != SimulatedSceneState.unlinked)
                {
                    if(typeof(T).Name == "ResourcePointer")
                    {
                        ResourcePointer p = GetFieldValue<ResourcePointer>(name);
                        p.LinkResource();                                        
                    }
                }
            }

        }

        public virtual void Start() { }
        public virtual void Step() { }
        public virtual void Pass(int passId) { }
        public virtual void Stop() { }


        internal void AttachToSimulatedObject(SimulatedObject? so)
        {
            simulatedObject = so;
        }

        internal void DetachFromSimulatedObject()
        {
            simulatedObject = null;
        }

        public SimulatedObject? GetSimulatedObject()
        {
            return simulatedObject;
        }

    }
}
