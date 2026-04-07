using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Events;

namespace ProjectSynth.Components
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileStickOnImpactByNormal : MonoBehaviour, IProjectileImpactBehavior
    {
        [Header("Stick Rules")]
        [Range(-1f, 1f)]
        public float minGroundNormalY = 0.65f;

        public bool ignoreCharacters = true;
        public bool ignoreWorld = false;

        [Header("Pose")]
        public bool alignNormals = true;
        public bool invertNormal = false;

        [Header("FX")]
        public string stickSoundString = "";
        public ParticleSystem[] stickParticleSystem;
        public UnityEvent stickEvent;

        public Transform stuckTransform { get; private set; }
        public CharacterBody stuckBody { get; private set; }
        public bool stuck { get; private set; }

        private Rigidbody rb;
        private ProjectileController pc;

        // if we stick to a moving transform, we keep local pose
        private Vector3 stuckLocalPos;
        private Quaternion stuckLocalRot;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            pc = GetComponent<ProjectileController>();
        }

        private void OnDisable()
        {
            // reset for pooling safety
            stuck = false;
            stuckTransform = null;
            stuckBody = null;
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (!enabled || stuck) return;

            var col = impactInfo.collider;
            if (!col) return;

            var n = impactInfo.estimatedImpactNormal;
            if (!PassesNormalGate(n)) return;

            // --- hurtbox / character ---
            var hb = col.GetComponent<HurtBox>();
            if (hb)
            {
                if (ignoreCharacters) return;

                var hc = hb.healthComponent;
                if (!hc) return;

                // don't stick to owner
                if (pc && pc.owner && hc.gameObject == pc.owner) return;

                StickToTransform(hb.transform, n, hc.body);
                return;
            }

            // --- world ---
            if (ignoreWorld) return;

            StickToTransform(col.transform, n, null);
        }

        private bool PassesNormalGate(Vector3 n)
        {
            if (n == Vector3.zero) return false;
            return n.normalized.y >= minGroundNormalY;
        }

        private void StickToTransform(Transform target, Vector3 impactNormal, CharacterBody body)
        {
            if (!target) return;

            // rotate to align with surface normal (optional)
            if (alignNormals && impactNormal != Vector3.zero)
            {
                Vector3 up = invertNormal ? -impactNormal : impactNormal;
                up.Normalize();

                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, up);
                if (forward.sqrMagnitude < 0.0001f)
                    forward = Vector3.ProjectOnPlane(Vector3.forward, up);
                if (forward.sqrMagnitude < 0.0001f)
                    forward = Vector3.ProjectOnPlane(Vector3.right, up);

                forward.Normalize();
                transform.rotation = Quaternion.LookRotation(forward, up);
            }

            stuck = true;
            stuckTransform = target;
            stuckBody = body;

            // store pose relative to target so we can follow moving targets
            stuckLocalPos = target.InverseTransformPoint(transform.position);
            stuckLocalRot = Quaternion.Inverse(target.rotation) * transform.rotation;

            // freeze physics
            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.detectCollisions = false;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rb.isKinematic = true;
            }

            // local FX/event
            FireStickFx_Local();
        }

        private void FixedUpdate()
        {
            if (!stuck || !stuckTransform) return;

            // follow target
            transform.SetPositionAndRotation(
                stuckTransform.TransformPoint(stuckLocalPos),
                alignNormals ? (stuckTransform.rotation * stuckLocalRot) : transform.rotation
            );
        }

        private void FireStickFx_Local()
        {
            if (stickParticleSystem != null)
            {
                for (int i = 0; i < stickParticleSystem.Length; i++)
                    if (stickParticleSystem[i]) stickParticleSystem[i].Play();
            }

            if (!string.IsNullOrEmpty(stickSoundString))
                Util.PlaySound(stickSoundString, gameObject);

            stickEvent?.Invoke();
        }
    }
}
