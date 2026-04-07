using EntityStates;
using ProjectSynth.Character.Synth.Content;
using ProjectSynth.Mod;
using RoR2;
using SyncLib.API;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class EncoreRuntime
{
    private static readonly GameObject encoreExplosionPrefab = SynthAssets.vfx_encoreExplosion;

    private class Sequence
    {
        public CharacterBody victim;
        public CharacterBody attacker;
    }

    private static readonly List<Sequence> active = [];

    public static void TryStartSequence(CharacterBody victim, CharacterBody attacker)
    {
        if (!NetworkServer.active) return;

        bool exists = active.Any(s => s.victim == victim && s.attacker == attacker);
        if (exists) return;

        active.Add(new Sequence
        {
            victim = victim,
            attacker = attacker,
        });
    }

    public static void Process()
    {
        if (!NetworkServer.active) return;
        if (active.Count == 0) return;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var s = active[i];

            if (!s.victim || !s.attacker)
            {
                active.RemoveAt(i);
                continue;
            }

            if (MusicSync.OnBeat())
            {
                Fire(s.victim.corePosition, s.attacker);

                int buffCount = s.victim.GetBuffCount(SynthBuffs.Encore.buffIndex);
                s.victim.SetBuffCount(SynthBuffs.Encore.buffIndex, buffCount - 1);
                int newBuffCount = s.victim.GetBuffCount(SynthBuffs.Encore.buffIndex);

                if (newBuffCount <= 0)
                {
                    active.RemoveAt(i);
                }
            } 
        }
    }

    private static void Fire(Vector3 pos, CharacterBody attacker)
    {
        float explosionRadius = 5f;
        new BlastAttack
        {
            radius = explosionRadius,
            baseDamage = attacker.damage * 0.5f,
            damageType = DamageType.AOE,
            falloffModel = BlastAttack.FalloffModel.None,
            attacker = attacker.gameObject,
            inflictor = attacker.gameObject,
            teamIndex = attacker.teamComponent.teamIndex,
            position = pos,
            crit = attacker.RollCrit()
        }.Fire();
        if (encoreExplosionPrefab)
        {
            EffectManager.SpawnEffect(encoreExplosionPrefab, new EffectData
            {
                origin = pos,
                scale = explosionRadius
            }, false);
        }
    }
}
