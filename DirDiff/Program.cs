using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        string dir1Path = @"c:\1"; // Replace with your directory path
        string dir2Path = @"c:\2"; // Replace with your directory path
        string tempDirPath = @"c:\tmp"; // Replace with your temp directory path

        // Collect SVG files from both directories recursively with MD5 hashes
        Dictionary<string, string> files1 = GetSVGFilesWithMD5(dir1Path);
        Dictionary<string, string> files2 = GetSVGFilesWithMD5(dir2Path);

        // Find missing files in dir2
        HashSet<string> missingFiles = new HashSet<string>(files1.Values);
        foreach (var kvp in files2)
        {
            if (!files1.ContainsKey(kvp.Key))
            {
                missingFiles.Add(kvp.Value);
            }
        }

        // Copy missing files to temp directory
        foreach (string file in missingFiles)
        {
            string destFileName = Path.Combine(tempDirPath, GetHashedFileName(file));
            File.Copy(file, destFileName);
        }

        Console.WriteLine("Missing SVG files copied to temp directory.");
    }

    static Dictionary<string, string> GetSVGFilesWithMD5(string directory)
    {
        var files = new Dictionary<string, string>();
        try
        {
            foreach (string filePath in Directory.GetFiles(directory, "*.svg", SearchOption.AllDirectories))
            {
                string hash = CalculateMD5(filePath);
                files[hash] = filePath;
            }
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Access to {directory} is denied.");
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"Directory {directory} not found.");
        }

        return files;
    }

    static string CalculateMD5(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    static string GetHashedFileName(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        int hashCode = fileName.GetHashCode();
        return $"{hashCode}_{fileName}";
    }
}
