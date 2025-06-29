#nullable enable
using Localizer.DataModel;
using Localizer.UI.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using static Localizer.UI.UIStates.UILocalizerList;

namespace Localizer.UI.UIStates;

[UIContent]
public class UILocalizerList : UIState
{
    private UILocalizerList() { }

    /// <summary>
    /// Key 模组名称， Value 路径
    /// </summary>
    public readonly static Dictionary<string, string> LoadPath = [];
    public UIList<UIModText> Mods { get; } = new ();
    public readonly UIList<UILocalizerText> LocalizerFileList = new ();
    public readonly UIScrollbar LocalizerFileListScrollbar = new UIScrollbar();
    public readonly UIScrollbar ModlistScrollbar = new UIScrollbar();

    public override void OnInitialize()
    {
        var uip = new UIMyPanel();
        uip.Width.Set(500, 0);
        uip.Height.Set(500, 0);

        var body = new UILineElements();
        uip.Append(body);
        body.Width = body.Parent.Width;
        body.Height.Pixels = 50;

        var openFile = new UIMyText("打开文件夹");
        openFile.MarginTop = 3f;
        openFile.MarginLeft = 3f;
        openFile.TextColor = Color.White;
        openFile.Width.Pixels = 42;
        openFile.Height.Pixels = 50;
        openFile.OnLeftClick += OpenFile;
        uip.Append(openFile);

        InitModsUI();
        InitLocalizerFileListUI();

        Height.Set(500, 0);
        Width.Set(500, 0);
        uip.Append(Mods);
        uip.Append(LocalizerFileList);
        Append(uip);
        OnLeftClick += OpenModLocalizerList;
        OnLeftClick += EnableOnDisableLocalizer;
        base.OnInitialize();
    }

