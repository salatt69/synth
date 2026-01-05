using ProjectSynth.Survivors.Synth.SkillStates;
using R2API;

namespace ProjectSynth.Survivors.Synth
{
    public static class SynthStates
    {
        public static void Init()
        {
            ContentAddition.AddEntityState(typeof(SynthMainState), out _);

            ContentAddition.AddEntityState(typeof(SlashCombo), out _);

            ContentAddition.AddEntityState(typeof(Shoot), out _);

            ContentAddition.AddEntityState(typeof(Roll), out _);

            ContentAddition.AddEntityState(typeof(ThrowBomb), out _);

            ContentAddition.AddEntityState(typeof(ThirtyNineMusic), out _);
        }
    }
}
