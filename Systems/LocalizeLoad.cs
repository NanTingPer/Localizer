#pragma warning disable CA2255
using Terraria.ModLoader;

namespace Localizer.Systems;

public class LocalizeLoad : ModSystem
{
    public static string GetStartChars(string fileName)
    {
        var splitString = fileName.Split("_");
        if (splitString.Length == 1) return "";
        //if (splitString[0] == "IL") return "";
        if (fileName.Contains("IL_")) return "IL";
        return splitString[1].Replace(".hjson", ".");
    }
}
