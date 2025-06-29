using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;
using Terraria.UI;

namespace Localizer.UI;
public abstract class GlobalUIModItem
{
    protected Type UIModItemType { get; } = Hooks._uIModItemType;
    protected Type UIModsType { get; } = Hooks._uIModsType;
    protected FieldInfo ModName { get; } = Hooks._modName;
    public virtual void UIModItemMouseOver(object obj, UIMouseEvent evt) { }
    public virtual void UIModItemMouseOut(object obj, UIMouseEvent evt) { }
    public virtual void UIModsDraw(object obj, SpriteBatch sb) { }
    public virtual void Load() { }
}
