using EntityStates;
using ProjectSynth.Character.Synth.Content;
using RoR2;

namespace ProjectSynth.Character.Synth.States
{
    public class SynthMain : GenericCharacterMain
    {
        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.sprintCrosshairPrefabOverride = SynthAssets.synthCrosshair;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}

