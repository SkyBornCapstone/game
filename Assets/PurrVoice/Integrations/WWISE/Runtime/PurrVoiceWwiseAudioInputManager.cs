using System.Collections.Generic;
using UnityEngine;

namespace PurrNet.Voice.WwiseIntegration
{
    static class PurrVoiceWwiseAudioInputManager
    {
        public delegate bool AudioSamplesDelegate(uint playingId, uint channelIndex, float[] samples);
        public delegate void AudioFormatDelegate(uint playingId, AkAudioFormat format);

        private static readonly Dictionary<uint, AudioSamplesDelegate> SamplesDelegates = new();
        private static readonly object Sync = new();

        private static readonly AkAudioInputManager.AudioSamplesInteropDelegate SamplesDelegate = InternalAudioSamplesDelegate;
        private static readonly AkAudioInputManager.AudioFormatInteropDelegate FormatDelegate = InternalAudioFormatDelegate;

        private static AudioSamplesDelegate _pendingSamplesDelegate;

        public static uint Post(uint eventId, GameObject gameObject, AudioSamplesDelegate samplesDelegate,
            AudioFormatDelegate formatDelegate)
        {
            RegisterCallbacks();

            lock (Sync)
            {
                _pendingSamplesDelegate = samplesDelegate;
            }

            uint playingId = AkUnitySoundEngine.PostEvent(eventId, gameObject,
                (uint)AkCallbackType.AK_EndOfEvent, EventCallback, null);

            lock (Sync)
            {
                _pendingSamplesDelegate = null;

                if (playingId != AkUnitySoundEngine.AK_INVALID_PLAYING_ID && samplesDelegate != null)
                    SamplesDelegates[playingId] = samplesDelegate;
            }

            return playingId;
        }

        public static void Remove(uint playingId)
        {
            if (playingId == AkUnitySoundEngine.AK_INVALID_PLAYING_ID)
                return;

            lock (Sync)
            {
                SamplesDelegates.Remove(playingId);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            lock (Sync)
            {
                SamplesDelegates.Clear();
                _pendingSamplesDelegate = null;
            }
        }

        public static void RegisterCallbacks()
        {
            AkUnitySoundEngine.SetAudioInputCallbacks(SamplesDelegate, FormatDelegate);
        }

        private static bool InternalAudioSamplesDelegate(uint playingId, float[] samples, uint channelIndex, uint frames)
        {
            AudioSamplesDelegate callback;
            lock (Sync)
            {
                if (!SamplesDelegates.TryGetValue(playingId, out callback))
                    callback = _pendingSamplesDelegate;
            }

            if (callback != null)
                return callback(playingId, channelIndex, samples);

            System.Array.Clear(samples, 0, samples.Length);
            return true;
        }

        private static void InternalAudioFormatDelegate(uint playingId, System.IntPtr format)
        {
        }

        private static void EventCallback(object cookie, AkCallbackType type, AkCallbackInfo callbackInfo)
        {
            if (type != AkCallbackType.AK_EndOfEvent || callbackInfo is not AkEventCallbackInfo info)
                return;

            lock (Sync)
            {
                SamplesDelegates.Remove(info.playingID);
            }
        }
    }
}
