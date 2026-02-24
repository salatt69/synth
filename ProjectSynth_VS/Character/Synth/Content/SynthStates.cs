using ProjectSynth.Character.Synth.States;
using ProjectSynth.Character.Synth.States.Hologram;
using ProjectSynth.Character.Synth.States.Override;
using ProjectSynth.Character.Synth.States.Primary;
using ProjectSynth.Character.Synth.States.Secondary;
using ProjectSynth.Character.Synth.States.Special;
using ProjectSynth.Character.Synth.States.Utility;
using ProjectSynth.Hologram;
using R2API;

namespace ProjectSynth.Character.Synth.Content
{
    public static class SynthStates
    {
        public static void Init()
        {
            // Main
            ContentAddition.AddEntityState(typeof(SynthMain), out _);

            // Skills            
            ContentAddition.AddEntityState(typeof(SlashCombo), out _);
            ContentAddition.AddEntityState(typeof(Shoot), out _);
            ContentAddition.AddEntityState(typeof(Roll), out _);
            ContentAddition.AddEntityState(typeof(ThrowBomb), out _);
            ContentAddition.AddEntityState(typeof(ThirtyNineMusic), out _);
            ContentAddition.AddEntityState(typeof(SonicBoom), out _);
            ContentAddition.AddEntityState(typeof(DeployDiva), out _);
            ContentAddition.AddEntityState(typeof(DivaTeleport), out _);

            // DivaBase
            ContentAddition.AddEntityState(typeof(Arm), out _);
            ContentAddition.AddEntityState(typeof(StunningPerformance), out _);
            ContentAddition.AddEntityState(typeof(WaitForStick), out _);

            // DivaArming
            ContentAddition.AddEntityState(typeof(DivaArmingUnarmed), out _);
            ContentAddition.AddEntityState(typeof(DivaArmingArmed), out _);

            // DivaStunning
            ContentAddition.AddEntityState(typeof(CultureShockState), out _);
        }
    }
}
