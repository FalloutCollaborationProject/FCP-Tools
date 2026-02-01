using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace FCP.Enlist;

public class Dialog_FactionStorage : Dialog_NodeTree
{
	public override float Margin => 0f;
	public override Vector2 InitialSize => new Vector2(620f, Mathf.Min(480, UI.screenHeight));

	private DiaOption xOption;
	private FactionStorage factionStorage;
	public Caravan caravan;
	public Dialog_FactionStorage(DiaNode startNode, bool radioMode, Caravan caravan, FactionStorage factionStorage)
		: base(startNode, radioMode)
	{
		this.caravan = caravan;

		this.factionStorage = factionStorage;

		xOption = new DiaOption("X");
		xOption.resolveTree = true;
		xOption.dialog = this;

		this.absorbInputAroundWindow = false;
	}

	public override void PreClose()
	{
		base.PreClose();
		curNode.PreClose();
	}

	public override void PostClose()
	{
		base.PostClose();
		if (closeAction != null)
		{
			closeAction();
		}
	}

	public override void WindowOnGUI()
	{
		if (screenFillColor != Color.clear)
		{
			GUI.color = screenFillColor;
			GUI.DrawTexture(new Rect(0f, 0f, UI.screenWidth, UI.screenHeight), BaseContent.WhiteTex);
			GUI.color = Color.white;
		}
		base.WindowOnGUI();
	}

	[TweakValue("0Enlist", 0, 700)] public static float cancelXOffset = 605f;
	[TweakValue("0Enlist", 0, 500)] public static float cancelYOffset = 60f;
	public override void DoWindowContents(Rect inRect)
	{
		factionStorage.FillTab(this);
		Rect cancelBox = new Rect(inRect.x + cancelXOffset, 0, 100, 30);
		GUIHelper.OptOnGUI(xOption, cancelBox);
		Text.Anchor = TextAnchor.UpperLeft;
	}
}
public class FactionStorage : IExposable
{
	public List<Thing> storedThings;

	private Vector2 scrollPositionFactionStorage;

	private float scrollViewHeightFactionStorage;

	private List<TransferableImmutable> cachedItemsFactionStorage = new List<TransferableImmutable>();

	private int cachedItemsHashFactionStorage;

	private int cachedItemsCountFactionStorage;

	private List<TransferableImmutable> cachedItemsCaravan = new List<TransferableImmutable>();

	private int cachedItemsHashCaravan;

	private int cachedItemsCountCaravan;

	private Vector2 scrollPositionCaravan;
									  
	private float scrollViewHeightCaravan;

	[TweakValue("0Enlist", 0f, 2f)] public static float separator = 1.6f;

	[TweakValue("0Enlist", 0f, 200f)] public static float lowerBoxHeight = 180f;
	public void FillTab(Dialog_FactionStorage window)
	{
		Rect rectFactionStorage = new Rect(0f, 0f, window.windowRect.size.x, window.windowRect.size.y / separator);
		rectFactionStorage.yMin += 7f;
		GUI.BeginGroup(rectFactionStorage);
		CheckCacheItemsFactionStorage();
		DoRowsFactionStorage(rectFactionStorage.size, cachedItemsFactionStorage, window.caravan, ref scrollPositionFactionStorage, ref scrollViewHeightFactionStorage);
		GUI.EndGroup();

		Rect rectCaravan = new Rect(rectFactionStorage.x, rectFactionStorage.yMax - 10, window.windowRect.size.x, lowerBoxHeight);
		GUI.BeginGroup(rectCaravan);
		CheckCacheItemsCaravan(window.caravan);
		DoRowsCaravan(rectCaravan.size, cachedItemsCaravan, window.caravan, ref scrollPositionCaravan, ref scrollViewHeightCaravan);
		GUI.EndGroup();
	}

	private void CheckCacheItemsFactionStorage()
	{
		if (storedThings.Count != cachedItemsCountFactionStorage)
		{
			CacheItemsFactionStorage();
			return;
		}
		int num = 0;
		for (int i = 0; i < storedThings.Count; i++)
		{
			num = Gen.HashCombineInt(num, storedThings[i].GetHashCode());
		}
		if (num != cachedItemsHashFactionStorage)
		{
			CacheItemsFactionStorage();
		}
	}

