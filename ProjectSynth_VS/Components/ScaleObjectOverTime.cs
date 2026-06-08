using UnityEngine;

namespace ProjectSynth.Components
{
    public class ScaleObjectOverTime : MonoBehaviour
    {
        public Vector3 InitialScale { get; private set; }
        public Vector3 finalScale;
        public float time;
        public float additionalScale;

        private float timeElapsed;

        public void OnEnable()
        {
            InitialScale = transform.localScale;
            timeElapsed = 0f;
        }

        public void FixedUpdate()
        {
            timeElapsed += Time.fixedDeltaTime;

            float t = Mathf.Clamp01(timeElapsed / time);
            float easedT = 1f - Mathf.Pow(1f - t, 3); // Cubic ease-out

            transform.localScale = Vector3.Lerp(InitialScale, finalScale + Vector3.one * additionalScale, easedT);
        }

        public void OnDisable()
        {
            transform.localScale = InitialScale;
            timeElapsed = 0f;
        }
    }
}
