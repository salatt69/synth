using ProjectSynth.Character.Synth.Content;
using R2API.Networking;
using R2API.Networking.Interfaces;
using Rewired.Utils;
using RoR2;
using RoR2.Networking;
using RoR2.Skills;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProjectSynth.Components
{
    public class DivaTracker : MonoBehaviour
    {
        public struct ConsumeOwnedBeaconMessage : INetMessage
        {
            public NetworkInstanceId bodyNetId;

            public readonly void Serialize(NetworkWriter writer) => writer.Write(bodyNetId);
            public void Deserialize(NetworkReader reader) => bodyNetId = reader.ReadNetworkId();

            public readonly void OnReceived()
            {
                if (!NetworkServer.active) return;

                GameObject bodyObj = Util.FindNetworkObject(bodyNetId);
                if (!bodyObj) return;

                var tracker = bodyObj.GetComponent<DivaTracker>();
                if (!tracker) return;

                tracker.ConsumeOwnedBeaconServer();
            }
        }

        public float maxTeleportDistance = 75f;
        public LayerMask losMask;
        public SkillDef blinkSkillDef;
        public GenericSkill overrideSlot;
        public GameObject indicatorPrefab;
        public GameObject indicatorLookingPrefab;

        private CharacterBody body;
        private SkillLocator skillLocator;

        private readonly List<ProjectileMarker> cachedBeacons = [];
        private ProjectileMarker focusedBeacon;
        private readonly List<Indicator> defaultIndicators = [];
        private Indicator focusedIndicator;

        private void Awake()
        {
            body = GetComponent<CharacterBody>();
            skillLocator = GetComponent<SkillLocator>();

            Bootstrap();

            focusedIndicator = new Indicator(gameObject, indicatorLookingPrefab);
        }

        private void OnDestroy()
        {
            focusedIndicator?.DestroyVisualizer();
            foreach (var ind in defaultIndicators)
                ind?.DestroyVisualizer();
        }

        private void Update()
        {
            if (losMask.value == 0 || !blinkSkillDef || !indicatorPrefab || !indicatorLookingPrefab || !overrideSlot)
            {
                Bootstrap();
            }

            if (!body || !body.hasAuthority) return;

            FindOwnedBeacons();

            bool hasBeacons = cachedBeacons.Count > 0;
            bool lookingAtBeacon = focusedBeacon != null && GetDot(focusedBeacon.transform.position) > 0.9f;
            bool inRange = focusedBeacon != null && Vector3.Distance(body.corePosition, focusedBeacon.transform.position) <= maxTeleportDistance;

            UpdateIndicators();

            if (hasBeacons && lookingAtBeacon && inRange)
                EnsureOverrideOn();
            else
                MaybeUnsetOverride();
        }

        private void Bootstrap()
        {
            if (losMask.value == 0) losMask = LayerIndex.world.mask;

            if (!blinkSkillDef)
                blinkSkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("Virtual Deviation Teleport"));

            if (!overrideSlot && skillLocator) overrideSlot = skillLocator.secondary;

            if (!indicatorPrefab)
                indicatorPrefab = SynthAssets.divaIndicator;

            if (!indicatorLookingPrefab)
                indicatorLookingPrefab = SynthAssets.divaIndicatorFocused;
        }

        private void FindOwnedBeacons()
        {
            cachedBeacons.Clear();
            focusedBeacon = null;

            var list = InstanceTracker.GetInstancesList<ProjectileMarker>();
            if (list == null || list.Count == 0) return;

            ProjectileMarker bestLooking = null;
            float bestDot = 0f;

            for (int i = 0; i < list.Count; i++)
            {
                var m = list[i];
                if (!m) continue;

                var owner = m.GetOwner();
                if (!owner) continue;

                if (owner != gameObject) continue;

                cachedBeacons.Add(m);

                float dot = GetDot(m.transform.position);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestLooking = m;
                }
            }

            focusedBeacon = bestLooking;
        }

        private float GetDot(Vector3 targetPos)
        {
            if (!body) return 0f;

            Vector3 direction = targetPos - body.inputBank.aimOrigin;
            direction.y = 0;
            if (direction.magnitude < 0.001f) return 0f;

            Vector3 forward = body.inputBank.aimDirection;
            forward.y = 0;
            if (forward.magnitude < 0.001f) return 0f;

            return Vector3.Dot(direction.normalized, forward.normalized);
        }

        private void EnsureOverrideOn()
        {
            if (!overrideSlot || !blinkSkillDef) return;
            overrideSlot.SetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void MaybeUnsetOverride()
        {
            if (!overrideSlot || !blinkSkillDef) return;
            overrideSlot.UnsetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void ClearClientUX()
        {
            cachedBeacons.Clear();
            focusedBeacon = null;

            foreach (var ind in defaultIndicators)
            {
                ind.targetTransform = null;
                ind.active = false;
                ind.UpdateVisualizer();
            }

            if (focusedIndicator != null)
            {
                focusedIndicator.targetTransform = null;
                focusedIndicator.active = false;
                focusedIndicator.UpdateVisualizer();
            }

            // local override cleanup
            if (overrideSlot && blinkSkillDef)
                overrideSlot.UnsetSkillOverride(this, blinkSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        private void ConsumeOwnedBeaconServer()
        {
            if (!NetworkServer.active) return;

            foreach (var beacon in cachedBeacons)
            {
                if (beacon)
                {
                    NetworkServer.Destroy(beacon.gameObject);
                }
            }
        }

        #region indicator
        private void UpdateIndicators()
        {
            while (defaultIndicators.Count < cachedBeacons.Count)
            {
                var ind = new Indicator(gameObject, indicatorPrefab);
                defaultIndicators.Add(ind);
            }

            while (defaultIndicators.Count > cachedBeacons.Count)
            {
                var ind = defaultIndicators[defaultIndicators.Count - 1];
                defaultIndicators.RemoveAt(defaultIndicators.Count - 1);
                ind?.DestroyVisualizer();
            }

            for (int i = 0; i < cachedBeacons.Count; i++)
            {
                var beacon = cachedBeacons[i];
                var ind = defaultIndicators[i];

                if (!beacon || ind.IsNullOrDestroyed())
                {
                    ind.targetTransform = null;
                    ind.active = false;
                    ind.UpdateVisualizer();
                    continue;
                }

                ind.targetTransform = beacon.transform;
                ind.active = true;
                ind.UpdateVisualizer();

                Color c = GetBeaconColor(beacon.transform.position);
                SetIndicatorColor(ind, c);
            }

            if (focusedIndicator != null)
            {
                bool inRange = focusedBeacon != null && Vector3.Distance(body.corePosition, focusedBeacon.transform.position) <= maxTeleportDistance;
                bool lookingAtBeacon = focusedBeacon != null && GetDot(focusedBeacon.transform.position) > 0.9f;

                if (inRange && lookingAtBeacon)
                {
                    focusedIndicator.targetTransform = focusedBeacon.transform;
                    focusedIndicator.active = true;
                    focusedIndicator.UpdateVisualizer();

                    Color c = GetBeaconColor(focusedBeacon.transform.position);
                    SetIndicatorColor(focusedIndicator, c);
                }
                else
                {
                    focusedIndicator.targetTransform = null;
                    focusedIndicator.active = false;
                    focusedIndicator.UpdateVisualizer();
                }
            }
        }

        private Color GetBeaconColor(Vector3 targetPos)
        {
            if (!body) return Color.gray;

            float dist = Vector3.Distance(body.corePosition, targetPos);
            if (dist > maxTeleportDistance) return Color.gray;

            Vector3 from = body.corePosition;
            Vector3 dir = targetPos - from;
            float len = dir.magnitude;
            if (len > 0.001f)
            {
                dir /= len;

                if (Physics.Raycast(from, dir, out RaycastHit hit, dist, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    bool allowed = hit.distance >= dist - 0.20f;
                    if (!allowed) return Color.red;
                    return Color.green;
                }
            }

            return Color.green;
        }

        private void SetIndicatorColor(Indicator indicator, Color c)
        {
            if (indicator?.visualizerInstance)
            {
                var sr = indicator.visualizerInstance.GetComponentInChildren<SpriteRenderer>();
                if (sr)
                {
                    if (sr.material != null && !sr.material.name.EndsWith("(Instance)"))
                    {
                        sr.material = UnityEngine.Object.Instantiate(sr.material);
                    }
                    sr.color = c;
                }

                var mr = indicator.visualizerInstance.GetComponentInChildren<MeshRenderer>();
                if (mr)
                {
                    if (mr.material != null && !mr.material.name.EndsWith("(Instance)"))
                    {
                        mr.material = UnityEngine.Object.Instantiate(mr.material);
                    }
                    if (mr.material != null)
                    {
                        mr.material.color = c;
                    }
                }
            }
        }
        #endregion

        #region api
        //public void ConsumeCurrentTarget()
        //{
        //    if (body && body.hasAuthority)
        //    {
        //        ClearClientUX();
        //    }

        //    if (NetworkServer.active)
        //    {
        //        ConsumeOwnedBeaconServer();
        //        return;
        //    }

        //    if (NetworkClient.active && body && body.hasAuthority)
        //    {
        //        var ni = body.GetComponent<NetworkIdentity>();
        //        if (!ni) return;

        //        new ConsumeOwnedBeaconMessage { bodyNetId = ni.netId }.Send(NetworkDestination.Server);
        //    }
        //}

        public bool CanTeleportTo(Vector3 pos, out bool blocked, out float dist)
        {
            blocked = false;
            dist = 999f;
            if (!body) return false;

            if (!focusedBeacon || GetDot(focusedBeacon.transform.position) <= 0.9f) return false;

            Vector3 from = body.corePosition;
            dist = Vector3.Distance(from, pos);
            if (dist > maxTeleportDistance) return false;

            Vector3 dir = (pos - from);
            float len = dir.magnitude;
            if (len > 0.001f)
            {
                dir /= len;

                if (Physics.Raycast(from, dir, out RaycastHit hit, dist, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    bool allowed = hit.distance >= dist - 0.20f;
                    if (!allowed) blocked = true;
                    return allowed;
                }
            }

            return true;
        }

        public bool TryGetBestTarget(out Transform t)
        {
            if (focusedBeacon)
            {
                t = focusedBeacon.GetTransform();
                return true;
            }

            t = null;
            return false;
        }
        #endregion
    }
}
