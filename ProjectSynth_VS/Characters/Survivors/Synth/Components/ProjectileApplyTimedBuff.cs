using ProjectSynth.Survivors.Synth;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;

namespace ProjectSynth.Characters.Survivors.Synth.Components
{
    // since RoR2's built-in "ProjectileInflictTimedBuff" didn't work for me, i had to made my own.. maybe im just dumb
    // changed "Inflict" to "Apply", used "IProjectileImpactBehavior" istead of "IOnDamageInflictedServerReceiver"
    public class ProjectileApplyTimedBuff : MonoBehaviour, IProjectileImpactBehavior
    {
        public GameObject projectile;

        public bool CanApply(bool canApply) { return canApply; }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (!NetworkServer.active) return;

            if (!MetronomeSequenceManager.GetHitResult()) return;
            
            MetronomeSequenceManager.SetHitResult(false);

            HurtBox hurtBox = impactInfo.collider
                ? impactInfo.collider.GetComponent<HurtBox>()
                : null;

            CharacterBody body = hurtBox?.healthComponent?.body;

            if (!body) return;

            ProjectileController projectileController = projectile.GetComponent<ProjectileController>();

            CharacterBody ownerBody =
                projectileController?.owner
                    ? projectileController.owner.GetComponent<CharacterBody>()
                    : null;

            if (!ownerBody) return;

            // Team check
            if (body.teamComponent &&
                body.teamComponent.teamIndex == ownerBody.teamComponent.teamIndex)
                return;

            // Apply debuff
            body.AddTimedBuff(SynthBuffs.encoreDebuff, 5f);
            EncoreSequenceManager.Start(body, ownerBody, 0f);
        }
    }
}
