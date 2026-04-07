using RoR2;

namespace ProjectSynth.States.Synth.Metro
{
    public sealed class MetroCooldownState : BaseMetroState
    {
        private const int CooldownBeats = 3;
        private const float AnimEarlyOffsetBeats = 0.30f;
        private int startBeatIndex;

        public override void OnEnter()
        {
            base.OnEnter();
            IsOnCooldown = true;

            metro.ongoing = true;
            startBeatIndex = metro.beatIndex;

            float speed = (metro.speedMult > 0f) ? metro.speedMult : 2f;
            float animBeats = CooldownBeats + AnimEarlyOffsetBeats;
            metro.cooldownSpeedMult = speed / animBeats;
            metro.cooldownStartedThisFrame = true;
        }

        public override void Update()
        {
            base.Update();
            if (metro.beatIndex - startBeatIndex >= CooldownBeats)
            {
                outer.SetNextState(new MetroWaitForInputState());
            }
        }
    }
}