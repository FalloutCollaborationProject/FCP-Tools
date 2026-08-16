using UnityEngine;

namespace FCPVT;

public class ThingComp_RollingVaultDoorNumbers : ThingComp
{
    public CompProperties_RollingVaultDoorNumbers Props => (CompProperties_RollingVaultDoorNumbers)props;

    private Building_RollingVaultDoor _vaultDoor;
    private ThingComp_RollingVaultDoorBase _compBase;
    private List<int> _selectedNums = [];
    private float _fadeMultiplier;
    private Vector3 _drawPos;
    private Rot4 _rotation;
    private float _rotationAngle;
    private Matrix4x4 _matrix;
    private Material _cachedMat;
    private Material _finalMat;
    private readonly MaterialPropertyBlock _mpb = new();

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        _vaultDoor = parent as Building_RollingVaultDoor;
        _compBase = parent.GetComp<ThingComp_RollingVaultDoorBase>();
        _rotation = parent.Rotation;
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (Gizmo gizmo in base.CompGetGizmosExtra())
        {
            yield return gizmo;
        }

        yield return new Command_Action()
        {
            defaultLabel = "Set Door Numbers",
            defaultDesc = "Choose up to 3 numbers to display on this door.",
            icon = ContentFinder<Texture2D>.Get("UI/Icons/SetDoorNumsIcon"),
            action = (() => Find.WindowStack.Add(new Dialog_NumberSelector(this)))
        };

        yield return new Command_Action
        {
            defaultLabel = "Clear Door Numbers",
            defaultDesc = "Reset the displayed numbers on this door.",
            icon = ContentFinder<Texture2D>.Get("UI/Icons/ClearDoorNumsIcon"),
            action = ClearNumbers
        };
    }

    public override void PostDraw()
    {
        if (_selectedNums.Count == 0 || _compBase == null) return;
        _fadeMultiplier = 1f - (_vaultDoor.OpenPct * Props.fadeFactor);
        float spacing = Props.numberSpacing;
        float totalWidth = (_selectedNums.Count - 1) * spacing;
        float startOffset = -totalWidth / 2;

        for (int i = 0; i < _selectedNums.Count; i++)
        {
            int num = _selectedNums[i];
            Texture2D texture = AssetsUtil.GetNumberTexture(num);

            if (texture == null) continue;
            float offset = startOffset + i * spacing;
            _cachedMat = MaterialPool.MatFrom(texture, ShaderDatabase.Transparent, Props.numbersColor);
            DrawExtraDoorGraphics(Props.rotationFactor, Props.shouldFade, _fadeMultiplier, _cachedMat, Props.drawSize, offset);
        }
    }

    private void DrawExtraDoorGraphics(float rotationFactor, bool shouldFade, float opacity, Material material, Vector3 drawSize, float offset)
    {
        _drawPos = _compBase.DrawPos;
        _drawPos.x += offset;
        _drawPos.y += 0.1f;
        _rotationAngle = _compBase.RotationAngle;
        _matrix = Matrix4x4.TRS(
            _drawPos, _rotation.AsQuat * Quaternion.Euler(0f, _rotationAngle * rotationFactor, 0f),
            new Vector3(drawSize.x, 1f, drawSize.y));
        _finalMat = shouldFade ? FadedMaterialPool.FadedVersionOf(material, opacity) : material;

        _mpb.Clear();

        _mpb.SetColor(ShaderPropertyIDs.Color, new Color(material.color.r, material.color.g, material.color.b, opacity));
        Graphics.DrawMesh(MeshPool.plane10, _matrix, _finalMat, 0, null, 0, _mpb);
    }

    public void SetNumbers(List<int> numbers)
    {
        _selectedNums = numbers.Count > 3 ? numbers.GetRange(0, 3) : new List<int>(numbers);
        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
    }

    private void ClearNumbers()
    {
        _selectedNums.Clear();
        parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Collections.Look(ref _selectedNums, "selectedNums", LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit && _selectedNums == null)
        {
            _selectedNums = [];
        }
    }
}
