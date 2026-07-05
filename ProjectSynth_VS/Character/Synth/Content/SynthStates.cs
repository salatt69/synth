using ProjectSynth.States;
using ProjectSynth.States.Synth;
using ProjectSynth.States.Synth.Diva;
using ProjectSynth.States.Synth.Metro;
using ProjectSynth.States.Synth.Weapon;
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
            ContentAddition.AddEntityState(typeof(TNM), out _);
            ContentAddition.AddEntityState(typeof(RollingGirl), out _);
            ContentAddition.AddEntityState(typeof(Backflip), out _);
            ContentAddition.AddEntityState(typeof(DeployDiva), out _);
            ContentAddition.AddEntityState(typeof(GroundSlam), out _);
            ContentAddition.AddEntityState(typeof(LeapTowardsDiva), out _);
            ContentAddition.AddEntityState(typeof(MikuBeamLeap), out _);
            ContentAddition.AddEntityState(typeof(MikuBeamSustain), out _);

            // DivaBase
            ContentAddition.AddEntityState(typeof(Arm), out _);
            ContentAddition.AddEntityState(typeof(StunningPerformance), out _);
            ContentAddition.AddEntityState(typeof(WaitForStick), out _);

            // DivaArming
            ContentAddition.AddEntityState(typeof(DivaArmingUnarmed), out _);
            ContentAddition.AddEntityState(typeof(DivaArmingArmed), out _);

            // DivaStunning
            ContentAddition.AddEntityState(typeof(CultureShockState), out _);

            // Metro
            ContentAddition.AddEntityState(typeof(MetroWaitForInputState), out _);
            ContentAddition.AddEntityState(typeof(MetroCooldownState), out _);
            ContentAddition.AddEntityState(typeof(MetroMissedState), out _);
        }
    }
}
