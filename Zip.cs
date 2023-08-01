using System.IO.Compression;

class ZipExtractor
{
    public string filePath;
    public string targetPath;

    public ZipExtractor(string filePath, string targetPath) {
        this.targetPath = targetPath;

        this.filePath = filePath;
    }
    public int Extract()
    {
        
        if (File.Exists(filePath))
        {
            return 1;
        }

        try
        {
            ZipFile.ExtractToDirectory(filePath, targetPath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return 1;
        }

        return 0;
        
    }
}
