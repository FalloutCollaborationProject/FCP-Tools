using UnityEngine;

namespace FCPVT;

public class Dialog_NumberSelector : Window
{
    private ThingComp_RollingVaultDoorNumbers _comp;
    private List<int> _selectedNumbers;
    private float _buttonW = 35f;
    private float _buttonH = 35f;
    private float _spacing = 2.5f;
    private float _yOffset = 40f;

    public Dialog_NumberSelector(ThingComp_RollingVaultDoorNumbers comp)
    {
        this._comp = comp;
        _selectedNumbers = [];
        doCloseX = true;
        closeOnClickedOutside = true;
        absorbInputAroundWindow = true;
    }

    public override Vector2 InitialSize => new (425f, 250f);

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(0, 0, inRect.width, 30), "Select Door Numbers");

        Text.Font = GameFont.Small;
        float totalWidth = _buttonW * 10 + _spacing * 9;
        float xOffset = (inRect.width - totalWidth) / 2;

        for (int i = 0; i < 10; i++)
        {
            Rect buttonRect = new (xOffset + (_buttonW + _spacing) * i, _yOffset, _buttonW, _buttonH);
            if (Widgets.ButtonText(buttonRect, i.ToString()))
            {
                if (_selectedNumbers.Count < 3)
                {
                    _selectedNumbers.Add(i);
                }
            }

            int count = _selectedNumbers.FindAll(num => num == i).Count;
            if (count > 0)
            {
                Widgets.DrawBoxSolid(buttonRect, new Color(0, 1, 0, Mathf.Clamp01(0.3f * count)));
            }
        }

        string selectedText = _selectedNumbers.Count > 0
            ? string.Join(" ", _selectedNumbers)
            : "No numbers selected.";
        Rect selectedNumbersRect = new(inRect.x, _yOffset + _buttonH + 10f, inRect.width, 30f);
        Widgets.Label(selectedNumbersRect, $"Selected: {selectedText}");

        if (Widgets.ButtonText(new Rect(inRect.width / 2 - 60, inRect.height - 80, 120, 30), "Reset"))
        {
            _selectedNumbers.Clear();
        }

        if (Widgets.ButtonText(new Rect(inRect.width / 2 - 60, inRect.height - 40, 120, 30), "Confirm"))
        {
            _comp.SetNumbers(_selectedNumbers);
            Close();
        }
    }
}
