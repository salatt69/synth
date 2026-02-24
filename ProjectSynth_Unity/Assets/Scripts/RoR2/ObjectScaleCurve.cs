using UnityEngine;

namespace RoR2
{
    public class ObjectScaleCurve : MonoBehaviour
    {
        public bool useOverallCurveOnly;
        public bool resetOnAwake = true;
        public bool resetOnDisable;
        public bool useUnscaledTime;

        public AnimationCurve curveX;
        public AnimationCurve curveY;
        public AnimationCurve curveZ;
        public AnimationCurve overallCurve;

        public float timeMax = 5f;
        public bool ignoreResetOnEnable;
        public bool loop;
    }
}