namespace VoiceProcessor.Utilities;

public static class CleanUpUtils
{
    public static void ClearAllOldFiles(TimeSpan fileLifetime)
    {
        var dateCutOff = DateTime.Now.Subtract(fileLifetime);
        
        foreach (var file in Directory.GetFiles(WorkingFolderUtils.GetTempFolder()))
        {
            try
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < dateCutOff)
                {
                    Console.WriteLine("Deleting unused file: " + fileInfo.Name);
                    fileInfo.Delete();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception thrown while trying to delete the file at path '{file}': {e}");
            }
        }
    }
}