	private void CacheItemsFactionStorage()
	{
		cachedItemsFactionStorage.Clear();
		int seed = 0;
		for (int i = 0; i < storedThings.Count; i++)
		{
			try
			{
				TransferableImmutable transferableImmutable = TransferableUtility.TransferableMatching(storedThings[i], cachedItemsFactionStorage, TransferAsOneMode.Normal);
				if (transferableImmutable == null)
				{
					transferableImmutable = new TransferableImmutable();
					cachedItemsFactionStorage.Add(transferableImmutable);
				}
				transferableImmutable.things.Add(storedThings[i]);
				seed = Gen.HashCombineInt(seed, storedThings[i].GetHashCode());
			}
			catch
			{
				Log.Message("Issue with " + storedThings[i]);
			}
		}
		cachedItemsCountFactionStorage = storedThings.Count;
		cachedItemsHashFactionStorage = seed;
	}
	public void DoRowsFactionStorage(Vector2 size, List<TransferableImmutable> things, Caravan caravan, ref Vector2 scrollPosition, ref float scrollViewHeight)
	{
		Text.Font = GameFont.Small;
		Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
		Rect viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
		Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
		float curY = 0f;
		Widgets.ListSeparator(ref curY, viewRect.width, "RH.StorageInventory".Translate());
		if (things.Any())
		{
			for (int i = 0; i < things.Count; i++)
			{
				DoRowFactionStorage(ref curY, viewRect, rect, scrollPosition, things[i], caravan);
			}
		}
		else
		{
			Widgets.NoneLabel(ref curY, viewRect.width);
		}
		if (Event.current.type == EventType.Layout)
		{
			scrollViewHeight = curY + 30f;
		}
		Widgets.EndScrollView();
	}
	private void DoRowFactionStorage(ref float curY, Rect viewRect, Rect scrollOutRect, Vector2 scrollPosition, TransferableImmutable thing, Caravan caravan)
	{
		float num = scrollPosition.y - 30f;
		float num2 = scrollPosition.y + scrollOutRect.height;
		if (curY > num && curY < num2)
		{
			DoRowFactionStorage(new Rect(0f, curY, viewRect.width, 30f), thing, caravan);
		}
		curY += 30f;
	}

