using System;
using EntityStates;
using EntityStates.Toolbot;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth
{
    // Token: 0x02000263 RID: 611
    public class RollingGirlImpact : BaseState
    {
        // Token: 0x06000C47 RID: 3143 RVA: 0x0003256C File Offset: 0x0003076C
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                if (this.victimHealthComponent)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = base.gameObject,
                        damage = this.damageStat * 5 * this.damageBoostFromSpeed,
                        crit = this.isCrit,
                        procCoefficient = 1f,
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = new DamageTypeCombo(DamageType.Stun1s, DamageTypeExtended.Generic, DamageSource.Utility),
                        position = base.characterBody.corePosition,
                        inflictedHurtbox = this.victimHurtBox
                    };
                    this.victimHealthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, this.victimHealthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, this.victimHealthComponent.gameObject);
                }
                base.healthComponent.TakeDamageForce(this.idealDirection * -100, true, false);
            }
            if (base.isAuthority)
            {
                //base.AddRecoil(-0.5f * ToolbotDash.recoilAmplitude * 3f, -0.5f * ToolbotDash.recoilAmplitude * 3f, -0.5f * ToolbotDash.recoilAmplitude * 8f, 0.5f * ToolbotDash.recoilAmplitude * 3f);
                //EffectManager.SimpleImpactEffect(ToolbotDash.knockbackEffectPrefab, base.characterBody.corePosition, base.characterDirection.forward, true);
                this.outer.SetNextStateToMain();
            }
        }

        // Token: 0x06000C48 RID: 3144 RVA: 0x000326E8 File Offset: 0x000308E8
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.victimHealthComponent ? this.victimHealthComponent.gameObject : null);
            writer.Write(this.idealDirection);
            writer.Write(this.damageBoostFromSpeed);
            writer.Write(this.isCrit);
        }

        // Token: 0x06000C49 RID: 3145 RVA: 0x00032744 File Offset: 0x00030944
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            GameObject gameObject = reader.ReadGameObject();
            this.victimHealthComponent = (gameObject ? gameObject.GetComponent<HealthComponent>() : null);
            this.idealDirection = reader.ReadVector3();
            this.damageBoostFromSpeed = reader.ReadSingle();
            this.isCrit = reader.ReadBoolean();
        }

        // Token: 0x06000C4A RID: 3146 RVA: 0x000150C6 File Offset: 0x000132C6
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        // Token: 0x04000D4C RID: 3404
        public HealthComponent victimHealthComponent;

        // Token: 0x04000D4D RID: 3405
        public Vector3 idealDirection;

        // Token: 0x04000D4E RID: 3406
        public float damageBoostFromSpeed;

        // Token: 0x04000D4F RID: 3407
        public bool isCrit;

        // Token: 0x04000D50 RID: 3408
        public HurtBox victimHurtBox;
    }
}
