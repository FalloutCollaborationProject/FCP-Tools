using System.Text;
using UnityEngine;
using Verse.AI;

namespace RangerRick_PowerArmor;

public class CompPowerArmorStation : ThingComp, IThingHolder, ISearchableContents
{
	private struct CachedGraphicRenderInfo
	{
		public Graphic graphic;

		public int layer;

		public Vector3 scale;

		public Vector3 positionOffset;

		public CachedGraphicRenderInfo(Graphic graphic, int layer, Vector3 scale, Vector3 positionOffset)
		{
			this.graphic = graphic;
			this.layer = layer;
			this.scale = scale;
			this.positionOffset = positionOffset;
		}
	}

	private ThingOwner<Thing> innerContainer;

	private readonly List<CachedGraphicRenderInfo> cachedApparelGraphicsHeadgear = new List<CachedGraphicRenderInfo>();

	private readonly List<CachedGraphicRenderInfo> cachedApparelGraphicsNonHeadgear = new List<CachedGraphicRenderInfo>();

	private bool cachedApparelRenderInfoSkipHead;

	private bool graphicsDirty = true;

	private Dictionary<ThingDef, float> accumulatedResourceCosts = new Dictionary<ThingDef, float>();

	public CompProperties_PowerArmorStation Props => (CompProperties_PowerArmorStation)props;

	public IReadOnlyList<Apparel> HeldApparels => innerContainer.InnerListForReading.OfType<Apparel>().ToList();

	public ThingOwner SearchableContents => innerContainer;

