using UnityEngine;

namespace FCP.Core;

public class CompProperties_VaultNumberStencil : CompProperties
{
    public int digitCount = 3;
    public Color stencilColor = new Color(0.945f, 0.769f, 0.059f);

    public CompProperties_VaultNumberStencil()
    {
        compClass = typeof(CompVaultNumberStencil);
    }
}

public class CompVaultNumberStencil : ThingComp
{
    private string number;
    private Graphic_VaultNumberStencil graphic;

    public CompProperties_VaultNumberStencil Props => (CompProperties_VaultNumberStencil)props;

    public string Number => number;

    public Graphic Graphic => graphic;

    public void SetNumber(string newNumber)
    {
        if (number == newNumber)
            return;

        number = newNumber;
        RebuildGraphic();
    }

    private void RebuildGraphic()
    {
        Texture2D texture = VaultNumberStencilTextureMaker.MakeTexture(number, Props.stencilColor);
        graphic = texture == null
            ? null
            : new Graphic_VaultNumberStencil
            {
                material = MaterialPool.MatFrom(new MaterialRequest
                {
                    mainTex = texture,
                    shader = ShaderDatabase.Cutout,
                    color = Color.white,
                }),
                drawSize = new Vector2(0.3f, 0.3f * texture.height / texture.width),
            };
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref number, "vaultNumber");

        if (Scribe.mode == LoadSaveMode.PostLoadInit && !number.NullOrEmpty())
            RebuildGraphic();
    }
}
