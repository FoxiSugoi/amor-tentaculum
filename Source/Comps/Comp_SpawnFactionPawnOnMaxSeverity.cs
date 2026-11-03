using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Sound;
using System.Text;

namespace AmorTentaculum
{

    // Cloned from VFE's HediffComp_SpawnPawnOnMaxSeverity, but modified to spawn a friendly pawn.
    public class HediffCompProperties_SpawnFactionPawnOnMaxSeverity : HediffCompProperties
    {
        public List<PawnKindDef> pawnKindOptions = new List<PawnKindDef>();
        public ThingDef filthCreated;
        public IntRange filthCountRange;
        public SoundDef sound;
        public DamageDef damage;
        public FloatRange damageAmount;

        public HediffCompProperties_SpawnFactionPawnOnMaxSeverity()
        {
            this.compClass = typeof(HediffComp_SpawnFactionPawnOnMaxSeverity);
        }
    }

    public class HediffComp_SpawnFactionPawnOnMaxSeverity : HediffComp
    {
        public HediffCompProperties_SpawnFactionPawnOnMaxSeverity Props => base.props as HediffCompProperties_SpawnFactionPawnOnMaxSeverity;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (parent.Severity >= 0.99f)
            {
                for (int i = 0; i < Props.pawnKindOptions.Count; i++)
                {
                    PawnKindDef kind = Props.pawnKindOptions[i];
                    Faction faction = Faction.OfPlayer;
                    float? fixedBiologicalAge = 0f;
                    Pawn child = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, fixedBiologicalAge: fixedBiologicalAge));
                    SpawnPawn(child, Pawn, Pawn.PositionHeld, Pawn.MapHeld);
                }

                for (int i = 0; i < Props.filthCountRange.RandomInRange; i++)
                {
                    IntVec3 c;
                    CellFinder.TryFindRandomReachableNearbyCell(Pawn.PositionHeld, Pawn.MapHeld, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                    FilthMaker.TryMakeFilth(c, Pawn.MapHeld, Props.filthCreated);
                }
                if (Props.sound != null)
                {
                    Props.sound.PlayOneShot(new TargetInfo(Pawn.PositionHeld, Pawn.MapHeld, false));
                }
                Pawn.TakeDamage(new DamageInfo(Props.damage, Props.damageAmount.RandomInRange, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));

                Pawn.health.RemoveHediff(this.parent);
            }
        }


        private void SpawnPawn(Pawn child, Pawn parent, IntVec3 position, Map map)
        {
            GenSpawn.Spawn(child, position, map, WipeMode.VanishOrMoveAside);

            FleshbeastUtility.SpawnPawnAsFlyer(child, map, position);
        }
    }

}