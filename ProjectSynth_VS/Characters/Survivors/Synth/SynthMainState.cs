using EntityStates;
using ProjectSynth.Characters.Survivors.Synth.Components;
using ProjectSynth.Survivors.Synth;
using RoR2;

public class SynthMainState : GenericCharacterMain
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
