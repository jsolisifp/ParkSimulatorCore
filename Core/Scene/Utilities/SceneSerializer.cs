using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace ParkSimulator
{
    public class SceneSerializer
    {
        public static string Serialize(SimulatedScene scene)
        {
            StringBuilder builder = new();
            string tab = "    ";

            int nextComponentId = 0;

            Dictionary<Component, int> componentToId = new();

            var simObjects = scene.GetSimulatedObjects();

            // Generate ids for components

            for (int i = 0; i < simObjects.Count; i++)
            {
                var components = simObjects[i].GetComponents();

                for (int j = 0; j < components.Count; j++)
                {
                    componentToId[components[j]] = nextComponentId;
                    nextComponentId++;
                }
            }

            // Generate string

            builder.AppendLine("Seed:" + SerializeInt32(scene.Seed));

            builder.AppendLine("ObjectsCount:" + SerializeInt32(simObjects.Count));

            for (int i = 0; i < simObjects.Count; i++)
            {
                SimulatedObject simObject = simObjects[i];
                builder.AppendLine(tab + "Name:" + simObject.Name);
                builder.AppendLine(tab + "Active:" + SerializeBool(simObject.Active));

                var components = simObject.GetComponents();

                builder.AppendLine(tab + "ComponentsCount:" + SerializeInt32(components.Count));

                for (int j = 0; j < components.Count; j++)
                {
                    Component component = components[j];

                    builder.AppendLine(tab + tab + "___Id:" + SerializeInt32(componentToId[component]));
                    builder.AppendLine(tab + tab + "___TypeName:" + component.GetType().Name);

                    var fields = component.GetFieldsInfo();

                    builder.AppendLine(tab + tab + "FieldsCount:" + SerializeInt32(fields.Count));

                    for (int k = 0; k < fields.Count; k++)
                    {
                        ComponentFieldInfo field = fields[k];

                        Object value = component.GetFieldValue<Object>(field.name);

                        builder.AppendLine(tab + tab + tab + "Name:" + field.name);
                        builder.AppendLine(tab + tab + tab + "Type:" + field.type);

                        string serializedValue;

                        if(field.type == "String")
                        {
                            serializedValue = SerializeString((string)value);
                        }
                        else if (field.type == "Single")
                        {
                            serializedValue = SerializeSingle((Single)value);
                        }
                        else if (field.type == "Int32")
                        {
                            serializedValue = SerializeInt32((Int32)value);
                        }
                        else if(field.type == "Boolean")
                        {
                            serializedValue = SerializeBool((Boolean)value);
                        }
                        else if (field.type == "Vector3")
                        {
                            Vector3 v = (Vector3)value;
                            serializedValue = SerializeSingle(v.X) + "," + SerializeSingle(v.Y) + "," + SerializeSingle(v.Z);
                        }
                        else if (field.type == "Vector4")
                        {
                            Vector4 v = (Vector4)value;
                            serializedValue = SerializeSingle(v.X) + "," + SerializeSingle(v.Y) + "," + SerializeSingle(v.Z) + "," + SerializeSingle(v.W);
                        }
                        else if(field.isResourcePointer)
                        {
                            ResourcePointer p = (ResourcePointer)value;
                            serializedValue = p.resourceId + "," + p.typeId;
                        }
                        else if (field.isComponent)
                        {
                            int id = componentToId[(Component)value];
                            serializedValue = SerializeInt32(id);
                        }
                        else
                        {
                            Debug.Assert(false, "Tipo de propiedad no reconocido");
                            serializedValue = "";
                        }

                        builder.AppendLine(tab + tab + tab + "Value:" + serializedValue);

                    }
                }


            }

            return builder.ToString();

        }

        struct ComponentReference
        {
            public Component targetComponent;
            public string targetFieldName;
            public int referencedComponentId;
        }

        public static SimulatedScene Deserialize(string serialized)
        {
            string[] lines = serialized.Split("\r\n"); 
            string line = "";
            int lineIndex = 0;

            Dictionary<int, Component> componentsById = new();
            List<ComponentReference> createdComponentsReferences = new();

            SimulatedScene scene = new();


            // First pass gathers components ids and deserializes everything but the references to components, that are stored in a list

            line = ReadLine(lines, ref lineIndex);

            scene.Seed = Int32.Parse(line.Split(':')[1]);

            line = ReadLine(lines, ref lineIndex);

            int simObjectCount = Int32.Parse(line.Split(':')[1], CultureInfo.InvariantCulture);

            var availableComponents = SimulatedObject.GetComponentsInfo();

            for (int i = 0; i < simObjectCount; i++)
            {
                SimulatedObject simObject = new SimulatedObject();;

                line = ReadLine(lines, ref lineIndex);

                simObject.Name = line.Split(':')[1];

                line = ReadLine(lines, ref lineIndex);
                simObject.Active = DeserializeBool(line.Split(':')[1]);

                line = ReadLine(lines, ref lineIndex);
                int componentCount;
                    
                componentCount = DeserializeInt32(line.Split(':')[1]);

                for (int j = 0; j < componentCount; j++)
                {
                    line = ReadLine(lines, ref lineIndex);
                    int id = DeserializeInt32(line.Split(':')[1]);

                    line = ReadLine(lines, ref lineIndex);
                    string typeName = line.Split(':')[1];
                        
                    line = ReadLine(lines, ref lineIndex);
                    int numFields = DeserializeInt32(line.Split(':')[1]);
                    
                    if(FindComponentInfo(typeName, availableComponents) != null)
                    {   
                        Component? createdComponent = SimulatedObject.CreateComponentByName(typeName);
                        componentsById[id] = createdComponent;

                        for (int k = 0; k < numFields; k++)
                        {
                            line = ReadLine(lines, ref lineIndex);
                            string fieldName = line.Split(':')[1];

                            line = ReadLine(lines, ref lineIndex);
                            string fieldTypeName = line.Split(':')[1];

                            line = ReadLine(lines, ref lineIndex);
                            string serializedValue = line.Split(':')[1];

                            var createdComponentFields = createdComponent?.GetFieldsInfo();
                            ComponentFieldInfo? matchingField = FindFieldInfo(fieldName, createdComponentFields);

                            if(matchingField.HasValue)
                            {
                                if(matchingField.Value.type == fieldTypeName)
                                {
                                    Object value = null;

                                    if (fieldTypeName == "Single") { value = DeserializeSingle(serializedValue); }
                                    else if (fieldTypeName == "Int32") { value = DeserializeInt32(serializedValue); }
                                    else if (fieldTypeName == "Boolean") { value = DeserializeBool(serializedValue); }
                                    else if (fieldTypeName == "Vector3")
                                    {   string[] parts = serializedValue.Split(',');
                                        value = new Vector3(DeserializeSingle(parts[0]),
                                                            DeserializeSingle(parts[1]),
                                                            DeserializeSingle(parts[2]));
                                    }
                                    else if (fieldTypeName == "Vector4")
                                    {   string[] parts = serializedValue.Split(',');
                                        value = new Vector4(DeserializeSingle(parts[0]),
                                                            DeserializeSingle(parts[1]),
                                                            DeserializeSingle(parts[2]),
                                                            DeserializeSingle(parts[3]));
                                    }
                                    else if (fieldTypeName == "String") { value = serializedValue; }
                                    else if(fieldTypeName == "ResourcePointer")
                                    {   string[] parts = serializedValue.Split(',');
                                        value = new ResourcePointer(parts[0], parts[1]);
                                    }
                                    else if(matchingField.Value.isComponent)
                                    {
                                        int componentId = DeserializeInt32(serializedValue);

                                        ComponentReference reference = new() { targetComponent = createdComponent,
                                                                                        targetFieldName = fieldName,
                                                                                        referencedComponentId = componentId };
                                        
                                        createdComponentsReferences.Add(reference);

                                    }

                                    if (value != null)
                                    {
                                        createdComponent.SetFieldValue<object>(fieldName, value);
                                    }

                                }
                            }

                        }

                        simObject.AddComponent(createdComponent);


                    }
                    else
                    {
                        // Skip component fields
                        lineIndex += numFields * 3;
                    }

                }

                scene.AddSimulatedObject(simObject);

            }

            // Second pass sets the references using the list

            for(int i = 0; i < createdComponentsReferences.Count; i++)
            {
                ComponentReference reference = createdComponentsReferences[i];
                reference.targetComponent.SetFieldValue<object>(reference.targetFieldName, componentsById[reference.referencedComponentId]);
            }

            return scene;

        }

        static ComponentInfo? FindComponentInfo(string name, ReadOnlyCollection<ComponentInfo> components)
        {
            ComponentInfo? result = null;
            int index = 0;
            while(index < components.Count && result == null)
            {
                if (components[index].name == name) { result = components[index]; }
                else { index ++; }
            }

            return result;
        }

        static ComponentFieldInfo? FindFieldInfo(string name, ReadOnlyCollection<ComponentFieldInfo> fields)
        {
            ComponentFieldInfo? result = null;
            int index = 0;
            while(index < fields.Count && result == null)
            {
                if (fields[index].name == name) { result = fields[index]; }
                else { index ++; }
            }

            return result;
        }

        static string ReadLine(string[] lines, ref int lineIndex)
        {
            string l = lines[lineIndex];
            lineIndex ++;
            return l;
        }

        static string SerializeString(string s)
        {
            // easy
            return s;
        }

        static string SerializeBool(bool b)
        {
            return b.ToString(CultureInfo.InvariantCulture);
        }

        static string SerializeSingle(float f)
        {
            return f.ToString(CultureInfo.InvariantCulture);
        }

        static string SerializeInt32(float f)
        {
            return f.ToString(CultureInfo.InvariantCulture);
        }
        static string DeserializeString(string s)
        {
            // easy
            return s;
        }

        static bool DeserializeBool(string s)
        {
            return Boolean.Parse(s);
        }

        static float DeserializeSingle(string s)
        {
            return Single.Parse(s, CultureInfo.InvariantCulture);
        }

        static int DeserializeInt32(string s)
        {
            return Int32.Parse(s, CultureInfo.InvariantCulture);
        }

    }
}
