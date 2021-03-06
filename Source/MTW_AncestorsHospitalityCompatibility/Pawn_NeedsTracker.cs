﻿using RimWorld;
using Verse;
using Source = RimWorld.Pawn_NeedsTracker;
using AncestorUtils = MTW_AncestorSpirits.AncestorUtils;

using System.Linq;
using System.Reflection;

namespace MTW_AncestorsHospitalityCompatibility
{
    // Copied from Hospitality - MTW
    /// <summary>
    /// Added Joy and Comfort to guests
    /// </summary>
    internal static class Pawn_NeedsTracker
    {
        private static readonly NeedDef defComfort = DefDatabase<NeedDef>.GetNamed("Comfort");
        private static readonly NeedDef defBeauty = DefDatabase<NeedDef>.GetNamed("Beauty");
        private static readonly NeedDef defSpace = DefDatabase<NeedDef>.GetNamed("Space");

        [Detour(typeof(Source), bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance)]
        public static bool ShouldHaveNeed(this Source _this, NeedDef nd)
        {
            Pawn pawn = (Pawn)typeof(RimWorld.Pawn_NeedsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_this);

            // BASE
            if (pawn.RaceProps.intelligence < nd.minIntelligence)
            {
                return false;
            }

            /************************************
            * Begin MTW_AncestorSpirits Section *
            *************************************/
            if (nd == NeedDefOf.Rest && AncestorUtils.IsAncestor(pawn)) // ADDED
            {
                return false;
            }
            if ((nd == NeedDefOf.Joy || nd == defBeauty || nd == defSpace) && AncestorUtils.IsAncestor(pawn))
            {
                return true;
            }
            /**********************************
            * End MTW_AncestorSpirits Section *
            ***********************************/

            /****************************
            * Begin Hospitality Section *
            *****************************/
            // The isGuest method is internal, so here are some Shenanigans to subvert that.
            var isGuestMethod = typeof(Hospitality.Building_GuestBed).Assembly
                .GetType("Hospitality.GuestUtility")
                .GetMethod("IsGuest", BindingFlags.Public | BindingFlags.Static);
            bool isGuest = (bool)isGuestMethod.Invoke(null, new object[] { pawn });

            if ((nd == NeedDefOf.Joy || nd == defComfort || nd == defBeauty || nd == defSpace) && isGuest) // ADDED
            {
                return true;
            }
            /**************************
            * End Hospitality Section *
            ***************************/

            if (nd.colonistsOnly && (pawn.Faction == null || !pawn.Faction.IsPlayer))
            {
                return false;
            }
            if (nd.colonistAndPrisonersOnly && (pawn.Faction == null || !pawn.Faction.IsPlayer) && (pawn.HostFaction == null || pawn.HostFaction != Faction.OfPlayer))
            {
                return false;
            }
            if (nd.onlyIfCausedByHediff && !pawn.health.hediffSet.hediffs.Any((Hediff x) => x.def.causesNeed == nd))
            {
                return false;
            }
            if (nd == NeedDefOf.Food)
            {
                return pawn.RaceProps.EatsFood;
            }

            return nd != NeedDefOf.Rest || pawn.RaceProps.needsRest;
        }
    }
}
