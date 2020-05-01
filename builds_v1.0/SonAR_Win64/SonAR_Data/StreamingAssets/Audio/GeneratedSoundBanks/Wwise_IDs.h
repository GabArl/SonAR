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
        static const AkUniqueID PLAY_CHORD_FULL = 2576020748U;
        static const AkUniqueID PLAY_CHORD_LOOP = 2096876433U;
        static const AkUniqueID PLAY_LASTCHORD = 3096279812U;
        static const AkUniqueID PLAY_METRO_STEP = 3880256402U;
        static const AkUniqueID PLAY_TICK = 3592141553U;
        static const AkUniqueID PLAY_TICK_GROUP = 1709201661U;
        static const AkUniqueID PLAY_TONE_IDLE = 4028792483U;
        static const AkUniqueID STOP_CHORD_LOOP = 3393091015U;
        static const AkUniqueID STOP_LASTCHORD = 987771470U;
        static const AkUniqueID STOP_METRO_STEP = 3618416136U;
        static const AkUniqueID STOP_TONE_IDLE = 553704049U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace CHORD_MODE
        {
            static const AkUniqueID GROUP = 2788513129U;

            namespace STATE
            {
                static const AkUniqueID MATH = 2708913211U;
                static const AkUniqueID MUSIC = 3991942870U;
            } // namespace STATE
        } // namespace CHORD_MODE

        namespace MUTE_CHORDS
        {
            static const AkUniqueID GROUP = 1372853048U;

            namespace STATE
            {
                static const AkUniqueID MUTE = 2974103762U;
            } // namespace STATE
        } // namespace MUTE_CHORDS

        namespace MUTE_TICKS
        {
            static const AkUniqueID GROUP = 175455601U;

            namespace STATE
            {
                static const AkUniqueID MUTE = 2974103762U;
            } // namespace STATE
        } // namespace MUTE_TICKS

    } // namespace STATES

    namespace SWITCHES
    {
        namespace CHORD_DESIGN
        {
            static const AkUniqueID GROUP = 4082000090U;

            namespace SWITCH
            {
                static const AkUniqueID ONE = 1064933119U;
                static const AkUniqueID THREE = 912956111U;
                static const AkUniqueID TWO = 678209053U;
            } // namespace SWITCH
        } // namespace CHORD_DESIGN

        namespace TICK_BINARY_ONE
        {
            static const AkUniqueID GROUP = 1312546987U;

            namespace SWITCH
            {
                static const AkUniqueID _0 = 846646256U;
                static const AkUniqueID _1 = 846646257U;
            } // namespace SWITCH
        } // namespace TICK_BINARY_ONE

        namespace TICK_BINARY_TWO
        {
            static const AkUniqueID GROUP = 923263321U;

            namespace SWITCH
            {
                static const AkUniqueID _0 = 846646256U;
                static const AkUniqueID _1 = 846646257U;
            } // namespace SWITCH
        } // namespace TICK_BINARY_TWO

        namespace TICK_DESIGN
        {
            static const AkUniqueID GROUP = 654608395U;

            namespace SWITCH
            {
                static const AkUniqueID ONE = 1064933119U;
                static const AkUniqueID TWO = 678209053U;
            } // namespace SWITCH
        } // namespace TICK_DESIGN

        namespace TICK_DIGIT
        {
            static const AkUniqueID GROUP = 365065828U;

            namespace SWITCH
            {
                static const AkUniqueID _0 = 846646256U;
                static const AkUniqueID _1 = 846646257U;
                static const AkUniqueID _2 = 846646258U;
                static const AkUniqueID _3 = 846646259U;
            } // namespace SWITCH
        } // namespace TICK_DIGIT

        namespace TICK_MODE
        {
            static const AkUniqueID GROUP = 1124709220U;

            namespace SWITCH
            {
                static const AkUniqueID BINARY = 3227068230U;
                static const AkUniqueID PITCH = 1908158473U;
                static const AkUniqueID REPEAT = 2639924424U;
            } // namespace SWITCH
        } // namespace TICK_MODE

    } // namespace SWITCHES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID CHORD_LENGTH = 1024289516U;
        static const AkUniqueID HOURS = 3737256986U;
        static const AkUniqueID LENGTH_TO_LAST = 3775536674U;
        static const AkUniqueID LENGTH_TO_ORIGIN = 277558524U;
        static const AkUniqueID MINUTES = 1053147292U;
        static const AkUniqueID RMS = 1114824701U;
        static const AkUniqueID RTPC_POS_X = 3414181498U;
        static const AkUniqueID RTPC_POS_Y = 3414181499U;
        static const AkUniqueID RTPC_POS_Z = 3414181496U;
        static const AkUniqueID RTPC_TICK_REPEAT_TRIGGERRATE = 3441649109U;
        static const AkUniqueID RTPC_VEL_X = 4196732797U;
        static const AkUniqueID RTPC_VEL_Y = 4196732796U;
        static const AkUniqueID RTPC_VEL_Z = 4196732799U;
        static const AkUniqueID SECONDS = 745018932U;
        static const AkUniqueID SEMITONE = 3102311527U;
        static const AkUniqueID TICK_DIGIT = 365065828U;
        static const AkUniqueID TICKSEMI = 4091128056U;
    } // namespace GAME_PARAMETERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID SB_SONAR = 5723758U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID MASTER_AUDIO_BUS = 3803692087U;
        static const AkUniqueID MONO = 3145425408U;
        static const AkUniqueID SPACE = 4164838345U;
    } // namespace BUSSES

    namespace AUX_BUSSES
    {
        static const AkUniqueID REVERB = 348963605U;
    } // namespace AUX_BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
