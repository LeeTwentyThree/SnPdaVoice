using System.Runtime.Versioning;
using System.Text.Json;
using VoiceProcessor.Data;

namespace VoiceProcessor.Utilities;

[SupportedOSPlatform("windows")]
public static class VoiceLoadingUtils
{
    public static Dictionary<string, VoiceLineGenerator> LoadAllVoices()
    {
        var voices = new Dictionary<string, VoiceLineGenerator>();
        var voicesFolder = WorkingFolderUtils.GetVoicesFolder();
        var files = Directory.GetFiles(voicesFolder);

        if (files.Length == 0)
        {
            Console.WriteLine($"Warning: No files found in folder '{voicesFolder}");
        }
        
        foreach (var path in files)
        {
            if (!LoadVoiceSettingsAtPathSafe(path, out var settings) || settings == null)
            {
                Console.WriteLine("Failed to initialize voice systems. Quitting...");
                continue;
            }
            
            var generator = new VoiceLineGenerator(settings);
            voices[settings.VoiceId] = generator;
        }

        return voices;
    }

    public static bool LoadVoiceSettingsAtPathSafe(string path, out VoiceGeneratorSettings? settings)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine("File does not exist at path " + path);
            settings = null;
            return false;
        }
        
        try
        {
            settings = JsonSerializer.Deserialize<VoiceGeneratorSettings>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception while loading settings:" + e);
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
            settings = null;
            return false;
        }

        if (settings == null)
        {
            Console.WriteLine("Voice generator settings are null! Press enter to exit...");
            Console.ReadLine();
            return false;
        }

        return true;
    }
}