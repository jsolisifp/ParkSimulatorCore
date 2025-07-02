using Microsoft.VisualBasic.FileIO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace ParkSimulator
{
    public class SceneSerializerUtility
    {
        const string nullSerializedValue = "null";

        enum TypeCategory
        {
            isNormal,
            isEnum,
            isResourcePointer,
            isComponent,
            isArray

        };
        public static string Serialize(SimulatedScene scene)
        {
            StringBuilder builder = new();
            string tab = "    ";

            int nextComponentId = 0;

            Dictionary<Component, int> componentToId = new();

            var simObjects = scene.LockSimulatedObjects();

            // Generate ids for components

            for (int i = 0; i < simObjects.Count; i++)
            {
                var components = simObjects[i].LockComponents();

                for (int j = 0; j < components.Count; j++)
                {
                    componentToId[components[j]] = nextComponentId;
                    nextComponentId++;
                }

                simObjects[i].UnlockComponents();
            }

            // Generate string

            builder.AppendLine("Seed:" + SerializeInt32(scene.Seed));

            builder.AppendLine("ObjectsCount:" + SerializeInt32(simObjects.Count));

            for (int i = 0; i < simObjects.Count; i++)
            {
                SimulatedObject simObject = simObjects[i];
                builder.AppendLine(tab + "Name:" + simObject.Name);
                builder.AppendLine(tab + "Active:" + SerializeBool(simObject.Active));

                var components = simObject.LockComponents();

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

                        Object? value = component.GetFieldValue<Object>(field.name);

                        builder.AppendLine(tab + tab + tab + "Name:" + field.name);
                        builder.AppendLine(tab + tab + tab + "Type:" + field.type);

                        string serializedValue;

                        TypeCategory category;

                        if (field.isEnum) { category = TypeCategory.isEnum; }
                        else if (field.isComponent) { category = TypeCategory.isComponent; }
                        else if (field.isResourcePointer) { category = TypeCategory.isResourcePointer; }
                        else if (field.isArray) { category = TypeCategory.isArray; }
                        else { category = TypeCategory.isNormal; }

                        TypeCategory elementCategory;

                        if(field.isArray)
                        {
                            if(field.arrayElementIsEnum)
                            {
                                elementCategory = TypeCategory.isEnum;
                            }
                            else if(field.arrayElementIsResourcePointer)
                            {
                                elementCategory = TypeCategory.isResourcePointer;
                            }
                            else if (field.arrayElementIsComponent)
                            {
                                elementCategory = TypeCategory.isComponent;
                            }
                            else
                            {
                                elementCategory = TypeCategory.isNormal;
                            }
                        }
                        else
                        {
                            elementCategory = TypeCategory.isNormal;
                        }

                        serializedValue = SerializeValue(field.type, category, field.arrayElementType,
                                                            elementCategory, value, componentToId);

                        builder.AppendLine(tab + tab + tab + "Value:" + serializedValue);

                    }
                }

                simObject.UnlockComponents();


            }

            scene.UnlockSimulatedObjects();

            return builder.ToString();

        }

        struct ComponentAssignOp
        {
            public Component targetComponent;
            public string targetFieldName;
            public bool targetFieldIsArray;
            public int targetFieldArrayIndex;
            public int componentIdToStore;
        }

        public static SimulatedScene Deserialize(string serialized)
        {
            string[] lines = serialized.Split("\r\n"); 
            string line = "";
            int lineIndex = 0;

            Dictionary<int, Component> componentsById = new();
            List<ComponentAssignOp> componentAssignOps = new();

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
                        Component? newComponent = SimulatedObject.CreateComponentByName(typeName);
                        componentsById[id] = newComponent;

                        for (int k = 0; k < numFields; k++)
                        {
                            line = ReadLine(lines, ref lineIndex);
                            string fieldName = line.Split(':')[1];

                            line = ReadLine(lines, ref lineIndex);
                            string fieldTypeName = line.Split(':')[1];

                            line = ReadLine(lines, ref lineIndex);
                            string serializedValue = line.Split(':')[1];

                            var createdComponentFields = newComponent?.GetFieldsInfo();

                            Debug.Assert(createdComponentFields != null);

                            ComponentFieldInfo? matchingField = FindFieldInfo(fieldName, createdComponentFields);

                            if(matchingField.HasValue)
                            {
                                if(matchingField.Value.type == fieldTypeName)
                                {
                                    Object? value = null;

                                    TypeCategory matchingFieldCategory;
                                    if (matchingField.Value.isEnum) { matchingFieldCategory = TypeCategory.isEnum; }
                                    else if (matchingField.Value.isComponent) { matchingFieldCategory = TypeCategory.isComponent; }
                                    else if (matchingField.Value.isResourcePointer) { matchingFieldCategory = TypeCategory.isResourcePointer; }
                                    else if (matchingField.Value.isArray) { matchingFieldCategory = TypeCategory.isArray; }
                                    else { matchingFieldCategory = TypeCategory.isNormal; }

                                    TypeCategory matchingFieldElementCategory;
                                    if (matchingField.Value.arrayElementIsEnum) { matchingFieldElementCategory = TypeCategory.isEnum; }
                                    else if (matchingField.Value.arrayElementIsResourcePointer) { matchingFieldElementCategory = TypeCategory.isResourcePointer; }
                                    else if (matchingField.Value.arrayElementIsComponent) { matchingFieldElementCategory = TypeCategory.isComponent; }
                                    else { matchingFieldElementCategory = TypeCategory.isNormal; }


                                    Debug.Assert(newComponent != null, "Component not set" + typeName);

                                    value = DeserializeValue(serializedValue, matchingField.Value.type, matchingFieldCategory,
                                                            matchingField.Value.arrayElementType, matchingFieldElementCategory,
                                                            newComponent, matchingField.Value.name, false, 0, componentAssignOps); 

                                    if (value != null)
                                    {
                                        Debug.Assert(newComponent != null);

                                        newComponent.SetFieldValue<object>(fieldName, value, false);
                                    }

                                }
                            }

                        }

                        Debug.Assert(newComponent != null);

                        simObject.AddComponent(newComponent);


                    }
                    else
                    {
                        // Skip component fields
                        lineIndex += numFields * 3;
                    }

                }

                scene.AddSimulatedObject(simObject);

            }

            // Second pass assigns components processing the assigns ops

            for(int i = 0; i < componentAssignOps.Count; i++)
            {
                ComponentAssignOp op = componentAssignOps[i];

                if(!op.targetFieldIsArray)
                {
                    op.targetComponent.SetFieldValue<object>(op.targetFieldName, componentsById[op.componentIdToStore], false);
                }
                else
                {
                    op.targetComponent.SetFieldArrayValue<object>(op.targetFieldName, op.targetFieldArrayIndex, componentsById[op.componentIdToStore], false);
                }
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

        static string SerializeValue(string type, TypeCategory category, string? arrayElementType,
                                     TypeCategory? arrayElementCategory, object? value, Dictionary<Component, int> componentToId)
        {
            string r;
            
            if (type == "String")
            {
                Debug.Assert(value != null);

                r = SerializeString((string)value);
            }
            else if (type == "Single")
            {
                Debug.Assert(value != null);

                r = SerializeSingle((Single)value);
            }
            else if (type == "Int32")
            {
                Debug.Assert(value != null);

                r = SerializeInt32((Int32)value);
            }
            else if (type == "Boolean")
            {
                Debug.Assert(value != null);

                r = SerializeBool((Boolean)value);
            }
            else if (type == "Vector2")
            {
                Debug.Assert(value != null);

                Vector2 v = (Vector2)value;
                r = SerializeSingle(v.X) + "," + SerializeSingle(v.Y);
            }
            else if (type == "Vector3")
            {
                Debug.Assert(value != null);

                Vector3 v = (Vector3)value;
                r = SerializeSingle(v.X) + "," + SerializeSingle(v.Y) + "," + SerializeSingle(v.Z);
            }
            else if (type == "Vector4")
            {
                Debug.Assert(value != null);

                Vector4 v = (Vector4)value;
                r = SerializeSingle(v.X) + "," + SerializeSingle(v.Y) + "," + SerializeSingle(v.Z) + "," + SerializeSingle(v.W);
            }
            else if (category == TypeCategory.isEnum)
            {
                Debug.Assert(value != null);

                int index = (int)value;
                r = SerializeInt32(index);

            }
            else if (category == TypeCategory.isArray)
            {
                Debug.Assert(arrayElementType != null);

                int length;

                r = "[";

                Array? arrayValue = (Array?)value;

                if (arrayValue == null) { length = 0; }
                else
                {
                    length = arrayValue.Length;
                }

                if (arrayValue != null)
                {
                    for (int i = 0; i < length; i++)
                    {
                        Debug.Assert(arrayElementCategory != null, "Array element category");
                        r += SerializeValue(arrayElementType, arrayElementCategory.Value, null, null , arrayValue.GetValue(i), componentToId);

                        r += (i < length - 1 ? "|" : "");
                    }

                }

                r += "]";

            }
            else if (category == TypeCategory.isResourcePointer)
            {
                Debug.Assert(value != null);

                ResourcePointer p = (ResourcePointer)value;

                if (p.ResourceId != null) { r = p.ResourceId + "," + p.TypeId; }
                else { r = nullSerializedValue; }
            }
            else if (category == TypeCategory.isComponent)
            {
                if ((Component?)value != null)
                {
                    int id = componentToId[(Component)value];
                    r = SerializeInt32(id);
                }
                else
                {
                    r = nullSerializedValue;
                }
            }
            else
            {
                Debug.Assert(false, "No se reconoce el tipo a serializar");
                r = "";
            }

            return r;

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

        static object? DeserializeValue(string serializedValue, string fieldType, TypeCategory fieldCategory,
                                        string? elementType, TypeCategory? elementCategory,
                                        Component assignOpTargetComponent, string assignOpTargetField, bool assignOpTargetFieldIsArray, int assignOpTargetArrayIndex,
                                        List<ComponentAssignOp> componentAssignOps)
        {
            object? r = null;

            if (fieldType == "Single") { r = DeserializeSingle(serializedValue); }
            else if (fieldType == "Int32") { r = DeserializeInt32(serializedValue); }
            else if (fieldType == "Boolean") { r = DeserializeBool(serializedValue); }
            else if (fieldType == "Vector2")
            {
                string[] parts = serializedValue.Split(',');
                r = new Vector2(DeserializeSingle(parts[0]),
                                    DeserializeSingle(parts[1]));
            }
            else if (fieldType == "Vector3")
            {
                string[] parts = serializedValue.Split(',');
                r = new Vector3(DeserializeSingle(parts[0]),
                                    DeserializeSingle(parts[1]),
                                    DeserializeSingle(parts[2]));
            }
            else if (fieldType == "Vector4")
            {
                string[] parts = serializedValue.Split(',');
                r = new Vector4(DeserializeSingle(parts[0]),
                                    DeserializeSingle(parts[1]),
                                    DeserializeSingle(parts[2]),
                                    DeserializeSingle(parts[3]));
            }
            else if (fieldType == "String") { r = serializedValue; }
            else if (fieldCategory == TypeCategory.isResourcePointer)
            {
                if (serializedValue != nullSerializedValue)
                {
                    string[] parts = serializedValue.Split(',');
                    r = new ResourcePointer(parts[0], parts[1]);
                }
                else
                {
                    r = new ResourcePointer();
                }
            }
            else if (fieldCategory == TypeCategory.isEnum)
            {
                Type? enumType = FindTypeByName(fieldType);

                Debug.Assert(enumType != null, "Enum not found");

                r = Enum.ToObject(enumType, DeserializeInt32(serializedValue));
            }
            else if(fieldCategory == TypeCategory.isArray)
            {
                string bracketsRemoved = serializedValue.Substring(1, serializedValue.Length - 2);
                string[] parts = bracketsRemoved.Split('|', StringSplitOptions.RemoveEmptyEntries);

                Debug.Assert(elementType != null && elementCategory != null);

                Type? _elementType = FindTypeByName(elementType);

                Debug.Assert(_elementType != null);

                Array a = Array.CreateInstance(_elementType, parts.Length);

                for(int i = 0; i < parts.Length; i++)
                {
                    object? v = DeserializeValue(parts[i], elementType, elementCategory.Value, null, null,
                                            assignOpTargetComponent, assignOpTargetField, true, i, componentAssignOps);

                    a.SetValue(v, i); 
                }

                r = a;

            }
            else if (fieldCategory == TypeCategory.isComponent)
            {
                if (serializedValue != nullSerializedValue)
                {
                    int componentId = DeserializeInt32(serializedValue);

                    Debug.Assert(assignOpTargetComponent != null);

                    ComponentAssignOp reference = new()
                    {
                        targetComponent = assignOpTargetComponent,
                        targetFieldName = assignOpTargetField,
                        targetFieldIsArray = assignOpTargetFieldIsArray,
                        targetFieldArrayIndex = assignOpTargetArrayIndex,
                        componentIdToStore = componentId
                    };

                    componentAssignOps.Add(reference);
                }
                else
                {
                    // nothing to do as null is already the default value for a component reference
                }

            }

            return r;

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


        static Type? FindTypeByName(string typeName)
        {
            Type? type = null;

            type = Type.GetType("System." + typeName, false);

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            int k = 0;
            while (k < assemblies.Length && type == null)
            {
                Assembly? a = assemblies[k];

                Debug.Assert(a != null, "No se encuentra el ensamblado");

                Type[] types = a.GetTypes();

                int i = 0;
                while (i < types.Length && type == null)
                {
                    Type t = types[i];
                    if (t.Name == typeName) { type = t; }
                    else { i++; }
                }

                if (type == null) { k++; }

            }

            return type;

        }

    }
}
