using BepInEx;
using ProjectSynth.Modules;
using ProjectSynth.Survivors.Synth;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ProjectSynth
{
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]

    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class SynthPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.TeamSynth.ProjectSynth";
        public const string MODNAME = "ProjectSynth";
        public const string MODVERSION = "0.0.0";

        public const string DEVELOPER_PREFIX = "TEAMSYNTH";

        public static SynthPlugin instance;

        void Awake()
        {
            instance = this;

            //easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            new SynthSurvivor().Initialize();

            // add hooks
            new SynthHooks().Initialize();
        }

    }
}
