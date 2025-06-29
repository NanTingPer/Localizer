#pragma warning disable CA2255
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.UI;

namespace Localizer.UI;

public static class UIContent
{
    private static Dictionary<Type, object> values = [];

    private static bool isInit = false;

    public static T GetUI<T>() where T : UIState
    {
        if(isInit == false) {
            var trueType = typeof(UIContent)
                .Assembly
                .GetTypes()
                .Where(t => {
                    var atr = t.GetCustomAttribute<UIContentAttribute>();
                    if (atr == null)
                        return false;
                    else
                        return true;
                });

            foreach (var autoType in trueType) {
                var ctor = autoType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, [])
                                        ?? throw new Exception("无效的UIContent, 没有无参构造！" + autoType.FullName);
                object obj;
                try {
                    obj = ctor.Invoke([]);
                } catch {
                    throw new Exception("无效的UIContent, 使用无参构造创建失败！" + autoType.FullName);
                }
                values.Add(autoType, obj);
            }
            isInit = true;
        }

        if(values.TryGetValue(typeof(T), out var value)){
            return (T)value;
        }
        return null;
    }

}
