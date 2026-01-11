using HarmonyLib;
using ProjectSynth.Character.Synth.Content.Items;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSynth.Core.Patches
{

    [HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.attackSpeed), MethodType.Getter)]
    class FreezeAttackSpeed
    {
        static void Postfix(CharacterBody __instance, ref float __result)
        {
            if (__instance.inventory?.GetItemCountEffective(Passive.Metronome) >= 1)
            {
                VirtualAttackSpeedManager.Set(__instance, __result);

                __result = __instance.baseAttackSpeed;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterBody), nameof(CharacterBody.OnDeathStart), MethodType.Normal)]
    public class CleanupFreezAttackSpeed
    {
        static void Postfix(CharacterBody __instance)
        {
            VirtualAttackSpeedManager.Remove(__instance);
        }
    }
}
