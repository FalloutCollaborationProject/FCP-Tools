using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace FCP.Core;

public class SettingsTab : IExposable
{
    public virtual string TabName => string.Empty;
    public virtual string TabToolTip => null;
    public virtual bool Enabled => true;

    public virtual void DoTabWindowContents(Rect tabRect) { }

    public virtual void ExposeData() { }

    public virtual void OnWriteSettings() { }
}