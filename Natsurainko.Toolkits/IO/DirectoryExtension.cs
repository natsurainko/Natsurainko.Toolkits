using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Natsurainko.Toolkits.IO
{
    public static class DirectoryExtension
    {
        public static void DeleteAllFiles(this DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
                file.Delete();

            directory.GetDirectories().ToList().ForEach(x =>
            {
                DeleteAllFiles(x);
                x.Delete();
            });
        }

        public static FileInfo Find(this DirectoryInfo directory,string file)
        {
            foreach (var item in directory.GetFiles())
                if (item.Name == file)
                    return item;

            foreach (var item in directory.GetDirectories())
                return item.Find(file);

            return null;
        }
    }
}
