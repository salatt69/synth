using R2API;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthDamageTypes
    {
        public static DamageAPI.ModdedDamageType EncoreDamage;

        public static void Register()
        {
            EncoreDamage = DamageAPI.ReserveDamageType();
        }
    }
}
