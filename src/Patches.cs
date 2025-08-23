using RimWorld;
using HarmonyLib;
using Verse;

namespace TazB_ScheduledBills;


[HarmonyPatch(typeof(RimWorld.Planet.World), nameof(RimWorld.Planet.World.WorldTick))]
class World_WorldTick_Patch
{
    [HarmonyPrefix]
    static void TickExtraBillInfo()
    {
        ExtraBillInfo.Tick();
    }
}



[HarmonyPatch(typeof(Bill_Production), MethodType.Constructor)]
class BillStack_Add_Patch
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
    static void SetModeState(Bill_Production __instance, out BillRepeatModeDef __state)
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