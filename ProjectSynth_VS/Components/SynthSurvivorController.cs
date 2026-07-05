using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Mod;
using ProjectSynth.States.Synth;
using ProjectSynth.States.Synth.Weapon;
using RoR2;
using RoR2.HudOverlay;
using RoR2.Skills;
using System.Collections;
using UnityEngine;

namespace ProjectSynth.Components
{
    public class SynthSurvivorController : MonoBehaviour
    {
        public GameObject overlayPrefab;
        public string childLocatorEntry = "CrosshairExtras";

        private OverlayController overlayController;
        private CharacterBody characterBody;
        private EntityStateMachine bodyStateMachine;
        private EntityStateMachine weaponStateMachine;
        private SkillLocator skillLocator;

        void OnEnable()
        {
            Log.Info("Wait for overlay decision... (1 frame)");
            StartCoroutine(EnsureOverlay());

            characterBody = gameObject.GetComponent<CharacterBody>();
            bodyStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Body");
            weaponStateMachine = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
            skillLocator = gameObject.GetComponent<SkillLocator>();
        }

        void OnDisable()
        {
            if (overlayController != null)
            {
                //overlayController.onInstanceAdded -= OnOverlayInstanceAdded;
                //overlayController.onInstanceRemove -= OnOverlayInstanceRemoved;
                HudOverlayManager.RemoveOverlay(overlayController);
            }
        }

        void FixedUpdate()
        {
            if (bodyStateMachine.state is SynthMain)
            {
                if (AllowGroundSlam() || weaponStateMachine.state is GroundSlam)
                {
                    skillLocator.utility.SetSkillOverride(this, SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Ground Slam")), GenericSkill.SkillOverridePriority.Contextual);
                }
                else
                {
                    skillLocator.utility.UnsetSkillOverride(this, SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Ground Slam")), GenericSkill.SkillOverridePriority.Contextual);
                }
            }
        }

        IEnumerator EnsureOverlay()
        {
            yield return null;

            overlayPrefab = DeciedeOverlay();

            OverlayCreationParams overlayParams = new()
            {
                prefab = overlayPrefab,
                childLocatorEntry = childLocatorEntry
            };
            overlayController = HudOverlayManager.AddOverlay(gameObject, overlayParams);
            //overlayController.onInstanceAdded += OnOverlayInstanceAdded;
            //overlayController.onInstanceRemove += OnOverlayInstanceRemoved;
        }

        private void OnOverlayInstanceAdded(OverlayController controller, GameObject instance)
        {

        }

        private void OnOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {

        }

        private GameObject DeciedeOverlay()
        {
            var body = GetComponent<CharacterBody>();
            bool hasMetro = SynthPassive.IsMetro(body);
            Log.Info($"Overlay decided! Has metronome: {hasMetro}");
            return hasMetro ? SynthAssets.synthMetroOverlay : SynthAssets.synthRushOverlay;
        }

        private bool AllowGroundSlam(float maxDistance = 10f)
        {
            Vector3 origin = characterBody.corePosition;
            bool raycastHit = Physics.Raycast(origin, Vector3.down, out _, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);

            // it is reversed, because then the name makes more sense
            return !raycastHit;
        }

    }
}
