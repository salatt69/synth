using UnityEngine;

namespace ProjectSynth.Hologram
{
    public class HologramLifetimeNotify : MonoBehaviour
    {
        public GameObject owner;

        private void OnDestroy()
        {
            if (!owner) return;

            var tracker = owner.GetComponent<ExpoTracker>();
            if (tracker) tracker.ClearHologramIf(gameObject);
        }
    }
}
