using EntityStates;
using ProjectSynth.Character.Synth.Content;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectSynth.Character.Synth.States.Hologram
{
    public class BaseDivaArmingState : BaseState
    {
        public GameObject Owner {  get; private set; }
        public GameObject ExplosionPrefab { get; private set; }
        public float ShockFieldRadius { get; set; }
        public string PathToChildToEnable { get; set; }

        private Transform enabledChild;

        public override void OnEnter()
        {
            base.OnEnter();
            Owner = GetComponent<ProjectileController>().owner;
            ExplosionPrefab = SynthAssets.vfx_divaExplosion;

            if (!string.IsNullOrEmpty(PathToChildToEnable))
            {
                enabledChild = transform.Find(PathToChildToEnable);
                enabledChild?.gameObject.SetActive(true);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            enabledChild?.gameObject.SetActive(false);
        }
    }
}
