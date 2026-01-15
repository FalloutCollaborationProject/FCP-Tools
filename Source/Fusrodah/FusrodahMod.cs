using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Fusrodah
{
	public class Extension : DefModExtension
	{
		public FloatRange rangeToLand;
		public DamageDef damageToApply;
		public float damageAmount;
		public bool directKnockback = false;
		public float buildingCollisionDamage = 10f;
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class HotSwappableAttribute : Attribute
	{
	}
	
	[HotSwappable]
	public class DamageWorker_Fusrodah : DamageWorker
	{
		public override DamageResult Apply(DamageInfo dinfo, Thing victim)
		{
			var result = base.Apply(dinfo, victim);
			if (victim.DestroyedOrNull() is false)
			{
				var props = dinfo.Def.GetModExtension<Extension>();
				if (props.damageToApply != null)
				{
					victim.TakeDamage(new DamageInfo(props.damageToApply, props.damageAmount,
						instigator: dinfo.instigatorInt, weapon: dinfo.Weapon));
				}

				if (victim is Pawn victimPawn && victimPawn.MapHeld != null && dinfo.Instigator != null)
				{
					IntVec3 targetCell;
					if (props.directKnockback)
					{
						Vector3 direction = (victim.DrawPos - dinfo.Instigator.DrawPos).normalized;
						float distance = props.rangeToLand.RandomInRange;

						// Calculate target position along the direct line
						Vector3 throwVector = direction * distance;
						targetCell = new IntVec3(
							victim.Position.x + (int)throwVector.x,
							0,
							victim.Position.z + (int)throwVector.z
						);

						// Ensure the cell is within map bounds
						targetCell.x = Mathf.Clamp(targetCell.x, 0, victimPawn.MapHeld.Size.x - 1);
						targetCell.z = Mathf.Clamp(targetCell.z, 0, victimPawn.MapHeld.Size.z - 1);

					}
					else 
					{
						targetCell = GenRadial.RadialCellsAround(victim.PositionHeld, props.rangeToLand.max, true)
						   .Where(x => x.DistanceTo(victim.PositionHeld) >= props.rangeToLand.min && x.InBounds(victimPawn.MapHeld)
						   && GenSight.LineOfSight(x, victim.PositionHeld, victimPawn.MapHeld)).RandomElement();
					}

					// Check for building collision along the path
					var path = GenSight.PointsOnLineOfSight(victim.Position, targetCell).ToList();
					bool hitImpassable = false;
					IntVec3 collisionPoint = targetCell;

					for (int i = 0; i < path.Count; i++)
					{
						Building building = path[i].GetFirstBuilding(victimPawn.MapHeld);
						if (building != null)
						{
							if (building.def.passability == Traversability.Impassable)
							{                           // Apply damage regardless of passability
								victim.TakeDamage(new DamageInfo(DamageDefOf.Blunt, props.buildingCollisionDamage,
									instigator: null, weapon: null));
								hitImpassable = true;
								collisionPoint = path[i];
								break;
							}
						}
					}

					if (hitImpassable)
					{
						// Stop at the collision point only if hit something impassable
						targetCell = path[Mathf.Max(0, path.IndexOf(collisionPoint) - 1)];
					}

					// Move the pawn or corpse
					if (victimPawn.Corpse != null)
					{
						victimPawn.Corpse.Position = targetCell;
					}
					else
					{
						victimPawn.pather?.StopDead();
						victimPawn.jobs?.StopAll();
						victimPawn.Position = targetCell;
					}
				}
			}
			return result;
		}
	}
}
