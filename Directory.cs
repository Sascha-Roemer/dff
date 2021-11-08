using System;
using System.Collections.Generic;

namespace dff
{
    public static class Directory
    {
        public static IEnumerable<System.IO.FileInfo> GetFiles(this IEnumerable<string> paths, int minSize)
        {
            Queue<string> queue = new Queue<string>();
            foreach(var e in paths) queue.Enqueue(e);
            
            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                try
                {
                    foreach (string subDir in System.IO.Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch(Exception)
                {
                }
                string[] files = null;
                try
                {
                    files = System.IO.Directory.GetFiles(path);
                }
                catch (Exception)
                {
                }
                if (files != null)
                {
                    for(int i = 0 ; i < files.Length ; i++)
                    {
                        
                        var info = new System.IO.FileInfo(files[i]);
                        if (info.Length >= (minSize * 1024))
                        {
                            yield return info;
                        }
                    }
                }
            }
        }
    }
}
