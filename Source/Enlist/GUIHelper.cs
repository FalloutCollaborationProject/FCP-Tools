using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Verse.Widgets;

namespace FCP.Enlist;

public static class GUIHelper
{
	public static float OptOnGUI(DiaOption option, Rect rect, bool active = true)
	{
		Color textColor = Widgets.NormalOptionColor;
		string text = option.text;
		if (option.disabled)
		{
			textColor = option.DisabledOptionColor;
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
			option.Activate();
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
				atlas = ButtonBGAtlasMouseover;
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
			Widgets.dropdownPainting = true;
			dropdownPainting_Payload = getPayload(target);
			dropdownPainting_Type = typeof(Payload);
			dropdownPainting_Text = ((dragLabel != null) ? dragLabel : buttonLabel);
			dropdownPainting_Icon = ((dragIcon != null) ? dragIcon : buttonIcon);
		}
		else
		{
			if (!paintable || !dropdownPainting || !Mouse.IsOver(rect) || !(dropdownPainting_Type == typeof(Payload)))
			{
				return;
			}
			FloatMenuOption floatMenuOption = (from opt in menuGenerator(target)
				where object.Equals(opt.payload, dropdownPainting_Payload)
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