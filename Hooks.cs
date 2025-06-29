#pragma warning disable CA2255
using Localizer.UI;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.ModLoader;
using Terraria.UI;
using static Localizer.UI.UIStates.UILocalizerList;
using static Localizer.Systems.LocalizeUtil;
using System.IO;
using System.Linq;
using Localizer.Systems;
using Terraria.Localization;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Text.Json;
using Localizer.DataModel;

namespace Localizer;

internal static class Hooks
{
    public readonly static Type _uIModItemType = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.UI.UIModItem");
    public readonly static Type _uIModsType = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.UI.UIMods");
    public readonly static FieldInfo _modName = _uIModItemType.GetField("_modName", BindingFlags.NonPublic | BindingFlags.Instance);

    private readonly static List<Hook> hooks = [];
    private readonly static List<Action<object, UIMouseEvent>> uiModItemMouseOver = [];
    private readonly static List<Action<object, UIMouseEvent>> uiModItemMouseOut = [];
    private readonly static List<Action<object, SpriteBatch>> uiModsDraw = [];

    #region UIHook
    [ModuleInitializer]
    internal static void UpHooks()
    {
        foreach (var trueT in typeof(Hooks).Assembly.GetTypes().Where(t => t.BaseType == typeof(GlobalUIModItem))) {
            var ctor = trueT.GetConstructor(BindingFlags.Instance | BindingFlags.Public, [])
                                    ?? throw new Exception(typeof(GlobalUIModItem).FullName + "不允许进行有参构造! ===>  " + trueT.FullName);

            var obj = (GlobalUIModItem)ctor.Invoke([]);
            obj.Load();
            uiModItemMouseOver.Add(obj.UIModItemMouseOver);
            uiModItemMouseOut.Add(obj.UIModItemMouseOut);
            uiModsDraw.Add(obj.UIModsDraw);
        }

        var overMethodInfo = _uIModItemType.GetMethod("MouseOver");
        hooks.Add(new Hook(overMethodInfo, MosueOverHook));

        var outMethodInfo = _uIModItemType.GetMethod("MouseOut");
        hooks.Add(new Hook(outMethodInfo, MosueOutHook));

        var drawMethodInfo = _uIModsType.GetMethod("Draw");
        hooks.Add(new Hook(drawMethodInfo, UIModsDrawHook));
    }

    //namespace Terraria.ModLoader.UI.UIModItem
    //          MouseOver(UIMouseEvent evt)
    private delegate void MouseHookDel(object obj, UIMouseEvent evt);
    private static void MosueOverHook(MouseHookDel self, object obj, UIMouseEvent evt)
    {
        self.Invoke(obj, evt);
        foreach (var item in uiModItemMouseOver) {
            try {
                item.Invoke(obj, evt);
            } catch (Exception e) {
                var mod = ModLoader.GetMod("Localizer");
                mod.Logger.Error(e.Message + e.StackTrace);
            }
        }
    }

    //namespace Terraria.ModLoader.UI.UIModItem
    //          MouseOut(UIMouseEvent evt)
    private static void MosueOutHook(MouseHookDel self, object obj, UIMouseEvent evt)
    {
        self.Invoke(obj, evt);
        foreach (var item in uiModItemMouseOut) {
            try {
                item.Invoke(obj, evt);
            } catch (Exception e) {
                var mod = ModLoader.GetMod("Localizer");
                mod.Logger.Error(e.Message + e.StackTrace);
            }
        }
    }
    //namespace Terraria.ModLoader.UI.UIMods
    //          Draw(SpriteBatch spriteBatch)
    private delegate void UIModsDrawDel(object obj, SpriteBatch sb);
    private static void UIModsDrawHook(UIModsDrawDel self, object obj, SpriteBatch sb)
    {
        self.Invoke(obj, sb);
        foreach (var item in uiModsDraw) {
            try {
                item.Invoke(obj, sb);
            } catch (Exception e) {
                var mod = ModLoader.GetMod("Localizer");
                mod.Logger.Error(e.Message + e.StackTrace);
            }
        }
    }
    #endregion

    #region LocalizerHook
    private static Hook hk;
    private static ILHook updateHook;

    [ModuleInitializer]
    internal static void HookLocalizer()
    {
        var method = typeof(LanguageManager).GetMethod("LoadFilesForCulture", BindingFlags.Instance | BindingFlags.NonPublic);
        hk = new Hook(method, LoadFilesForCultureHookMethod);

        var BFSN = BindingFlags.Static | BindingFlags.NonPublic;
        var tarMethod = typeof(LocalizationLoader).GetMethod("Update", BFSN);
        if (tarMethod == null)
            return;
        //MonoModHooks.Add(tarMethod, LocalizationLoaderHookMethod);
        updateHook =
        new ILHook(tarMethod, il => {
            var iLCursor = new ILCursor(il);
            while (iLCursor.Next != null) {
                iLCursor.TryGotoNext();
                var currILOper = iLCursor.Previous?.Operand;
                if (currILOper == null)
                    continue;

                //"IL_006a: call System.Void Terraria.Utils::LogAndChatAndConsoleInfoMessage(System.String)"
                //"IL_00e9: callvirt System.Void Terraria.Localization.LanguageManager::ReloadLanguage(System.Boolean)"
                if (currILOper.ToString().Contains("System.Void Terraria.Localization.LanguageManager::ReloadLanguage")) {
                    //iLCursor.GotoNext();
                    var callMethod = typeof(Hooks).GetMethod("LoadLocalizerText", BindingFlags.Static | BindingFlags.Public);
                    iLCursor.Emit(OpCodes.Call, callMethod);
                    break;
                }
            }
        });
    }