    private void OpenFile(UIMouseEvent evt, UIElement listeningElement)
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = "explorer.exe",
            Arguments = ResPath,
            UseShellExecute = true
        });
    }

    private void InitModsUI()
    {
        Mods.Height.Set(500, 0);
        Mods.Width.Set(200, 0);
        ModlistScrollbar.Height.Set(500, 0);
        ModlistScrollbar.Width.Set(200, 0);
        ModlistScrollbar.MaxHeight.Set(500, 0);
        Mods.SetScrollbar(ModlistScrollbar);
        Mods.Top.Pixels = 50;
        ModlistScrollbar.Top.Pixels = 50;
    }

    private void InitLocalizerFileListUI()
    {
        LocalizerFileList.Height.Set(500, 0);
        LocalizerFileList.Width.Set(200, 0);
        LocalizerFileListScrollbar.Height.Set(500, 0);
        LocalizerFileListScrollbar.Width.Set(200, 0);
        LocalizerFileListScrollbar.MaxHeight.Set(500, 0);
        LocalizerFileList.SetScrollbar(LocalizerFileListScrollbar);
        LocalizerFileList.Left.Set(200, 0);
        LocalizerFileList.Top.Pixels = 50;
        LocalizerFileListScrollbar.Top.Pixels = 50;
    }

    private void EnableOnDisableLocalizer(UIMouseEvent evt, UIElement listeningElement)
    {
        if(evt.Target is UILocalizerText localizertext) {
            if(localizertext.NetPath != null) {
                DownloadLocalizer(localizertext);
            }

            if (localizertext.LocalPath != null) {
                EnableOrDisableLocalizer(localizertext);
            }
        }
    }

    private Dictionary<UILocalizerText, CancellationTokenSource> downloadCTS = [];

    private async void DownloadLocalizer(UILocalizerText localizertext)
    {
        if (localizertext.NetPath == null)
            return;

        if(downloadCTS.TryGetValue(localizertext, out var cts)) {
            cts.Cancel();
            downloadCTS[localizertext] = new CancellationTokenSource();
        } else {
            downloadCTS[localizertext] = new CancellationTokenSource();
        }
        cts?.Dispose();

        var thisCToken = downloadCTS[localizertext].Token;

        var APIUrl = string.Join("/", GetResourceAPI, localizertext.Mod.Name, localizertext.NetPath);

        List<FileContent>? urls;
        try {
            /*.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue(""));*/
            string reqC = await (await DownloadWebClient.GetAsync(APIUrl, thisCToken))
                        .Content
                        .ReadAsStringAsync(thisCToken);
            if (reqC == null)
                return;
            urls = JsonSerializer.Deserialize<List<FileContent>>(reqC) ?? [];
        } catch (Exception e) {
            Log.Error("资源请求超时, 或Json解析失败! " + e.Message + e.StackTrace);
            return;
        }

        List<Task> downloadTasks = [];
        if(!(urls == null)) {
            var downloadUrls = urls.Where(fc => "file".Equals(fc.Type))
                .Select(fc => new { url = fc.DownloadUrl, name = fc.Name })
                .ToList();

            foreach (var urlAname in downloadUrls) {
                var dPath = Path.Combine(ResPath, localizertext.Mod.Name, localizertext.NetPath, urlAname.name!);
                downloadTasks.Add(DownloadFile(urlAname.url, dPath, thisCToken));
            }
        }

        try {
            await Task.WhenAll(downloadTasks).WaitAsync(thisCToken);
        } catch (Exception) {
            return;
        } finally {
            Main.QueueMainThreadAction(() => {
                foreach (var item in Mods) {
                    if (item is UIModText uit) {
                        if (uit.Mod == localizertext.Mod) {
                            OpenModLocalizer(uit);
                            break;
                        }
                    }
                }
            });
        }
        //localizertext
    }

    private void EnableOrDisableLocalizer(UILocalizerText localizertext)
    {
        if (localizertext.LocalPath == null) return;
        var filePath = localizertext.LocalPath;
        if (Directory.Exists(filePath)) {
            string modName = localizertext.Mod.Name;
            if (LoadPath.TryGetValue(modName, out var path)) {
                if (path == filePath) {
                    LoadPath.Remove(modName); //重复点击关闭
                    localizertext.SetText(GetDirectoryName(localizertext.LocalPath));
                } else {
                    LoadPath[modName] = filePath;
                    localizertext.SetText("(开启)" + localizertext.Text);
                    DisThisAll(localizertext);
                }
            } else {
                LoadPath.Add(modName, filePath);
                localizertext.SetText("(开启)" + localizertext.Text);
                DisThisAll(localizertext);
            }
        }
    }

    private void DisThisAll(UILocalizerText localizertext)
    {
        foreach (var item in LocalizerFileList) {
            if(item is UILocalizerText ult) {
                if (item != localizertext && ult.LocalPath != null) {
                    ult.SetText(GetDirectoryName(ult.LocalPath));
                }
            }
        }
    }

    private void OpenModLocalizerList(UIMouseEvent evt, UIElement listeningElement)
    {
        if(evt.Target is UIModText modText) {
            OpenModLocalizer(modText);
        }
    }

    private void OpenModLocalizer(UIModText modText)
    {
        LocalizerFileList.Clear();
        var modResPath = Path.Combine(ResPath, modText.Mod.Name);
        if (!Directory.Exists(modResPath)) {
            Directory.CreateDirectory(modResPath);
        }

        var files = Directory.GetDirectories(modResPath);
        foreach (var localizerFile in files) {
            var ltextUI = CreateLocalizerTextUI(GetDirectoryName(localizerFile), modText.Mod);
            ltextUI.LocalPath = localizerFile;
            var isEnable = LoadPath.Any(f => f.Value == ltextUI.LocalPath);
            if (isEnable) {
                ltextUI.SetText("(开启)" + GetDirectoryName(ltextUI.LocalPath));
            }
            LocalizerFileList.Add(ltextUI);
        }

        var netPath = ReadRouteText();
        foreach (var route in netPath) {
            if (route.ModName == modText.Mod.Name && !NameExists(route.DirectoryName)) {
                var localizerNet = CreateLocalizerTextUI(("未下载") + route.DirectoryName, modText.Mod);
                localizerNet.NetPath = route.DirectoryName;
                localizerNet.LocalPath = null;
                LocalizerFileList.Add(localizerNet);
            }
        }
    }

    /// <returns>如果存在 返回true</returns>
    private bool NameExists(string name)
    {
        return LocalizerFileList.FirstOrDefault(ult => {
            if (ult is UILocalizerText ULT) {
                if (ULT.LocalPath != null) {
                    return GetDirectoryName(ULT.LocalPath) == name;
                }
                return false;
            }
            return false;
        }) != null;
    }

    private static string GetDirectoryName(string directoryPath)
    {
        return directoryPath.Split(Path.DirectorySeparatorChar)[^1];
    }

    public override void Update(GameTime gameTime)
    {
        Elements.ForEach(uie => uie.Update(gameTime));
        base.Update(gameTime);
    }

    public static UIModText CreateUITextUI(string textf, Mod mod, float scala = 1.0f)
    {
        var text = new UIModText(textf, mod, scala);
        text.Width.Set(200, 0);
        text.Height.Set(50, 0);
        text.Top.Set(0, 0);
        text.Left.Set(0, 0);
        text.MarginTop = 2f;
        return text;
    }

    public static UILocalizerText CreateLocalizerTextUI(string textf, Mod mod, float scala = 1.0f)
    {
        var text = new UILocalizerText(textf) { Mod = mod };
        text.Width.Set(190, 0);
        text.Height.Set(50, 0);
        text.Top.Set(0, 0);
        text.Left.Set(0, 0);
        text.MarginTop = 2f;
        return text;
    }
}


