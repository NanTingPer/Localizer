global using static Localizer.Localizer;
using System.IO;
using System;
using Terraria.ModLoader;
using Localizer.DataModel;
using System.Collections.Generic;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using System.Text;
using System.Threading;
using System.Linq;

namespace Localizer;

// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public class Localizer : Mod
{
    public static ILog Log { get => ModLoader.GetMod(nameof(Localizer)).Logger; }
    public const string RouteNetPath = "https://raw.githubusercontent.com/NanTingPer/Localizer/refs/heads/main/LocalizationResource/Route.json";
    public const string ResourceAPI = "https://api.github.com/repos/NanTingPer/Localizer/contents/LocalizationResource";
    private static string? resPath;
    public static string ResPath
    {
        get
        {
            if(resPath == null) {
                resPath = Path.Combine(Environment.CurrentDirectory, nameof(Localizer));
                if (!Directory.Exists(resPath)) {
                    Directory.CreateDirectory(resPath);
                }
            }
            return resPath;
        }
    }
    public readonly static string RoutePath = Path.Combine(ResPath, "Route.json");
    public static string LoadFilesPath { get => Path.Combine(ResPath, "LoadFiles.conf"); }
    public readonly static string SplitString = "###Localizer###";

    private static HttpClient downloadWebClient;
    public static HttpClient DownloadWebClient
    {
        get
        {
            if (downloadWebClient == null) {
                downloadWebClient = new HttpClient();
                downloadWebClient.DefaultRequestHeaders.Add("User-Agent", "tModLoader_Mod_Localizer");
            }
            return downloadWebClient;
        }
    }

    public const string GetResourceAPI = "https://api.github.com/repos/NanTingPer/Localizer/contents/LocalizationResource";

    public override void Load()
    {
        BuildRouteFile();
        base.Load();
    }

    private static List<RouteModel> localizerRoutes = [];
    private static object lockObj = new();

    private static async void BuildRouteFile()
    {
        List<Task> lt = [];
        var fileList = await APIRequest(ResourceAPI);
        var mods = fileList
            .Where(fc => "dir".Equals(fc.Type))
            .ToHashSet()
            .ToList();

        foreach (var fileContent in mods) {
            lt.Add(BuildRouteModel(fileContent));
        }
        await Task.WhenAll(lt);

        if(localizerRoutes.Count > 0) {
            var jsonText = JsonSerializer.Serialize(localizerRoutes);
            File.WriteAllText(RoutePath, jsonText);
        }
    }

    private static async Task BuildRouteModel(FileContent modFileContent)
    {
        var rms = new List<RouteModel>();
        var newAPI = string.Join("/", ResourceAPI, modFileContent.Name); //Name => 文件夹名称兼模组名称
        var localizerList = await APIRequest(newAPI);
        localizerList = localizerList.Where(f => "dir".Equals(f.Type)).ToList();

        foreach (var localizer in localizerList) {
            rms.Add(new RouteModel()
            {
                DirectoryName = localizer.Name,
                ModName = modFileContent.Name
            });
        }
        LocalizerRoutesAdd([.. rms]);
    }

    private static void LocalizerRoutesAdd(params RouteModel[] routes)
    {
        lock (lockObj) {
            localizerRoutes.AddRange(routes);
        }
    }

    private static async Task<List<FileContent>> APIRequest(string apiUri)
    {
        var hrm = await DownloadWebClient.GetAsync(apiUri);
        if (hrm.StatusCode != System.Net.HttpStatusCode.OK) {
            Log.Error("API请求失败，Github请求失败！" + apiUri);
            return [];
        }
        var jsonText = await hrm.Content.ReadAsStringAsync();

        List<FileContent> fileList;
        try {
            fileList = JsonSerializer.Deserialize<List<FileContent>>(jsonText) ?? [];
        } catch (Exception e) {
            Log.Error("构建路由文件失败，Json解析失败！" + apiUri, e);
            return [];
        }
        return fileList ?? [];
    }


    public static async Task DownloadFile(string url, string filePath, CancellationToken ct = default)
    {
        HttpResponseMessage hrm;
        try {
            hrm = await DownloadWebClient.GetAsync(url, ct);
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
        string fileText = await hrm.Content.ReadAsStringAsync(ct);
        using var fs = new FileStream(filePath, FileMode.Create);
        await fs.WriteAsync(Encoding.UTF8.GetBytes(fileText), ct);
        fs.Flush();
        hrm.Dispose();

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
