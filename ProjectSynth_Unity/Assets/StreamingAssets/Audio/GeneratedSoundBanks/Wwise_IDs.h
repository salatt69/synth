/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_METRONOMERECHARGE_1 = 3990587749U;
        static const AkUniqueID PLAY_METRONOMERECHARGE_2 = 3990587750U;
        static const AkUniqueID PLAY_METRONOMERECHARGE_3 = 3990587751U;
        static const AkUniqueID PLAY_METRONOMERECHARGE_4 = 3990587744U;
        static const AkUniqueID PLAY_SYNTH_METRONOMESUSTAIN = 2177578824U;
        static const AkUniqueID PLAY_SYNTH_METRONOMETICK = 4019602470U;
        static const AkUniqueID PLAY_SYNTH_THIRTYNINEMUSIC_SHOT = 2888092107U;
        static const AkUniqueID STOP_SYNTH_METRONOMESUSTAIN = 950680430U;
        static const AkUniqueID SYNTHDJ_PAUSE_MUSIC_SYSTEM = 3058367100U;
        static const AkUniqueID SYNTHDJ_PLAY_MUSIC_SYSTEM = 478512564U;
        static const AkUniqueID SYNTHDJ_UNPAUSE_MUSIC_SYSTEM = 1905278449U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace BOSSPHASE
        {
            static const AkUniqueID GROUP = 2561284575U;

            namespace STATE
            {
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID PHASE1 = 3630028971U;
                static const AkUniqueID PHASE2 = 3630028968U;
                static const AkUniqueID PHASE3 = 3630028969U;
                static const AkUniqueID PHASE4 = 3630028974U;
            } // namespace STATE
        } // namespace BOSSPHASE

        namespace BOSSSTATUS
        {
            static const AkUniqueID GROUP = 549431000U;

            namespace STATE
            {
                static const AkUniqueID ALIVE = 655265632U;
                static const AkUniqueID DEAD = 2044049779U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace BOSSSTATUS

        namespace ENEMYTYPE
        {
            static const AkUniqueID GROUP = 3807720919U;

            namespace STATE
            {
                static const AkUniqueID BOSS = 1560169506U;
                static const AkUniqueID ELITE = 1464097230U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID NORMAL = 1160234136U;
            } // namespace STATE
        } // namespace ENEMYTYPE

        namespace GHOSTSTATUS
        {
            static const AkUniqueID GROUP = 3480929964U;

            namespace STATE
            {
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID TRUE = 3053630529U;
            } // namespace STATE
        } // namespace GHOSTSTATUS

        namespace MUSIC_MENU
        {
            static const AkUniqueID GROUP = 1598298728U;

            namespace STATE
            {
                static const AkUniqueID LOGBOOK = 1654435662U;
                static const AkUniqueID MAIN = 3161908922U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID OPENINGCUTSCENE1 = 113661376U;
                static const AkUniqueID OPENINGCUTSCENE2 = 113661379U;
                static const AkUniqueID OUTROCUTSCENEA = 2266819555U;
                static const AkUniqueID OUTROCUTSCENEB = 2266819552U;
                static const AkUniqueID OUTROCUTSCENEFULL = 2121506681U;
                static const AkUniqueID PRISMATIC = 3912041495U;
            } // namespace STATE
        } // namespace MUSIC_MENU

        namespace MUSIC_SYSTEM
        {
            static const AkUniqueID GROUP = 792781730U;

            namespace STATE
            {
                static const AkUniqueID BOSSFIGHT = 580146960U;
                static const AkUniqueID GAMEPLAY = 89505537U;
                static const AkUniqueID MENU = 2607556080U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID SECRETLEVEL = 778026301U;
            } // namespace STATE
        } // namespace MUSIC_SYSTEM

        namespace SYNTHDJ_GAMEPLAYMUSIC
        {
            static const AkUniqueID GROUP = 1714381171U;

            namespace STATE
            {
                static const AkUniqueID DECOMPRESSION_LOOP = 3403129731U;
                static const AkUniqueID MYJAZZSONG = 145640315U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace SYNTHDJ_GAMEPLAYMUSIC

        namespace TABSTATUS
        {
            static const AkUniqueID GROUP = 3028652072U;

            namespace STATE
            {
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID OFF = 930712164U;
                static const AkUniqueID ON = 1651971902U;
            } // namespace STATE
        } // namespace TABSTATUS

        namespace TELEPORTERZONE
        {
            static const AkUniqueID GROUP = 4244680111U;

            namespace STATE
            {
                static const AkUniqueID IN = 1752637612U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID OUT = 645492555U;
            } // namespace STATE
        } // namespace TELEPORTERZONE

    } // namespace STATES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID ESCAPETIMER = 681309163U;
        static const AkUniqueID METER_CHARACTER = 1514947204U;
        static const AkUniqueID METER_SFX = 2969259856U;
        static const AkUniqueID METER_UI = 1871679725U;
        static const AkUniqueID PLAYERHEALTH = 151362964U;
        static const AkUniqueID SYNCMUSICTOTEMPO = 3846151963U;
        static const AkUniqueID TELEPORTERDIRECTION = 4014092120U;
        static const AkUniqueID TELEPORTERPLAYERSTATUS = 1010989040U;
        static const AkUniqueID TELEPORTERPROXIMITY = 1924079656U;
        static const AkUniqueID VOLUME_MASTER = 3695994288U;
        static const AkUniqueID VOLUME_MSX = 3729143042U;
        static const AkUniqueID VOLUME_SFX = 3673881719U;
    } // namespace GAME_PARAMETERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID SYNTHDJ = 393143179U;
        static const AkUniqueID SYNTHSOUNDS = 2800900469U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID HDR = 931844945U;
        static const AkUniqueID MASTER_AUDIO_BUS = 3803692087U;
        static const AkUniqueID MASTER_SECONDARY_BUS = 805203703U;
        static const AkUniqueID MSX_BEACON = 2060036974U;
        static const AkUniqueID MSX_BUS = 2893081552U;
        static const AkUniqueID SFX_BUS = 213475909U;
    } // namespace BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
