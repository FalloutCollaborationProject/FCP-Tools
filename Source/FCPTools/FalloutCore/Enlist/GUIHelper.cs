using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Verse.Widgets;

namespace FCP.Enlist;

public static class GUIHelper
{
	private static readonly FieldInfo diaOptionTextField = AccessTools.Field(typeof(DiaOption), "text");
	private static readonly FieldInfo diaOptionDisabledColorField = AccessTools.Field(typeof(DiaOption), "DisabledOptionColor");
	private static readonly MethodInfo diaOptionActivateMethod = AccessTools.Method(typeof(DiaOption), "Activate");
	private static readonly FieldInfo buttonBGAtlasMouseoverField = AccessTools.Field(typeof(Widgets), "ButtonBGAtlasMouseover");
	private static readonly FieldInfo dropdownPaintingField = AccessTools.Field(typeof(Widgets), "dropdownPainting");
	private static readonly FieldInfo dropdownPaintingPayloadField = AccessTools.Field(typeof(Widgets), "dropdownPainting_Payload");
	private static readonly FieldInfo dropdownPaintingTypeField = AccessTools.Field(typeof(Widgets), "dropdownPainting_Type");
	private static readonly FieldInfo dropdownPaintingTextField = AccessTools.Field(typeof(Widgets), "dropdownPainting_Text");
	private static readonly FieldInfo dropdownPaintingIconField = AccessTools.Field(typeof(Widgets), "dropdownPainting_Icon");

	public static float OptOnGUI(DiaOption option, Rect rect, bool active = true)
	{
		Color textColor = Widgets.NormalOptionColor;
		string text = (string)diaOptionTextField.GetValue(option);
		if (option.disabled)
		{
			textColor = (Color)diaOptionDisabledColorField.GetValue(option);
			if (option.disabledReason != null)
			{
				text = text + " (" + option.disabledReason + ")";
			}
		}
		rect.height = Text.CalcHeight(text, rect.width);
		if (option.hyperlink.def != null)
		{
			Widgets.HyperlinkWithIcon(rect, option.hyperlink, text);
		}
		else if (ButtonTextWorker(rect, text, drawBackground: false, !option.disabled, textColor, active && !option.disabled, false) == DraggableResult.Pressed)
		{
			diaOptionActivateMethod.Invoke(option, null);
		}
		return rect.height;
	}

	private static DraggableResult ButtonTextWorker(Rect rect, string label, bool drawBackground, bool doMouseoverSound, Color textColor, bool active, bool draggable, bool middleCenter = false)
	{
		TextAnchor anchor = Text.Anchor;
		Color color = GUI.color;
		if (drawBackground)
		{
			Texture2D atlas = ButtonBGAtlas;
			if (Mouse.IsOver(rect))
			{
				atlas = (Texture2D)buttonBGAtlasMouseoverField.GetValue(null);
				if (Input.GetMouseButton(0))
				{
					atlas = ButtonBGAtlasClick;
				}
			}
			DrawAtlas(rect, atlas);
		}
		if (doMouseoverSound)
		{
			MouseoverSounds.DoRegion(rect);
		}
		if (!drawBackground)
		{
			GUI.color = textColor;
			if (Mouse.IsOver(rect))
			{
				GUI.color = MouseoverOptionColor;
			}
		}
		if (drawBackground || middleCenter)
		{
			Text.Anchor = TextAnchor.MiddleCenter;
		}
		else
		{
		}
		bool wordWrap = Text.WordWrap;
		if (rect.height < Text.LineHeight * 2f)
		{
			//Text.WordWrap = false;
		}
		Label(rect, label);
		Text.Anchor = anchor;
		GUI.color = color;
		Text.WordWrap = wordWrap;
		if (active && draggable)
		{
			return ButtonInvisibleDraggable(rect);
		}
		if (active)
		{
			if (!ButtonInvisible(rect, doMouseoverSound: false))
			{
				return DraggableResult.Idle;
			}
			return DraggableResult.Pressed;
		}
		return DraggableResult.Idle;
	}
	public static void Dropdown<Target, Payload>(Rect rect, Target target, Color iconColor, Func<Target, Payload> getPayload, Func<Target, IEnumerable<DropdownMenuElement<Payload>>> menuGenerator, string buttonLabel = null, Texture2D buttonIcon = null, string dragLabel = null, Texture2D dragIcon = null, Action dropdownOpened = null, bool paintable = false)
	{
		MouseoverSounds.DoRegion(rect);
		DraggableResult draggableResult;
		if (buttonIcon != null)
		{
			DrawHighlightIfMouseover(rect);
			GUI.color = iconColor;
			DrawTextureFitted(rect, buttonIcon, 1f);
			GUI.color = Color.white;
			draggableResult = ButtonInvisibleDraggable(rect);
		}
		else
		{
			draggableResult = ButtonTextWorker(rect, buttonLabel, drawBackground: false, true, NormalOptionColor, true, false, true);
		}
		if (draggableResult == DraggableResult.Pressed)
		{
			List<FloatMenuOption> options = (from opt in menuGenerator(target)
				select opt.option).ToList();
			Find.WindowStack.Add(new FloatMenu(options));
			dropdownOpened?.Invoke();
		}
		else if (paintable && draggableResult == DraggableResult.Dragged)
		{
			dropdownPaintingField.SetValue(null, true);
			dropdownPaintingPayloadField.SetValue(null, getPayload(target));
			dropdownPaintingTypeField.SetValue(null, typeof(Payload));
			dropdownPaintingTextField.SetValue(null, (dragLabel != null) ? dragLabel : buttonLabel);
			dropdownPaintingIconField.SetValue(null, (dragIcon != null) ? dragIcon : buttonIcon);
		}
		else
		{
			bool isPainting = (bool)dropdownPaintingField.GetValue(null);
			Type paintingType = (Type)dropdownPaintingTypeField.GetValue(null);
			if (!paintable || !isPainting || !Mouse.IsOver(rect) || paintingType != typeof(Payload))
			{
				return;
			}
			object paintingPayload = dropdownPaintingPayloadField.GetValue(null);
			FloatMenuOption floatMenuOption = (from opt in menuGenerator(target)
				where object.Equals(opt.payload, paintingPayload)
				select opt.option).FirstOrDefault();
			if (floatMenuOption != null && !floatMenuOption.Disabled)
			{
				Payload x = getPayload(target);
				floatMenuOption.action();
				Payload y = getPayload(target);
				if (!EqualityComparer<Payload>.Default.Equals(x, y))
				{
					SoundDefOf.Click.PlayOneShotOnCamera();
				}
			}
		}
	}
}