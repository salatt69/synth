using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Components;
using ProjectSynth.Mod;
using ProjectSynth.Modules.BaseContent.BaseStates.Metro;
using ProjectSynth.States.Synth.Metro;
using Rewired.Utils;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using SyncLib.API;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectSynth.States.Synth.Weapon
{
    public class TNM : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        public GameObject projectilePrefab = SynthAssets.proj_ThirtyNineMusic;
        public GameObject projectilePrefabAlt = SynthAssets.proj_ThirtyNineMusicAlt;
        public GameObject muzzleFlashPrefab = SynthAssets.vfx_tnmMuzzleFlash;
        public float damageCoefficient = SynthStaticValues.thirtyNineMusicDamageCoefficient;
        public double duration = MusicSync.BeatInterval;
        public float force = 20f;
        public float bloom = 5f;
        public float recoilAmplitude = 1.2f;
        public string attackSoundString;
        public string attackSoundStringAlt;

        private SoundWave soundWave;
        private bool hasFired;
        private Animator animator;
        private ChildLocator childLocator;
        private Transform muzzleTransform;
        private string muzzleString;

        private Vector3 originalScale;
        private Vector3 originalScaleAlt;

        public enum SoundWave
        {
            Left,
            Right,
            Center,
            Circle
        }

        public void SetStep(int i)
        {
            soundWave = (SoundWave)i;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            float scaleFactor = attackSpeedStat * 0.75f;

            var scale = projectilePrefab.GetComponent<ScaleObjectOverTime>();
            if (scale.additionalScale != scaleFactor)
            {
                scale.additionalScale = scaleFactor;
            }

            var scaleAlt = projectilePrefabAlt.GetComponent<ScaleObjectOverTime>();
            if (scaleAlt.additionalScale != scaleFactor)
            {
                scaleAlt.additionalScale = scaleFactor;
            }

            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            if (animator)
            {
                childLocator = animator.GetComponent<ChildLocator>();
            }
            switch (soundWave)
            {
                case SoundWave.Left:
                    muzzleString = "HandL";
                    break;
                case SoundWave.Right:
                    muzzleString = "HandR";
                    break;
                case SoundWave.Center:
                    muzzleString = "Tie";
                    break;
                case SoundWave.Circle:
                    muzzleString = "Skirt";
                    break;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            if (hasFired) return;

            characterBody.AddSpreadBloom(bloom);
            Ray aimRay = GetAimRay();
            if (soundWave == SoundWave.Circle)
            {
                Vector3 flat = new Vector3(aimRay.direction.x, 0f, aimRay.direction.z).normalized;
                aimRay = new Ray(aimRay.origin, flat);
            }
            if (childLocator)
            {
                muzzleTransform = childLocator.FindChild(muzzleString);
            }
            if (muzzleFlashPrefab)
            {
                //EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, gameObject, muzzleString, false);
            }
            if (isAuthority)
            {
                float damage = damageStat * damageCoefficient;
                FireProjectileInfo fireProjectileInfo = new()
                {
                    projectilePrefab = projectilePrefab,
                    position = muzzleTransform.position,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    owner = this.gameObject,
                    target = null,
                    useSpeedOverride = false,
                    useFuseOverride = false,
                    damage = damage,
                    force = force,
                    crit = RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    damageTypeOverride = DamageSource.Primary
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            AddRecoil(-0.1f * recoilAmplitude, 0.1f * recoilAmplitude, -1.0f * recoilAmplitude, 1.0f * recoilAmplitude);
        }

        public override void Update()
        {
            base.Update();
            if (MusicSync.OnBeat())
            {
                if (soundWave == SoundWave.Circle && !hasFired)
                {
                    //Util.PlaySound()
                    projectilePrefab = projectilePrefabAlt;
                    damageCoefficient *= 3f;
                    Fire();
                    hasFired = true;
                }
                else if (!hasFired)
                {
                    //Util.PlaySound()
                    Fire();
                    hasFired = true;
                }
            }

            if (isAuthority && hasFired)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)soundWave);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            soundWave = (SoundWave)reader.ReadByte();
        }
    }
}
