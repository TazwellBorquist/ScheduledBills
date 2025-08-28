using RimWorld;
using HarmonyLib;
using Verse;

namespace TazB_ScheduledBills
{

    [HarmonyPatch(typeof(RimWorld.Planet.World), nameof(RimWorld.Planet.World.WorldTick))]
    class World_WorldTick_Patch
    {
        [HarmonyPostfix]
        static void TickExtraBillInfo()
        {
            ExtraBillInfo.Tick();
        }
    }



    [HarmonyPatch(typeof(Bill_Production), MethodType.Constructor)]
    class Bill_Production_Constructor_Patch
    {
        [HarmonyPostfix]
        static void AddExtraInfo(Bill_Production __instance)
        {
            ExtraBillInfo.AddBill(__instance);
        }
    }



    [HarmonyPatch(typeof(Bill_Production), nameof(Bill_Production.ShouldDoNow))]
    class Bill_Production_ShouldDoNow_Patch
    {
        [HarmonyPrefix]
        static bool CheckShouldDoNow(Bill_Production __instance, ref bool __result)
        {
            if (__instance.repeatMode != BillRepeatModeDefOf.TazB_XPerNDays)
                return false;

            __result = __instance.repeatCount > 0;

            return true;
        }
    }



    [HarmonyPatch(typeof(Bill_Production), nameof(Bill_Production.Notify_IterationCompleted))]
    class Bill_Production_IterationCompleted_Patch
    {
        // This one is going to be a bit funky, I don't really want to transpile it so I'm just gonna be lazy
        [HarmonyPrefix]
        static void SetModeState(Bill_Production __instance, BillRepeatModeDef __state)
        {
            __state = __instance.repeatMode;
            if (__instance.repeatMode == BillRepeatModeDefOf.TazB_XPerNDays)
            {
                __instance.repeatMode = RimWorld.BillRepeatModeDefOf.RepeatCount;
            }
        }

        [HarmonyPostfix]
        static void SetRepeatModeFromState(Bill_Production __instance, BillRepeatModeDef __state)
        {
            __instance.repeatMode = __state;
        }
    }

    [HarmonyPatch(typeof(Bill_Production), nameof(Bill_Production.RepeatInfoText), MethodType.Getter)]
    class Bill_Production_RepeatInfoText_Patch
    {
        [HarmonyPrefix]
        static bool ScheduledBillsText(Bill_Production __instance, ref string __result)
        {
            if (__instance.repeatMode == BillRepeatModeDefOf.TazB_XPerNDays)
            {
                BillInfo billInfo = ExtraBillInfo.GetInfo(__instance);
                Date date = null;
                int amount = -1;

                if (billInfo != null)
                {
                    date = billInfo.RepeatsOn;
                    amount = billInfo.RepeatCount;
                }
                else
                    date = Date.InvalidDate;

                __result = String.Format("Producing {0} on {1}", amount, date.ToNextResetDayString());
                return true;
            }

            return false;
        }
    }
}