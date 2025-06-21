﻿using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ParkSimulator
{
    public class FileStorage : Storage
    {
        string? basePath;

        public override void Init(Config config)
        {
            basePath = config.GetTextValue("basePath", "");

            loaders.Add("scene", new SceneFileLoader());
            loaders.Add("txt", new TextFileLoader());
        }

        public override void Finish()
        {
            // Nothing to do
        }

        public override ReadOnlyCollection<ResourceInfo> GetResourcesInfo()
        {
            Debug.Assert(!string.IsNullOrEmpty(basePath));

            string[] files = Directory.GetFileSystemEntries(basePath);

            List<ResourceInfo> infos = new();

            for (int i = 0; i < files.Length; i++)
            {
                ResourceInfo info = new() { id = files[i].Substring(basePath.Length), typeId = Path.GetExtension(files[i]) };
                infos.Add(info);
            }

            return infos.AsReadOnly<ResourceInfo>();
        }

    }
}
