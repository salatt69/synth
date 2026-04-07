using EntityStates;
using ProjectSynth.Components;
using RoR2;

namespace ProjectSynth.States.Synth.Metro
{
    public abstract class BaseMetroState : BaseState
    {
        protected SynthMetroRuntime metro;
        public bool IsOnCooldown { get; protected set; }

        public override void OnEnter()
        {
            base.OnEnter();

            metro = gameObject.GetComponent<SynthMetroRuntime>();
        }

        public bool IsInTimingWindow => metro != null && metro.timingWindowOpen;

        // idk if grade will be used for anything
        // but maybe someday it could be used to determine the strength of the boost or something
        protected MetroGrade Grade => metro != null ? metro.grade : MetroGrade.None;

        protected float GetTimerAnimSpeedForBeats(float beats, float fallbackSpeedMult = 2f)
        {
            float speed = (metro != null && metro.speedMult > 0f) ? metro.speedMult : fallbackSpeedMult;
            return speed / beats;
        }

        protected EntityState GetCurrentSkillState()
        {
            if (!metro?.body) return null;

            GenericSkill skill = null;
            InputBankTest input = metro.body.inputBank;
            SkillLocator loc = metro.body.skillLocator;

            if (input == null || loc == null) return null;

            if (input.skill1.justPressed == true) skill = loc.primary;
            if (input.skill2.justPressed == true) skill = loc.secondary;
            if (input.skill3.justPressed == true) skill = loc.utility;
            if (input.skill4.justPressed == true) skill = loc.special;

            if (!skill) return null;

            var esm = EntityStateMachine.FindByCustomName(metro.body.gameObject, skill.skillDef.activationStateMachineName);
            return esm ? esm.state : null;
        }

        public void EnterCooldownState()
        {
            outer.SetNextState(new MetroCooldownState());
        }

        public void EnterMissedState()
        {
            outer.SetNextState(new MetroMissedState());
        }
    }
}