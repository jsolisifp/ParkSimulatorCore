using System.Collections.ObjectModel;

namespace ParkSimulator
{
    public class FileStorage : Storage
    {
        public string BasePath { get; set; } = "";

        public override void Init(Config config)
        {
            BasePath = config.GetTextValue("storageBasePath", "Resources\\");

            if(!Directory.Exists(BasePath)) { Directory.CreateDirectory(BasePath); }

            loaders.Add(typeIdScene, new FileSceneLoader(this));
            loaders.Add(typeIdText, new FileTextLoader(this));
        }

        public override void Finish()
        {
            // Nothing to do
        }

        public override ReadOnlyCollection<ResourcePointer> GetResourcePointers()
        {
            string[] files = Directory.GetFileSystemEntries(BasePath);

            List<ResourcePointer> infos = new();

            for (int i = 0; i < files.Length; i++)
            {
                ResourcePointer info = new(Path.GetFileNameWithoutExtension(files[i].Substring(BasePath.Length)), Path.GetExtension(files[i]).Remove(0,1));
                infos.Add(info);
            }

            return infos.AsReadOnly<ResourcePointer>();
        }

        public override bool ExistsResource(string resourceId, string typeId)
        {
            return File.Exists(BasePath + resourceId + '.' + typeId);
        }
    }
}
