﻿using Verse;
using HarmonyLib;

namespace TazB_ScheduledBills
{

    public class Mod : Verse.Mod
    {
        //public static Settings settings;
        public Mod(ModContentPack content) : base(content)
        {
            // initialize settings
            // settings = GetSettings<Settings>();
#if DEBUG
            Harmony.DEBUG = true;
#endif

            Harmony harmony = new Harmony("com.tazb.scheduledbills");

            harmony.PatchAll();
        }

        //		public override void DoSettingsWindowContents(Rect inRect)
        //		{
        //			base.DoSettingsWindowContents(inRect);
        //			settings.DoWindowContents(inRect);
        //		}
        //
        //		public override string SettingsCategory()
        //		{
        //			return "Scheduled Bills";
        //		}
    }
}