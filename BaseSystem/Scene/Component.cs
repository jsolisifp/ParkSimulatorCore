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
        public bool isArray;
        public string? arrayElementType;
        public bool arrayElementIsComponent;
        public bool arrayElementIsEnum;
        public bool arrayElementIsResourcePointer;
        public bool isWritable;
    };

    public struct ComponentEnumInfo
    {
        public ReadOnlyCollection<string> names;
        public ReadOnlyCollection<object> values;
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


            Array valuesArray = Enum.GetValues(type);

            object[] values = new object[valuesArray.Length];
            for(int i = 0; i < valuesArray.Length; i++)
            {
                object? v = valuesArray.GetValue(i);
                Debug.Assert(v != null);
                values[i] = v;
            }

            r.values = values.AsReadOnly<object>();

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
                f.isArray = p.PropertyType.IsArray;

                Type? elementType = p.PropertyType.GetElementType();

                if(elementType != null)
                {
                    f.arrayElementType = elementType.Name;
                    f.arrayElementIsEnum = elementType.IsEnum;
                    f.arrayElementIsComponent = elementType.IsSubclassOf(typeof(Component));
                    f.arrayElementIsResourcePointer = (elementType.Name == "ResourcePointer");
                }

                fields.Add(f);
            }

            return fields.AsReadOnly<ComponentFieldInfo>();
        }

        public T? GetFieldValue<T>(string name)
        {
            return (T?)propertiesByName[name].GetValue(this);
        }

        public int GetFieldArrayLength(string name)
        {
            Array? array = (Array?)propertiesByName[name].GetValue(this);
            Debug.Assert(array != null, "Field is not an array");
            return array.Length;
        }

        public T? GetFieldArrayValue<T>(string name, int index)
        {
            Array? array = (Array)propertiesByName[name].GetValue(this);
            return (T?)array.GetValue(index);
        }

        public void SetFieldValue<T>(string name, T? value, bool manageResourceLinking = true)
        {            
            SimulatedScene? simScene = simulatedObject?.GetAttachedScene();

            if (simScene != null && manageResourceLinking)
            {
                if (simScene.State != SimulatedSceneState.unlinked)
                {
                    if (value != null && value.GetType().Name == "ResourcePointer")
                    {
                        ResourcePointer p = GetFieldValue<ResourcePointer>(name);
                        p.UnlinkResource();
                    }
                }
            }

            propertiesByName[name].SetValue(this, value);

            if (simScene != null && manageResourceLinking)
            {
                if (simScene.State != SimulatedSceneState.unlinked)
                {
                    if (value != null && value.GetType().Name == "ResourcePointer")
                    {
                        ResourcePointer p = GetFieldValue<ResourcePointer>(name);
                        p.LinkResource();
                    }
                }
            }
        }


        public void SetFieldArrayValue<T>(string name, int index, T? value, bool manageResourceLinking = true)
        {
            SimulatedScene? simScene = simulatedObject?.GetAttachedScene();

            if(simScene != null && manageResourceLinking)
            {
                if(simScene.State != SimulatedSceneState.unlinked)
                {
                    if(value != null && value.GetType().Name == "ResourcePointer")
                    {
                        ResourcePointer p = GetFieldArrayValue<ResourcePointer>(name, index);
                        p.UnlinkResource();                                        
                    }
                }
            }

            Array? a = (Array?)propertiesByName[name].GetValue(this);

            Debug.Assert(a != null);
            
            a.SetValue(value, index);

            propertiesByName[name].SetValue(this, a);

            if(simScene != null && manageResourceLinking)
            {
                if(simScene.State != SimulatedSceneState.unlinked)
                {
                    if(value != null && value.GetType().Name == "ResourcePointer")
                    {
                        ResourcePointer p = GetFieldArrayValue<ResourcePointer>(name, index);
                        p.LinkResource();                                        
                    }
                }
            }

        }

        public void AddFieldArrayElements(string name, int index, int count)
        {
            Array? a = (Array?)propertiesByName[name].GetValue(this);

            Debug.Assert(a != null, "Property is not an array");

            Type? elementType = propertiesByName[name].PropertyType.GetElementType();

            Debug.Assert(elementType != null, "Cannot get element type");

            Array newA = Array.CreateInstance(elementType, a.Length + count);

            for(int i = 0; i < index; i++) { newA.SetValue(a.GetValue(i), i); }

            for(int i = index + count; i < a.Length + count; i++)  { newA.SetValue(a.GetValue(i - count), i); }

            propertiesByName[name].SetValue(this, newA);
            

        }

        public void RemoveFieldArrayElements(string name, int index, int count, bool manageResourceLinking = true)
        {
            Array? a = (Array?)propertiesByName[name].GetValue(this);

            SimulatedScene? simScene = simulatedObject?.GetAttachedScene();

            if(simScene != null && manageResourceLinking)
            {
                if(simScene.State != SimulatedSceneState.unlinked)
                {
                    for(int i = index; i < index + count; i++)
                    {
                        object? value = a.GetValue(i);

                        if(value != null && value.GetType().Name == "ResourcePointer")
                        {
                            ResourcePointer p = GetFieldArrayValue<ResourcePointer>(name, i);
                            p.UnlinkResource();                                        
                        }

                    }
                }
            }

            Debug.Assert(a != null, "Property is not an array");

            Type? elementType = propertiesByName[name].PropertyType.GetElementType();

            Debug.Assert(elementType != null, "Cannot get element type");

            Array newA = Array.CreateInstance(elementType, a.Length - count);

            for(int i = 0; i < index; i++) { newA.SetValue(a.GetValue(i), i); }

            for(int i = index + count; i < a.Length; i++)  { newA.SetValue(a.GetValue(i), i - count); }

            propertiesByName[name].SetValue(this, newA);
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
