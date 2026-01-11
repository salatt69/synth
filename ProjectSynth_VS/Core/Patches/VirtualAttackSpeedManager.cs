using RoR2;
using System.Collections.Generic;

public static class VirtualAttackSpeedManager
{
    public static readonly Dictionary<CharacterBody, float> virtualAttackSpeed = new();

    public static float Get(CharacterBody body)
    {
        return virtualAttackSpeed.TryGetValue(body, out var val) ? val : body.baseAttackSpeed;
    }

    public static void Set(CharacterBody body, float value)
    {
        virtualAttackSpeed[body] = value;
    }

    public static void Remove(CharacterBody body)
    {
        virtualAttackSpeed.Remove(body);
    }
}
