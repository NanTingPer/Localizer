global using static Localizer.Localizer;
using System.IO;
using System;
using Terraria.ModLoader;
using Localizer.DataModel;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using log4net.Core;
using log4net;
using System.Text;

namespace Localizer;

// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public class Localizer : Mod
{
    public static ILog Log { get => ModLoader.GetMod(nameof(Localizer)).Logger; }
    public const string RouteNetPath = "https://raw.githubusercontent.com/NanTingPer/Localizer/refs/heads/main/LocalizationResource/Route.json";
    public readonly static string ResPath = Path.Combine(Environment.CurrentDirectory, nameof(Localizer));
    public readonly static string RoutePath = Path.Combine(ResPath, "Route.json");
    public static string LoadFilesPath { get => Path.Combine(ResPath, "LoadFiles.conf"); }
    public readonly static string SplitString = "###Localizer###";
    public readonly static HttpClient DownloadWebClient = new ();
    public const string GetResourceAPI = "https://api.github.com/repos/NanTingPer/Localizer/contents/LocalizationResource";

    public override void Load()
    {
        if (!File.Exists(RoutePath)) {
            _ = DownloadFile(RouteNetPath, RoutePath);
        }
        base.Load();
    }

    public static async Task DownloadFile(string url, string filePath)
    {
        HttpResponseMessage hrm;
        try {
            hrm = await DownloadWebClient.GetAsync(url);
            if (hrm.StatusCode != System.Net.HttpStatusCode.OK) {
                Log.Error("文件请求失败！" + url + " ====> " + filePath);
                return;
            }
        } catch (Exception ex) {
            Log.Error("可能是请求超时了 : " + url + " ====> " + filePath + ex.Message + "\n" + ex.StackTrace);
            return;
        }

        //判断文件夹是否存在
        var directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        //using var rns = hrm.Content.ReadAsStream();
        string fileText = await hrm.Content.ReadAsStringAsync();

        using var fs = new FileStream(filePath, FileMode.Create);
        await Task.Delay(2000);
        fs.Write(Encoding.UTF8.GetBytes(fileText));
        fs.Flush();


        //var buffer = new byte[1024];
        //var readLength = 0;
        //while((readLength = rns.Read(buffer)) != 0) {
        //    fs.Write(buffer, 0, readLength);
        //}
        //await fs.FlushAsync();
        //fs.Dispose();
        //rns.Dispose();
    }

    public static List<RouteModel> ReadRouteText()
    {
        if (!File.Exists(RoutePath)) {
            //TODO 从网络下载
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