    private delegate void LoadFilesForCultureDel(object obj, GameCulture gcul);
    private static void LoadFilesForCultureHookMethod(LoadFilesForCultureDel orig, object obj, GameCulture gcul)
    {
        orig.Invoke(obj, gcul); //todo
        LoadLocalizerText();
    }

    public static void LoadLocalizerText()
    {
        if (!File.Exists(LoadFilesPath))
            return;

        var localizerfiles = File.ReadAllLines(LoadFilesPath); //获取需要加载的目录
        for (int i = 0; i < localizerfiles.Length; i++) {
            if (!string.IsNullOrEmpty(localizerfiles[i])) {
                string[] kv = localizerfiles[i].Split(SplitString);
                string path = kv[1];
                string modName = kv[0];

                localizerfiles[i] = path;
                if (LoadPath.TryGetValue(modName, out _)) {
                    LoadPath[modName] = path;
                } else {
                    LoadPath.Add(modName, path);
                }
            }
        }
        foreach (var localizerDirectory in localizerfiles) { //localizerText => 目录
            if (!string.IsNullOrEmpty(localizerDirectory)) {
                if (!Directory.Exists(localizerDirectory))
                    return;

                string[] files = Directory.GetFiles(localizerDirectory);
                foreach (var localizerAbsFile in files) {
                    string startChar = LocalizeLoad.GetStartChars(Path.GetFileName(localizerAbsFile));

                    switch (startChar) {
                        case "IL":
                            LoadILHjsonValue(localizerAbsFile);
                            break;
                        default:
                            LoadHjsonValue(localizerAbsFile);
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 传入绝对路径，加载到<see cref="LocalizedTexts"/>
    /// </summary>
    private static void LoadHjsonValue(string localizerFilePath)
    {
        string startChar = LocalizeLoad.GetStartChars(Path.GetFileName(localizerFilePath));
        var kvs = GetLocalizerTextKeyValue(File.ReadAllText(localizerFilePath), startChar);
        foreach (var localizerKv in kvs) {
            if (!LocalizedTexts.TryGetValue(localizerKv.Item1, out var tartext)) {
                LocalizedTexts.Add(localizerKv.Item1, CreateLocalizedText(localizerKv.Item1, localizerKv.Item2));
            } else {
                LocalizedTextSetValue.Invoke(tartext, [localizerKv.Item2]);
            }
        }
    }

    private static void LoadILHjsonValue(string localizerFilePath)
    {
        string modName = null;
        var dirPath = Path.GetDirectoryName(localizerFilePath);
        foreach (var nameOrPath in LoadPath) {
            if(nameOrPath.Value == dirPath) {
                modName = nameOrPath.Key;
                break;
            }
        }
        if(modName != null) {
            var dll = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>(); // => string => typeName
            string jsonText = File.ReadAllText(localizerFilePath);
            IEnumerable<LocalizerILModel> ilmodels = JsonSerializer.Deserialize<List<LocalizerILModel>>(jsonText);
            ilmodels = ilmodels.Select(f => f.Formate());
            var fls = new ForceLocalizeSystem(modName);

            foreach (var lilm in ilmodels) {
                string methodName = lilm.MethodName;
                string typeName = lilm.TypeName;
                if (dll.TryGetValue(typeName, out var methodValues)) {
                    if(methodValues.TryGetValue(methodName, out var values)) {
                        values[lilm.OldValue] = lilm.NewValue;
                    } else { //不存在方法
                        methodValues[methodName] = new Dictionary<string, string>();
                        methodValues[methodName][lilm.OldValue] = lilm.NewValue;
                    }
                } else { //不存在类型
                    var mvalues = new Dictionary<string, Dictionary<string, string>>();
                    var values = new Dictionary<string, string>();
                    values[lilm.OldValue] = lilm.NewValue;
                    dll[typeName] = mvalues;
                    mvalues.Add(lilm.MethodName, values);
                }
            }

            foreach (var tmv in dll) {
                string typeName = tmv.Key;
                foreach (var mv in tmv.Value) {
                    fls.LocalizeByTypeFullName(typeName, mv.Key, mv.Value);
                }
            }
        }
    }
    #endregion

    #region ReLoad
    private static Hook reLoadHook;
    private delegate void ReLoadHookDL();
    private static void ReLoadHookMethod(ReLoadHookDL ogir)
    {
        ogir.Invoke();
        using var writer = File.CreateText(LoadFilesPath);
        var k = LoadPath;
        foreach (var keyValue in LoadPath) {
            writer.WriteLine(keyValue.Key + SplitString + keyValue.Value);
        }
        writer.Flush();
        writer.Close();
    }

    [ModuleInitializer]
    internal static void UpReLoadHook()
    {
        //ModLoader
        var reloadMethod = typeof(ModLoader).GetMethod("Reload", BindingFlags.Static | BindingFlags.NonPublic);
        reLoadHook = new Hook(reloadMethod, ReLoadHookMethod);
    }
    #endregion
}