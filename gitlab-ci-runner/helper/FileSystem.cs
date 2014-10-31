using System.IO;
using Microsoft.Experimental.IO;

namespace gitlab_ci_runner.helper
{
    public class FileSystem
    {
        public static void EnsureFolderExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }


        /// <summary>
        /// Delete non empty directory tree
        /// </summary>
        public static void DeleteDirectory(string path)
        {
            var files = Directory.GetFiles(path);
            var dirs = Directory.GetDirectories(path);

            foreach (var file in files)
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch (PathTooLongException)
                {
                    LongPathFile.Delete(file);
                }
            }

            foreach (var dir in dirs)
            {
                // Only recurse into "normal" directories
                if ((File.GetAttributes(dir) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    try
                    {
                        Directory.Delete(dir, false);
                    }
                    catch (PathTooLongException)
                    {
                        LongPathDirectory.Delete(dir);
                    }
                else
                    DeleteDirectory(dir);
            }

            try
            {
                Directory.Delete(path, false);
            }
            catch (PathTooLongException)
            {
                LongPathDirectory.Delete(path);
            }
        }
    }
}