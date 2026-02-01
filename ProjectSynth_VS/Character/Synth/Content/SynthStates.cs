using ProjectSynth.Character.Synth.States;
using ProjectSynth.Character.Synth.States.Override;
using ProjectSynth.Character.Synth.States.Primary;
using ProjectSynth.Character.Synth.States.Secondary;
using ProjectSynth.Character.Synth.States.Special;
using ProjectSynth.Character.Synth.States.Utility;
using R2API;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthStates
    {
        public static void Init()
        {
            ContentAddition.AddEntityState(typeof(SynthMain), out _);
            
            ContentAddition.AddEntityState(typeof(SlashCombo), out _);
            
            ContentAddition.AddEntityState(typeof(Shoot), out _);

            ContentAddition.AddEntityState(typeof(Roll), out _);

            ContentAddition.AddEntityState(typeof(ThrowBomb), out _);

            ContentAddition.AddEntityState(typeof(ThirtyNineMusic), out _);
            
            ContentAddition.AddEntityState(typeof(SonicBoom), out _);

            ContentAddition.AddEntityState(typeof(ExpoNade), out _);
            
            ContentAddition.AddEntityState(typeof(ExpoShift), out _);
        }
    }
}
