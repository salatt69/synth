using ProjectSynth.Core;
using UnityEngine;

namespace ProjectSynth.Modules
{
    internal static class Math
    {
        public static readonly float centsInOneOctave = 1200.0f;
        public static readonly float wwiseUnitsPerOctave = 25.0f;
        public static readonly float wwiseCenterValue = 50f;

        public static readonly float downLimit = 0.25f;
        public static readonly float upLimit = 3.0f;

        public static float CalculateSpeedToPitch(float animSpeedMult)
        {
            if (animSpeedMult < downLimit || animSpeedMult > upLimit)
            {
                Log.Error($"CalculateSpeedToPitch: argument passed was out of \'{downLimit} - {upLimit}\' bonds.");
                return wwiseCenterValue;
            }

            float logOfAnimSpeedMult = Mathf.Log(animSpeedMult, 2);

            float newTempoPitch = centsInOneOctave * logOfAnimSpeedMult;

            if (newTempoPitch != 0) Log.Info($"Music transposed to: {newTempoPitch} cents.");

            float finalWwiseUnits = wwiseCenterValue + wwiseUnitsPerOctave * logOfAnimSpeedMult;

            return finalWwiseUnits;
        }
    }
}
