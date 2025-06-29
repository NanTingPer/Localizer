#nullable enable
using System.Collections.Generic;

namespace Localizer.DataModel;
public class RouteModel
{
    /// <summary>
    /// 模组名称
    /// </summary>
    public string ModName { get; set; } = string.Empty;

    /// <summary>
    /// 目标文件名称 (汉化集合的名称)
    /// </summary>
    public string DirectoryName { get; set; } = string.Empty;

    /// <summary>
    /// 此汉化集合的全部文件
    /// </summary>
    public List<string> FileNames { get; set; } = [];
}
