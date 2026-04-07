using RoR2;

namespace ProjectSynth.States.Synth.Metro
{
    public sealed class MetroWaitForInputState : BaseMetroState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            IsOnCooldown = false;
        }

        public override void Update()
        {
            base.Update();
        }
    }
}