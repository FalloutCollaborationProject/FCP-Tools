using UnityEngine;

namespace FCP.Core;

public static class VaultNumberStencilTextureMaker
{
    private const int GlyphColumns = 5;
    private const int GlyphRows = 7;
    private const int PixelScale = 8;
    private const int DigitSpacing = 0;
    private const int Margin = 8;

    private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
    {
        ['0'] = new[] { "01110", "10001", "10011", "10101", "11001", "10001", "01110" },
        ['1'] = new[] { "00100", "01100", "00100", "00100", "00100", "00100", "01110" },
        ['2'] = new[] { "01110", "10001", "00001", "00010", "00100", "01000", "11111" },
        ['3'] = new[] { "11111", "00010", "00100", "00010", "00001", "10001", "01110" },
        ['4'] = new[] { "00010", "00110", "01010", "10010", "11111", "00010", "00010" },
        ['5'] = new[] { "11111", "10000", "11110", "00001", "00001", "10001", "01110" },
        ['6'] = new[] { "00110", "01000", "10000", "11110", "10001", "10001", "01110" },
        ['7'] = new[] { "11111", "00001", "00010", "00100", "01000", "01000", "01000" },
        ['8'] = new[] { "01110", "10001", "10001", "01110", "10001", "10001", "01110" },
        ['9'] = new[] { "01110", "10001", "10001", "01111", "00001", "00010", "01100" },
    };

    public static Texture2D MakeTexture(string number, Color color)
    {
        if (number.NullOrEmpty())
            return null;

        Texture2D vaultTecTexture = TryMakeFromVaultTecOverlays(number, color);
        return vaultTecTexture != null ? vaultTecTexture : MakeFromBuiltinGlyphs(number, color);
    }

    private static Texture2D TryMakeFromVaultTecOverlays(string number, Color color)
    {
        Texture2D[] digitTextures = new Texture2D[number.Length];
        for (int i = 0; i < number.Length; i++)
        {
            digitTextures[i] = ContentFinder<Texture2D>.Get("Things/Overlays/Num_" + number[i], false);
            if (digitTextures[i] == null)
                return null;
        }

        int glyphHeight = 0;
        foreach (Texture2D tex in digitTextures)
            glyphHeight = Mathf.Max(glyphHeight, tex.height);

        int width = Margin * 2;
        foreach (Texture2D tex in digitTextures)
            width += tex.width;
        width += DigitSpacing * (digitTextures.Length - 1);
        int height = glyphHeight + Margin * 2;

        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
        RenderTexture previousActive = RenderTexture.active;

        try
        {
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
            GL.PushMatrix();
            GL.LoadPixelMatrix(0f, width, height, 0f);

            Material blitMat = MaterialPool.MatFrom(new MaterialRequest
            {
                shader = ShaderDatabase.Transparent,
                color = color,
            });
            int cursorX = Margin;
            foreach (Texture2D tex in digitTextures)
            {
                int drawY = Margin + (glyphHeight - tex.height) / 2;
                Graphics.DrawTexture(new Rect(cursorX, drawY, tex.width, tex.height), tex, new Rect(0f, 0f, 1f, 1f), 0, 0, 0, 0, color, blitMat);
                cursorX += tex.width + DigitSpacing;
            }

            GL.PopMatrix();

            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false)
            {
                name = "FCP_VaultNumberStencil_" + number,
                filterMode = FilterMode.Point,
            };
            result.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            result.Apply();
            return result;
        }
        finally
        {
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }

    private static Texture2D MakeFromBuiltinGlyphs(string number, Color color)
    {
        int glyphWidth = GlyphColumns * PixelScale;
        int glyphHeight = GlyphRows * PixelScale;
        int width = number.Length * glyphWidth + (number.Length - 1) * DigitSpacing + Margin * 2;
        int height = glyphHeight + Margin * 2;

        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false)
        {
            name = "FCP_VaultNumberStencil_" + number,
            filterMode = FilterMode.Point,
        };

        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 ink = color;
        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        for (int digitIndex = 0; digitIndex < number.Length; digitIndex++)
        {
            if (!Glyphs.TryGetValue(number[digitIndex], out string[] glyph))
                continue;

            int originX = Margin + digitIndex * (glyphWidth + DigitSpacing);
            for (int row = 0; row < GlyphRows; row++)
            {
                string bits = glyph[row];
                int rowOriginY = Margin + (GlyphRows - 1 - row) * PixelScale;
                for (int col = 0; col < GlyphColumns; col++)
                {
                    if (bits[col] != '1')
                        continue;

                    int colOriginX = originX + col * PixelScale;
                    for (int py = 0; py < PixelScale; py++)
                    {
                        int y = rowOriginY + py;
                        int rowStart = y * width;
                        for (int px = 0; px < PixelScale; px++)
                            pixels[rowStart + colOriginX + px] = ink;
                    }
                }
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        return texture;
    }
}
