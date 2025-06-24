namespace ParkSimulator
{
    public abstract class Config
    {
        public abstract void Init();
        public abstract void SetIntValue(string id, int value);
        public abstract int GetIntValue(string id, int defaultValue = 0);
        public abstract void SetFloatValue(string id, float value);
        public abstract float GetFloatValue(string id, float defaultValue = 0);
        public abstract void SetTextValue(string id, string value);
        public abstract string GetTextValue(string id, string defaultValue = "");
        public abstract void Finish();
    }
}
