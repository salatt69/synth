using ProjectSynth.Character.Synth.Content;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class EncoreManager
{
    private class Sequence
    {
        public CharacterBody victim;
        public CharacterBody attacker;
        public int remaining;
        public float procCoef;
        public float nextTime;
    }

    private static readonly List<Sequence> active = new();

    public static void Start(CharacterBody victim, CharacterBody attacker, float procCoef)
    {
        if (!NetworkServer.active) return;

        active.Add(new Sequence
        {
            victim = victim,
            attacker = attacker,
            remaining = 3,
            procCoef = procCoef,
            nextTime = Run.instance ? Run.instance.fixedTime : UnityEngine.Time.fixedTime
        });
    }

    public static void Process()
    {
        if (!NetworkServer.active) return;

        float now = Run.instance ? Run.instance.fixedTime : UnityEngine.Time.fixedTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var s = active[i];

            if (!s.victim || !s.attacker)
            {
                active.RemoveAt(i);
                continue;
            }

            if (now < s.nextTime) continue;

            FireEncoreExplosion(s.victim.corePosition, s.attacker, s.procCoef);

            s.remaining--;
            s.nextTime = now + 0.75f;

            if (s.remaining <= 0)
            {
                s.victim.ClearTimedBuffs(SynthBuffs.EncoreDebuff);
                active.RemoveAt(i);
            }
        }
    }

    private static void FireEncoreExplosion(Vector3 pos, CharacterBody attacker, float procCoef)
    {
        var blast = new BlastAttack
        {
            attacker = attacker.gameObject,
            inflictor = attacker.gameObject,
            teamIndex = attacker.teamComponent.teamIndex,
            position = pos,
            radius = 8f,
            baseDamage = attacker.damage * 1.5f,
            baseForce = 800f,
            crit = Util.CheckRoll(attacker.crit, attacker.master),
            procCoefficient = procCoef * 0.3f,
            damageType = DamageType.Generic
        };

        blast.Fire();
    }
}
