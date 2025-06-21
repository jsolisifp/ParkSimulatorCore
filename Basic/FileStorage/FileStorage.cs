﻿using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ParkSimulator
{
    public class FileStorage : Storage
    {
        public string BasePath { get; set; } = "";

        public override void Init(Config config)
        {
            BasePath = config.GetTextValue("basePath", "");

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
                ResourcePointer info = new() { resourceId = files[i].Substring(BasePath.Length), typeId = Path.GetExtension(files[i]) };
                infos.Add(info);
            }

            return infos.AsReadOnly<ResourcePointer>();
        }

    }
}
