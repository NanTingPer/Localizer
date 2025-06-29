using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;

namespace Localizer.UI.UIElements;

public class UIMyText : UIText
{
    private Action<Rectangle, SpriteBatch> draw;
    public Color BackColor = Color.Black;
    public Color BackColorMouseOver = Color.White;
    public Color FontColor = Color.White;
    public Color FontColorMouseOver = Color.Black;
    private Texture2D backColorTexture;
    private Texture2D backColorMouseOverTexture;
    private Texture2D white;

    private Texture2D White
    {
        get
        {
            if (white == null) {
                white = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                white.SetData([Color.White]);
            }
            return white;
        }
    }
    
    private Texture2D BackColorTexture
    {
        get
        {
            if(backColorTexture == null) {
                backColorTexture = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                backColorTexture.SetData([BackColor]);
            }
            return backColorTexture;
        }
    }

    private Texture2D BackColorMouseOverTexture
    {
        get
        {
            if (backColorMouseOverTexture == null) {
                backColorMouseOverTexture = new Texture2D(Main.instance.GraphicsDevice, 1, 1);
                backColorMouseOverTexture.SetData([BackColorMouseOver]);
            }
            return backColorMouseOverTexture;
        }
    }



    new private Color TextColor { get; set; } = Color.White;

    public UIMyText(string text, float textScale = 1, bool large = false) : base(text, textScale, large)
    {
        draw ??= BaseDraw;
        OnMouseOver += MouseOver;
        OnMouseOut += MouseOut;
    }
    //关于RT2D //End 设置 Begin Draw null

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        draw?.Invoke(this.GetRectangle(), spriteBatch);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        step = Math.Clamp(step + 0.1f, 0, 1);
        //base.DrawSelf(spriteBatch);
    }

    private float step = 0;
    private void BaseDraw(Rectangle rectangle, SpriteBatch sb)
    {
        sb.Draw(White, rectangle, Color.Lerp(BackColorMouseOver, BackColor, step));
        FontDraw(sb, FontColorMouseOver, FontColor);
    }

    private void MouseOver(Terraria.UI.UIMouseEvent evt, Terraria.UI.UIElement listeningElement)
    {
        draw = MouseOverDraw;
        step = 0;
    }
    private void MouseOverDraw(Rectangle rectangle, SpriteBatch sb)
    {
        sb.Draw(White, rectangle, Color.Lerp(BackColor, BackColorMouseOver, step));
        FontDraw(sb, FontColor, FontColorMouseOver);
    }
    private void MouseOutDraw(Rectangle rectangle, SpriteBatch sb)
    {
        sb.Draw(White, rectangle, Color.Lerp(BackColorMouseOver, BackColor, step));
        FontDraw(sb, FontColorMouseOver, FontColor);
    }
    private void FontDraw(SpriteBatch sb, Color c1, Color c2)
    {
        var rectangle = this.GetRectangle();
        Vector2 pos = new Vector2(rectangle.X, rectangle.Y);
        var font = FontAssets.MouseText;
        var textSize = font.Value.MeasureString(Text);
        var textPos = pos + new Vector2((rectangle.Width - textSize.X) / 2f, (rectangle.Height - textSize.Y) / 2f);
        sb.DrawString(font.Value, Text, textPos, Color.Lerp(c1, c2, step));

    }
    private void MouseOut(Terraria.UI.UIMouseEvent evt, Terraria.UI.UIElement listeningElement)
    {
        draw = MouseOutDraw;
        step = 0;
    }
}
