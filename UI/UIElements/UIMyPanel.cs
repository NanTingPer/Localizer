using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using static System.Net.Mime.MediaTypeNames;

namespace Localizer.UI.UIElements;

public class UIMyPanel : UIPanel
{
    private readonly static Texture2D tex2d = new Texture2D(Main.instance.GraphicsDevice, 1,1);
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        tex2d.SetData([Color.White]);
        //base.DrawSelf(spriteBatch);
        spriteBatch.Draw(tex2d, this.GetRectangle(), Color.Black);
    }
}
