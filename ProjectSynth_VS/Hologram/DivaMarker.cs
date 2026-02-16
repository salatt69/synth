using EntityStates.Engi.Mine;
using ProjectSynth.Core;
using R2API.Utils;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace ProjectSynth.Hologram
{
    public class DivaMarker : MonoBehaviour
    {
        private ProjectileController pc;

        private void Awake()
        {
            pc = GetComponent<ProjectileController>();
        }

        private void OnEnable() => InstanceTracker.Add(this);   

        private void OnDisable() => InstanceTracker.Remove(this);

        public GameObject GetOwner()
        {
            return pc ? pc.owner : null;
        }

        public Transform GetTransform() => transform;
    }
}