	private void DoRowFactionStorage(Rect rect, TransferableImmutable thing, Caravan caravan)
	{
		GUI.BeginGroup(rect);
		Rect rect2 = rect.AtZero();
		if (thing.TotalStackCount != 1)
		{
			DoAbandonSpecificCountButtonFactionStorage(rect2, thing, caravan);
		}
		rect2.width -= 24f;
		DoAbandonButtonFactionStorage(rect2, thing, caravan);
		rect2.width -= 24f;
		Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, thing.AnyThing);
		rect2.width -= 24f;
		Rect rect3 = rect2;
		rect3.xMin = rect3.xMax - 60f;
		CaravanThingsTabUtility.DrawMass(thing, rect3);
		rect2.width -= 60f;
		Widgets.DrawHighlightIfMouseover(rect2);
		Rect rect4 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
		Widgets.ThingIcon(rect4, thing.AnyThing);
		Rect rect5 = new Rect(rect4.xMax + 4f, 0f, 300f, 30f);
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
		Widgets.Label(rect5, thing.LabelCapWithTotalStackCount.Truncate(rect5.width));
		Text.Anchor = TextAnchor.UpperLeft;
		Text.WordWrap = true;
		GUI.EndGroup();
	}

	public void DoAbandonButtonFactionStorage(Rect rowRect, TransferableImmutable t, Caravan caravan)
	{
		Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
		if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.AbandonButtonTex))
		{
			foreach (var thing in t.things)
			{
				CaravanInventoryUtility.GiveThing(caravan, thing);
				this.storedThings.Remove(thing);
			}
		}
	}
	public void DoAbandonSpecificCountButtonFactionStorage(Rect rowRect, TransferableImmutable t, Caravan caravan)
	{
		Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
		if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.AbandonSpecificCountButtonTex))
		{
			TryAbandonSpecificCountViaInterfaceFactionStorage(t, caravan);
		}
	}


	public void TryAbandonSpecificCountViaInterfaceFactionStorage(TransferableImmutable t, Caravan caravan)
	{
		Find.WindowStack.Add(new Dialog_Slider("AbandonSliderText".Translate(t.Label), 1, t.TotalStackCount, delegate (int x)
		{
			int num = x;
			for (int i = 0; i < t.things.Count; i++)
			{
				if (num <= 0)
				{
					break;
				}
				Thing thing = t.things[i];

				if (num >= thing.stackCount)
				{
					num -= thing.stackCount;
					CaravanInventoryUtility.GiveThing(caravan, thing);
					this.storedThings.Remove(thing);
				}
				else
				{
					CaravanInventoryUtility.GiveThing(caravan, thing.SplitOff(num));
					num = 0;
				}
			}
		}));
	}
	public string GetAbandonOrBanishButtonTooltipFactionStorage(TransferableImmutable t, bool abandonSpecificCount)
	{
		Pawn pawn = t.AnyThing as Pawn;
		if (pawn != null)
		{
			return PawnBanishUtility.GetBanishButtonTip(pawn);
		}
		return GetAbandonItemButtonTooltipFactionStorage(t.TotalStackCount, abandonSpecificCount);
	}

	private string GetAbandonItemButtonTooltipFactionStorage(int currentStackCount, bool abandonSpecificCount)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (currentStackCount == 1)
		{
			stringBuilder.AppendLine("AbandonTip".Translate());
		}
		else if (abandonSpecificCount)
		{
			stringBuilder.AppendLine("AbandonSpecificCountTip".Translate());
		}
		else
		{
			stringBuilder.AppendLine("AbandonAllTip".Translate());
		}
		stringBuilder.AppendLine();
		stringBuilder.Append("AbandonItemTipExtraText".Translate());
		return stringBuilder.ToString();
	}

	private void CheckCacheItemsCaravan(Caravan caravan)
	{
		var caravanThings = CaravanInventoryUtility.AllInventoryItems(caravan).ToList();
		if (caravanThings.Count != cachedItemsCountCaravan)
		{
			CacheItemsCaravan(caravan);
			return;
		}
		int num = 0;
		for (int i = 0; i < caravanThings.Count; i++)
		{
			num = Gen.HashCombineInt(num, caravanThings[i].GetHashCode());
		}
		if (num != cachedItemsHashCaravan)
		{
			CacheItemsCaravan(caravan);
		}
	}

	private void CacheItemsCaravan(Caravan caravan)
	{
		var caravanThings = CaravanInventoryUtility.AllInventoryItems(caravan).ToList();
		cachedItemsCaravan.Clear();
		int seed = 0;
		for (int i = 0; i < caravanThings.Count; i++)
		{
			try
			{
				TransferableImmutable transferableImmutable = TransferableUtility.TransferableMatching(caravanThings[i], cachedItemsCaravan, TransferAsOneMode.Normal);
				if (transferableImmutable == null)
				{
					transferableImmutable = new TransferableImmutable();
					cachedItemsCaravan.Add(transferableImmutable);
				}
				transferableImmutable.things.Add(caravanThings[i]);
				seed = Gen.HashCombineInt(seed, caravanThings[i].GetHashCode());
			}
			catch
			{
				Log.Message("Issue with " + caravanThings[i]);
			}
		}
		cachedItemsCountCaravan = caravanThings.Count;
		cachedItemsHashCaravan = seed;
	}
	public void DoRowsCaravan(Vector2 size, List<TransferableImmutable> things, Caravan caravan, ref Vector2 scrollPosition, ref float scrollViewHeight)
	{
		Text.Font = GameFont.Small;
		Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
		Rect viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
		Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
		float curY = 0f;
		Widgets.ListSeparator(ref curY, viewRect.width, "RH.CaravanInventory".Translate());
		if (things.Any())
		{
			for (int i = 0; i < things.Count; i++)
			{
				DoRowCaravan(ref curY, viewRect, rect, scrollPosition, things[i], caravan);
			}
		}
		else
		{
			Widgets.NoneLabel(ref curY, viewRect.width);
		}
		if (Event.current.type == EventType.Layout)
		{
			scrollViewHeight = curY + 30f;
		}
		Widgets.EndScrollView();
	}
	private void DoRowCaravan(ref float curY, Rect viewRect, Rect scrollOutRect, Vector2 scrollPosition, TransferableImmutable thing, Caravan caravan)
	{
		float num = scrollPosition.y - 30f;
		float num2 = scrollPosition.y + scrollOutRect.height;
		if (curY > num && curY < num2)
		{
			DoRowCaravan(new Rect(0f, curY, viewRect.width, 30f), thing, caravan);
		}
		curY += 30f;
	}

	private void DoRowCaravan(Rect rect, TransferableImmutable thing, Caravan caravan)
	{
		GUI.BeginGroup(rect);
		Rect rect2 = rect.AtZero();
		if (thing.TotalStackCount != 1)
		{
			DoAbandonSpecificCountButtonCaravan(rect2, thing, caravan);
		}
		rect2.width -= 24f;
		DoAbandonButtonCaravan(rect2, thing, caravan);
		rect2.width -= 24f;
		Widgets.InfoCardButton(rect2.width - 24f, (rect.height - 24f) / 2f, thing.AnyThing);
		rect2.width -= 24f;
		Rect rect3 = rect2;
		rect3.xMin = rect3.xMax - 60f;
		CaravanThingsTabUtility.DrawMass(thing, rect3);
		rect2.width -= 60f;
		Widgets.DrawHighlightIfMouseover(rect2);
		Rect rect4 = new Rect(4f, (rect.height - 27f) / 2f, 27f, 27f);
		Widgets.ThingIcon(rect4, thing.AnyThing);
		Rect rect5 = new Rect(rect4.xMax + 4f, 0f, 300f, 30f);
		Text.Anchor = TextAnchor.MiddleLeft;
		Text.WordWrap = false;
		Widgets.Label(rect5, thing.LabelCapWithTotalStackCount.Truncate(rect5.width));
		Text.Anchor = TextAnchor.UpperLeft;
		Text.WordWrap = true;
		GUI.EndGroup();
	}

	public void DoAbandonButtonCaravan(Rect rowRect, TransferableImmutable t, Caravan caravan)
	{
		Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
		if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.AbandonButtonTex))
		{
			foreach (var thing in t.things)
			{
				thing.holdingOwner.Remove(thing);
				this.storedThings.Add(thing);
			}
		}
		//if (Mouse.IsOver(rect))
		//{
		//	TooltipHandler.TipRegion(rect, () => CaravanAbandonOrBanishUtility.GetAbandonOrBanishButtonTooltip(t, abandonSpecificCount: false), Gen.HashCombineInt(t.GetHashCode(), 8476546));
		//}
	}


	public void DoAbandonSpecificCountButtonCaravan(Rect rowRect, TransferableImmutable t, Caravan caravan)
	{
		Rect rect = new Rect(rowRect.width - 24f, (rowRect.height - 24f) / 2f, 24f, 24f);
		if (Widgets.ButtonImage(rect, CaravanThingsTabUtility.AbandonSpecificCountButtonTex))
		{
			TryAbandonSpecificCountViaInterfaceCaravan(t, caravan);
		}
	}


	public void TryAbandonSpecificCountViaInterfaceCaravan(TransferableImmutable t, Caravan caravan)
	{
		Find.WindowStack.Add(new Dialog_Slider("AbandonSliderText".Translate(t.Label), 1, t.TotalStackCount, delegate (int x)
		{
			int num = x;
			for (int i = 0; i < t.things.Count; i++)
			{
				if (num <= 0)
				{
					break;
				}
				Thing thing = t.things[i];

				if (num >= thing.stackCount)
				{
					num -= thing.stackCount;
					thing.holdingOwner.Remove(thing);
					this.storedThings.Add(thing);
				}
				else
				{
					var newThing = thing.SplitOff(num);
					this.storedThings.Add(newThing);
					num = 0;
				}
			}
		}));
	}
	public string GetAbandonOrBanishButtonTooltipCaravan(TransferableImmutable t, bool abandonSpecificCount)
	{
		Pawn pawn = t.AnyThing as Pawn;
		if (pawn != null)
		{
			return PawnBanishUtility.GetBanishButtonTip(pawn);
		}
		return GetAbandonItemButtonTooltipCaravan(t.TotalStackCount, abandonSpecificCount);
	}

	private string GetAbandonItemButtonTooltipCaravan(int currentStackCount, bool abandonSpecificCount)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (currentStackCount == 1)
		{
			stringBuilder.AppendLine("AbandonTip".Translate());
		}
		else if (abandonSpecificCount)
		{
			stringBuilder.AppendLine("AbandonSpecificCountTip".Translate());
		}
		else
		{
			stringBuilder.AppendLine("AbandonAllTip".Translate());
		}
		stringBuilder.AppendLine();
		stringBuilder.Append("AbandonItemTipExtraText".Translate());
		return stringBuilder.ToString();
	}
	public void ExposeData()
	{
		Scribe_Collections.Look(ref storedThings, "storedThings", LookMode.Deep);
	}
}