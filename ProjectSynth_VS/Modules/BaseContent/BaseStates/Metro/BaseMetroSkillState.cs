using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Components;
using ProjectSynth.States.Synth.Metro;
using RoR2;

namespace ProjectSynth.Modules.BaseContent.BaseStates.Metro
{
    public abstract class BaseMetroSkillState : BaseSkillState
    {
        public virtual string MetroEsmName => "Metro";
        public virtual bool UseMetronome => true;
        public bool IsMetronomeHit { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();
            TryHandleMetronomeWindow();
        }

        private void TryHandleMetronomeWindow()
        {
            if (!UseMetronome || !characterBody) return;
            if (!SynthPassive.IsMetro(characterBody)) return;

            var esm = EntityStateMachine.FindByCustomName(characterBody.gameObject, MetroEsmName);
            if (!esm) return;

            if (esm.state is not BaseMetroState metroState) return;

            if (metroState.IsInTimingWindow)
            {
                OnMetronomeHit(metroState);
                metroState.EnterCooldownState();
            }
            else
            {
                OnMetronomeMiss(metroState);
                metroState.EnterMissedState();
            }
        }

        public virtual void OnMetronomeHit(BaseMetroState metroState)
        {
            IsMetronomeHit = true;
        }

        public virtual void OnMetronomeMiss(BaseMetroState metroState)
        {
            IsMetronomeHit = false;
        }
    }
}