public class LocalizerUIMods : GlobalUIModItem
{
    public static bool IsDraw = false;
    public readonly static UserInterface Interface = new();
    public static UILocalizerList LocalizerListUI { get; } =  UIContent.GetUI<UILocalizerList>();
    internal static SpriteBatch? UISpriteBatch;

    public override void Load()
    {
        Interface.SetState(LocalizerListUI);
        Main.QueueMainThreadAction(() => UISpriteBatch = new SpriteBatch(Main.graphics.GraphicsDevice));
        base.Load();
    }

    public override void UIModItemMouseOut(object obj, UIMouseEvent evt)
    {
        var modNameUi = (UIText)ModName.GetValue(obj)!;
        if (modNameUi.Text.Contains("Localizer")) {
            var uie = (UIElement)obj;
            modNameUi.OnLeftClick -= MosueClick;
        }
    }

    public override void UIModItemMouseOver(object obj, UIMouseEvent evt)
    {
        var modNameUi = (UIText)ModName.GetValue(obj)!;
        if (modNameUi.Text.Contains("Localizer")) {
            var uie = (UIElement)obj;
            modNameUi.OnLeftClick += MosueClick;
        }
    }

    public override void UIModsDraw(object obj, SpriteBatch sb)
    {
        if (IsDraw == true &&
            UISpriteBatch != null) {
            UISpriteBatch.Begin();
            Interface.Update(Main.gameTimeCache);
            Interface.Draw(UISpriteBatch, Main.gameTimeCache);
            UISpriteBatch.End();
        }
    }

    private static void MosueClick(UIMouseEvent eve, UIElement orig)
    {
        Directory.CreateDirectory(ResPath);
        var resDirectorys = Directory.GetDirectories(ResPath);
        for (int i = 0; i < resDirectorys.Length; i++) {
            resDirectorys[i] = resDirectorys[i].Split("\\")[^1];
        }

        LocalizerListUI.Mods.Clear();

        var routeMods = JsonSerializer.Deserialize<List<RouteModel>>(File.ReadAllText(RoutePath)) ?? [];
        var localizermods = routeMods.Select(rm => rm.ModName).Union(resDirectorys);
        foreach (var item in ModLoader.Mods) {
            if (localizermods.Any(f => f.Equals(item.Name)) /*&& !isTrue*/)
                LocalizerListUI.Mods.Add(CreateUITextUI(item.Name, item, 1));
        }
        IsDraw = !IsDraw;
    }
}
