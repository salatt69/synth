using BepInEx;
using R2API;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace ProjectSynth.Core
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
            new Character.Synth.SynthSurvivor().Initialize();

            // add hooks
            new Hooks.SynthHooks().Initialize();
        }

    }
}
