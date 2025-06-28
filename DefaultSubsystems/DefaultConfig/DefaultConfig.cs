namespace ParkSimulator
{
    public class DefaultConfig : Config
    {
        struct Value
        {
            public int intValue;
            public float floatValue;
            public string textValue;
        };

        Dictionary<string, Value> valuesById;

        public DefaultConfig()
        {
            valuesById = new Dictionary<string, Value>();
        }

        public override void Init()
        {
            valuesById.Clear();
        }

        public override void Finish()
        {
            // Nothing to do
        }

        public override float GetFloatValue(string id, float defaultValue = 0)
        {
            return valuesById.ContainsKey(id) ? valuesById[id].floatValue : defaultValue;
        }

        public override int GetIntValue(string id, int defaultValue = 0)
        {
            return valuesById.ContainsKey(id) ? valuesById[id].intValue : defaultValue;
        }

        public override string GetTextValue(string id, string defaultValue = "")
        {
            return valuesById.ContainsKey(id) ? valuesById[id].textValue: defaultValue;
        }

        public override void SetFloatValue(string id, float value)
        {
            Value v = new();
            v.floatValue = value;

            if(!valuesById.ContainsKey(id)) { valuesById.Add(id, v); }
            else { valuesById[id] = v; }
        }

        public override void SetIntValue(string id, int value)
        {
            Value v = new();
            v.intValue = value;

            if(!valuesById.ContainsKey(id)) { valuesById.Add(id, v); }
            else { valuesById[id] = v; }
        }

        public override void SetTextValue(string id, string value)
        {
            Value v = new();
            v.textValue = value;

            if(!valuesById.ContainsKey(id)) { valuesById.Add(id, v); }
            else { valuesById[id] = v; }
        }
    }
}
