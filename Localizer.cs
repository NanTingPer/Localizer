global using static Localizer.Localizer;
using System.IO;
using System;
using Terraria.ModLoader;
using Localizer.DataModel;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;

namespace Localizer;

// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public class Localizer : Mod
{
    public readonly static string ResPath = Path.Combine(Environment.CurrentDirectory, nameof(Localizer));
    public readonly static string RoutePath = Path.Combine(ResPath, "Route.json");
    public static string LoadFilesPath { get => Path.Combine(ResPath, "LoadFiles.conf"); }
    public readonly static string SplitString = "###Localizer###";
    public readonly static HttpClient DownloadWebClient = new ();

    public static List<RouteModel> ReadRouteText()
    {
        if (!File.Exists(RoutePath)) {
            //TODO ¥”Õ¯¬Áœ¬‘ÿ
            File.Create(RoutePath).Dispose();
        }
        try {
            var allText = File.ReadAllText(RoutePath);
            return JsonSerializer.Deserialize<List<RouteModel>>(allText) ?? [];
        } catch(Exception e) {
            return [];
        }
    }
}
