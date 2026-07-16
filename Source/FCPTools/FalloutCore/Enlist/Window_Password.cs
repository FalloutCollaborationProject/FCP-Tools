using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FCP.Enlist;

public class Window_Password : Window
{
    public Dictionary<int, Action> actionsByHashCodes;

    protected string password;

    protected string nameMessageKey;

    protected string invalidPasswordMessageKey;

    public bool showInvalidAttempt;

    public bool focusedArea;
    private float Height => 200f;
    public override Vector2 InitialSize => new Vector2(640f, Height);

    public Window_Password(Dictionary<int, Action> actionsByHashCodes, string nameMessageKey, string invalidPasswordMessageKey)
    {
        forcePause = true;
        closeOnAccept = false;
        closeOnCancel = false;
        doCloseX = true;
        this.actionsByHashCodes = actionsByHashCodes;
        this.nameMessageKey = nameMessageKey;
        this.invalidPasswordMessageKey = invalidPasswordMessageKey;
    }
    public override void DoWindowContents(Rect rect)
    {
        Text.Font = GameFont.Small;
        bool flag = false;
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            flag = true;
            Event.current.Use();
        }
        Rect rect2;
        TaggedString message = nameMessageKey.Translate().CapitalizeFirst();
        if (showInvalidAttempt)
        {
            message += "\n\n\n" + invalidPasswordMessageKey.Translate().Colorize(Color.red);
        }
        Widgets.Label(new Rect(0f, 0f, rect.width, rect.height), message);
        GUI.SetNextControlName("passwordField");
        password = Widgets.TextField(new Rect(0f, 80f, (rect.width / 2f) + 70f, 35f), password);
        if (!focusedArea)
        {
            UI.FocusControl("passwordField", this);
            focusedArea = true;
        }
        rect2 = new Rect((rect.width / 2f) + 90f, rect.height - 35f, (rect.width / 2f) - 90f, 35f);

        if (!(Widgets.ButtonText(rect2, "OK".Translate()) || flag))
        {
            return;
        }
        var hashCode = GetDeterministicHashCode(password);
        if (actionsByHashCodes.TryGetValue(hashCode, out var action))
        {
            action();
            Find.WindowStack.TryRemove(this);
        }
        else
        {
            showInvalidAttempt = true;
        }
        Event.current.Use();
    }

    public int GetDeterministicHashCode(string str)
    {
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;
            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                {
                    break;
                }
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }
            return hash1 + (hash2 * 1566083941);
        }
    }
}