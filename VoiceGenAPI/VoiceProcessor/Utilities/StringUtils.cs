namespace VoiceProcessor.Utilities;

public static class StringUtils
{
    public static string Truncate(this string value, int maxLength)
    {
        var realMaxLength = maxLength - 3;
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= realMaxLength ? value : value.Substring(0, realMaxLength) + "..."; 
    }
}