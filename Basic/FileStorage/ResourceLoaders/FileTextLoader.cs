using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkSimulator
{
    public class FileTextLoader : ResourceLoader
    {
        FileStorage storage;

        public FileTextLoader(FileStorage _storage)
        {
            storage = _storage;
        }

        public override object? Load(string id)
        {
            string r = File.ReadAllText(storage.BasePath + id);

            return r;
        }

        public override void Save(ref string id, object resource)
        {
            Debug.Assert(typeof(string) == resource.GetType());
        }

        public override void Unload(string id)
        {
            // Nothing to do
        }
    }
}
