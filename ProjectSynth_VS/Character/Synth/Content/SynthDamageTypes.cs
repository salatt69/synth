using R2API;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthDamageTypes
    {
        public static DamageAPI.ModdedDamageType Encore { get; set; }
        public static DamageAPI.ModdedDamageType CultureShock { get; set; }

        public static void Register()
        {
            Encore = DamageAPI.ReserveDamageType();
            CultureShock = DamageAPI.ReserveDamageType();
        }
    }
}
