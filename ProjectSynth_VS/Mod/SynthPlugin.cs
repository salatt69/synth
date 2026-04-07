using BepInEx;
using HarmonyLib;
using ProjectSynth.Character.Synth.Content;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using SyncLib.API;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using static ProjectSynth.Components.DivaTracker;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ProjectSynth.Mod
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency("com.salatt.SyncLib", BepInDependency.DependencyFlags.HardDependency)]

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

            // INetMesages
            RegisterMessages();

            var harmony = new Harmony(MODUID);
            harmony.PatchAll();

            MusicSync.Initialize(true);

            // easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            new Character.Synth.SynthSurvivor().Initialize();

            // add hooks
            new Hooks.SynthHooks().Initialize();
        }

        private void RegisterMessages()
        {
            NetworkingAPI.RegisterMessageType<ConsumeOwnedBeaconMessage>();
        }
    }
}
