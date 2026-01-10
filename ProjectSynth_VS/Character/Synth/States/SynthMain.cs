using EntityStates;
using ProjectSynth.Character.Synth.Content;

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

