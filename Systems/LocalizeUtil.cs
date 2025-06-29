#nullable enable
using Hjson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Localizer.Systems;

public class LocalizeUtil : ModSystem
{
    private static MethodInfo? localizedTextSetValue;
    /// <summary>
    /// 设置LocalizedText值的方法
    /// </summary>
    public static MethodInfo LocalizedTextSetValue { 
        get
        {
            return localizedTextSetValue ??= 
                typeof(LocalizedText)
                    .GetProperty(nameof(LocalizedText.Value))!
                    .GetSetMethod(true)!;
        }
    }

    internal static Dictionary<string, LocalizedText>? localizedTexts;
    /// <summary>
    /// 全部本地化键和值
    /// </summary>
    public static Dictionary<string, LocalizedText> LocalizedTexts 
    {
        get
        {
            return localizedTexts ??=
                (Dictionary<string, LocalizedText>)typeof(LanguageManager)!
                    .GetField("_localizedTexts", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(LanguageManager.Instance)!;
        }
    }

    /// <summary>
    /// LocalizedText的构造方法(私有的)
    /// </summary>
    private readonly static ConstructorInfo LocalizedTextCons = typeof(LocalizedText).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, [typeof(string), typeof(string)])!;

    /// <summary>
    /// 创建一个LocalizedText，其构造方法是私有的
    /// </summary>
    public static LocalizedText CreateLocalizedText(string Key, string Value)
    {
        return (LocalizedText)LocalizedTextCons.Invoke([Key, Value]);
    }

    public static string? GetText(Stream fileStream, bool isCompressed = false)
    {
        if (!fileStream.CanRead)
            return null;
        if (isCompressed)
            fileStream = new DeflateStream(fileStream, CompressionMode.Decompress);

        StreamReader fileRead = new StreamReader(fileStream, Encoding.UTF8);
        string? content = fileRead.ReadToEnd();
        return content;
    }

    /// <summary>
    /// 传入Hjson的内容,解析为键值，这个是TML的方法
    /// <para> LocalizationLoader.LoadTranslations</para>
    /// </summary>
    public static List<(string, string)> GetLocalizerTextKeyValue(string translationFileContents, string startChar = "", string fileName = "")
    {
        // Parse HJSON and convert to standard JSON
        var flattened = new List<(string, string)>();
        string jsonString;
        try {
            jsonString = HjsonValue.Parse(translationFileContents).ToString();
        } catch (Exception e) {
            string additionalContext = "";
            if (e is ArgumentException && Regex.Match(e.Message, "At line (\\d+),") is Match { Success: true } match && int.TryParse(match.Groups[1].Value, out int line)) {
                string[] lines = translationFileContents.Replace("\r", "").Replace("\t", "    ").Split('\n');
                int start = Math.Max(0, line - 4);
                int end = Math.Min(lines.Length, line + 3);
                var linesOutput = new StringBuilder();
                for (int i = start; i < end; i++) {
                    if (line - 1 == i)
                        linesOutput.Append($"\n{i + 1}[c/ff0000:>" + lines[i] + "]");
                    else
                        linesOutput.Append($"\n{i + 1}:" + lines[i]);
                }
                additionalContext = "\nContext:" + linesOutput.ToString();
            }
            throw new Exception($"The localization file \"{fileName}\" is malformed and failed to load:{additionalContext} ", e);
        }

        // Parse JSON
        var jsonObject = JObject.Parse(jsonString);

        foreach (JToken t in jsonObject.SelectTokens("$..*")) {
            if (t.HasValues) {
                continue;
            }

            // Due to comments, some objects can by empty
            if (t is JObject obj && obj.Count == 0)
                continue;

            // Custom implementation of Path to allow "x.y" keys
            string path = "";
            JToken current = t;

            for (JToken parent = t.Parent; parent != null; parent = parent.Parent) {
                path = parent switch
                {
                    JProperty property => property.Name + (path == string.Empty ? string.Empty : "." + path),
                    JArray array => array.IndexOf(current) + (path == string.Empty ? string.Empty : "." + path),
                    _ => path
                };
                current = parent;
            }
            path = startChar + path.Replace(".$parentVal", "");
            flattened.Add((path, t.ToString()));
        }


        return flattened;
    }
}