	public CompPowerArmorStation()
	{
		innerContainer = new ThingOwner<Thing>(this);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		base.PostSpawnSetup(respawningAfterLoad);
		graphicsDirty = true;
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode != DestroyMode.WillReplace)
		{
			innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
		}
		base.PostDeSpawn(map, mode);
	}

	public override void PostDraw()
	{
		if (graphicsDirty)
		{
			RecacheGraphics();
		}
		if (!innerContainer.Any)
		{
			return;
		}
		Rot4 rot = parent.Rotation;
		Vector3 drawLoc = parent.DrawPos;
		Vector3 offset = Props.armorDrawOffset;
		offset = offset.RotatedBy(rot.AsAngle);
		Vector3 pos = drawLoc + offset;
		Mesh mesh = MeshPool.GetMeshSetForSize(1.5f, 1.5f).MeshAt(rot);
		foreach (CachedGraphicRenderInfo item in cachedApparelGraphicsNonHeadgear)
		{
			Vector3 vector = pos;
			vector.y = AltitudeLayer.Item.AltitudeFor() + PawnRenderUtility.AltitudeForLayer(item.layer);
			Material material = item.graphic.MatAt(rot);
			Vector3 vector2 = item.graphic.DrawOffset(rot) + item.positionOffset;
			Quaternion q = item.graphic.QuatFromRot(rot);
			Matrix4x4 matrix = Matrix4x4.TRS(vector + vector2, q, item.scale);
			Graphics.DrawMesh(mesh, matrix, material, 0);
		}
		foreach (CachedGraphicRenderInfo item2 in cachedApparelGraphicsHeadgear)
		{
			Vector3 vector3 = pos + HeadOffsetAt(rot);
			vector3.y = AltitudeLayer.ItemImportant.AltitudeFor() + PawnRenderUtility.AltitudeForLayer(item2.layer);
			Material material2 = item2.graphic.MatAt(rot);
			Vector3 vector4 = item2.graphic.DrawOffset(rot) + item2.positionOffset;
			Quaternion q2 = item2.graphic.QuatFromRot(rot);
			Matrix4x4 matrix2 = Matrix4x4.TRS(vector3 + vector4, q2, item2.scale);
			Graphics.DrawMesh(mesh, matrix2, material2, 0);
		}
	}

	private Vector3 HeadOffsetAt(Rot4 rotation)
	{
		Vector2 headOffset = BodyTypeDefOf.Hulk.headOffset;
		return rotation.AsInt switch
		{
			0 => new Vector3(0f, 0f, headOffset.y),
			1 => new Vector3(headOffset.x, 0f, headOffset.y),
			2 => new Vector3(0f, 0f, headOffset.y),
			3 => new Vector3(0f - headOffset.x, 0f, headOffset.y),
			_ => Vector3.zero,
		};
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		Scribe_Collections.Look(ref accumulatedResourceCosts, "accumulatedResourceCosts", LookMode.Def, LookMode.Value);

		if (Scribe.mode == LoadSaveMode.PostLoadInit && accumulatedResourceCosts == null)
		{
			accumulatedResourceCosts = new Dictionary<ThingDef, float>();
		}
	}

	public override void CompTick()
	{
		innerContainer.DoTick();
	}

	public Apparel GetApparelForLayer(ApparelLayerDef layer)
	{
		foreach (Thing item in innerContainer)
		{
			if (item is Apparel apparel && apparel.def.apparel.LastLayer == layer)
			{
				return apparel;
			}
		}
		return null;
	}

	private void RecacheGraphics()
	{
		cachedApparelGraphicsHeadgear.Clear();
		cachedApparelGraphicsNonHeadgear.Clear();
		cachedApparelRenderInfoSkipHead = false;
		List<Apparel> apparelList = [.. innerContainer.InnerListForReading.OfType<Apparel>()];
		apparelList.SortBy(a => a.def.apparel.LastLayer.drawOrder);
		Dictionary<ApparelLayerDef, int> dictionary = new Dictionary<ApparelLayerDef, int>();
		foreach (Apparel apparel in apparelList)
		{
			ApparelLayerDef lastLayer = apparel.def.apparel.LastLayer;
			bool flag = lastLayer == ApparelLayerDefOf.Overhead || lastLayer == ApparelLayerDefOf.EyeCover;
			cachedApparelRenderInfoSkipHead = cachedApparelRenderInfoSkipHead || apparel.def.apparel.renderSkipFlags.NotNullAndContains(RenderSkipFlagDefOf.Head);
			if (!ApparelGraphicRecordGetter.TryGetGraphicApparel(apparel, BodyTypeDefOf.Hulk, forStatue: false, out var rec))
			{
				continue;
			}
			int valueOrDefault = CollectionExtensions.GetValueOrDefault<ApparelLayerDef, int>((IReadOnlyDictionary<ApparelLayerDef, int>)dictionary, lastLayer, 0);
			dictionary[lastLayer] = valueOrDefault + 1;
			int num = lastLayer.drawOrder / 10 + valueOrDefault;
			float? num2 = apparel.def.apparel.drawData?.LayerForRot(parent.Rotation, -1f);
			if (num2.HasValue)
			{
				float valueOrDefault2 = num2.GetValueOrDefault();
				if (valueOrDefault2 > 0f)
				{
					int num3 = (flag ? 70 : 20);
					num += (int)valueOrDefault2 - num3;
				}
			}
			Vector3 one = Vector3.one;
			Vector3 zero = Vector3.zero;
			if (apparel.RenderAsPack())
			{
				Vector2 vector = apparel.def.apparel.wornGraphicData.BeltScaleAt(parent.Rotation, BodyTypeDefOf.Hulk);
				one.x *= vector.x;
				one.z *= vector.y;
				Vector2 vector2 = apparel.def.apparel.wornGraphicData.BeltOffsetAt(parent.Rotation, BodyTypeDefOf.Hulk);
				zero.x += vector2.x;
				zero.z += vector2.y;
				if (parent.Rotation == Rot4.North)
				{
					num = 93;
				}
				else if (parent.Rotation == Rot4.South)
				{
					num = -3;
				}
			}
			if (flag)
			{
				cachedApparelGraphicsHeadgear.Add(new CachedGraphicRenderInfo(rec.graphic, num, one, zero));
			}
			else
			{
				cachedApparelGraphicsNonHeadgear.Add(new CachedGraphicRenderInfo(rec.graphic, num, one, zero));
			}
		}
		graphicsDirty = false;
	}

	public bool HasRoomForApparel(Apparel apparel)
	{
		ApparelLayerDef layer = apparel.def.apparel.LastLayer;
		if (!Props.allowedLayers.Contains(layer))
		{
			return false;
		}
		Apparel existing = GetApparelForLayer(layer);
		return existing == null;
	}

	public bool IsPowerArmorApparel(Apparel apparel)
	{
		return Props.allowedLayers.Contains(apparel.def.apparel.LastLayer);
	}

	public bool EquipApparel(Pawn pawn, Apparel apparel)
	{
		if (innerContainer.Remove(apparel))
		{
			graphicsDirty = true;
			pawn.apparel.Wear(apparel);
			return true;
		}
		return false;
	}

	public bool StoreApparel(Pawn pawn, Apparel apparel)
	{
		pawn.apparel.Remove(apparel);
		ApparelLayerDef layer = apparel.def.apparel.LastLayer;
		Apparel existing = GetApparelForLayer(layer);
		if (existing != null)
		{
			innerContainer.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near);
		}
		if (apparel.Spawned)
		{
			apparel.DeSpawn();
		}
		apparel.holdingOwner?.Remove(apparel);
		if (innerContainer.TryAdd(apparel))
		{
			graphicsDirty = true;
			return true;
		}
		return false;
	}

	public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		if (!selPawn.IsColonistPlayerControlled)
		{
			yield break;
		}
		if (!selPawn.CanReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
		{
			yield return new FloatMenuOption("FCP_CannotUsePowerArmorStation".Translate() + ": " + "NoPath".Translate(), null);
			yield break;
		}
		bool hasApparelToStore = selPawn.apparel.WornApparel.Any(wornApparel =>
			IsPowerArmorApparel(wornApparel) &&
			(wornApparel.GetComp<CompApparelRequirement>()?.HasRequiredTrait(selPawn) ?? true)
		);
		if (hasApparelToStore)
		{
			yield return new FloatMenuOption("FCP_StorePowerArmor".Translate().CapitalizeFirst(), delegate
			{
				selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(RR_DefOf.FCP_StorePowerArmor, parent), JobTag.Misc);
			});
		}
		var cantReason = GetEquipCantReason(selPawn);
		if (cantReason.Accepted || cantReason.Reason.NullOrEmpty() is false)
		{
			if (innerContainer.Any)
			{
				yield return CreateFloatMenuOption("FCP_EquipFromStation".Translate().CapitalizeFirst(), RR_DefOf.FCP_EquipFromStation, cantReason, selPawn);
			}
			if (hasApparelToStore && innerContainer.Any)
			{
				yield return CreateFloatMenuOption("FCP_SwapPowerArmor".Translate().CapitalizeFirst(), RR_DefOf.FCP_SwapPowerArmor, cantReason, selPawn);
			}
		}
	}

	private FloatMenuOption CreateFloatMenuOption(string label, JobDef jobDef, AcceptanceReport cantReason, Pawn pawn)
	{
		if (cantReason.Accepted)
		{
			return new FloatMenuOption(label, delegate
			{
				pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(jobDef, parent), JobTag.Misc);
			});
		}
		return new FloatMenuOption(label + ": " + cantReason.Reason, null);
	}

	private AcceptanceReport GetEquipCantReason(Pawn pawn)
	{
		foreach (Thing item in innerContainer)
		{
			if (item is Apparel apparel)
			{
				if (!ApparelUtility.HasPartsToWear(pawn, apparel.def))
				{
					return false;
				}
				var reqComp = item.TryGetComp<CompApparelRequirement>();
				if (reqComp != null && !reqComp.HasRequiredTrait(pawn))
				{
					return "RR.RequiresTrait".Translate(reqComp.Props.requiredTrait.degreeDatas[0].label);
				}
			}
		}
		return true;
	}

	public bool RepairTick(Pawn pawn, List<Thing> resources)
	{
		foreach (Apparel apparel in HeldApparels)
		{
			if (apparel.HitPoints < apparel.MaxHitPoints)
			{
				var repairComp = apparel.GetComp<CompRepairableAtStation>();
				if (repairComp != null && repairComp.Props.repairResourcesPerHP != null && repairComp.Props.repairResourcesPerHP.Count > 0)
				{
					foreach (ThingDefFloatClass repairResource in repairComp.Props.repairResourcesPerHP)
					{
						if (!accumulatedResourceCosts.ContainsKey(repairResource.thingDef))
						{
							accumulatedResourceCosts[repairResource.thingDef] = 0f;
						}
						accumulatedResourceCosts[repairResource.thingDef] += repairResource.count;
					}

					foreach (var kvp in accumulatedResourceCosts.ToList())
					{
						int wholeUnits = Mathf.FloorToInt(kvp.Value);
						if (wholeUnits > 0)
						{
							int remaining = wholeUnits;
							foreach (Thing resource in resources.Where(r => r?.def == kvp.Key))
							{
								if (resource == null) continue;

								int toConsume = Mathf.Min(remaining, resource.stackCount);
								resource.SplitOff(toConsume).Destroy();
								remaining -= toConsume;

								if (remaining <= 0) break;
							}
							accumulatedResourceCosts[kvp.Key] -= (wholeUnits - remaining);
						}
					}

					apparel.HitPoints++;
					if (apparel.HitPoints >= apparel.MaxHitPoints)
					{
						pawn.records.Increment(RecordDefOf.ThingsRepaired);
						accumulatedResourceCosts.Clear();
					}
					return true;
				}
			}
		}
		return false;
	}

	public override string CompTipStringExtra()
	{
		if (!innerContainer.Any)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("FCP_StoredApparel".Translate());
		foreach (Apparel apparel in HeldApparels)
		{
			stringBuilder.AppendLine("  - " + apparel.LabelCap);
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}

	public void ClearAccumulatedCosts()
	{
		accumulatedResourceCosts.Clear();
	}
}
