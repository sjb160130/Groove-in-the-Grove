using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    [System.Serializable]
    public class FadeTrack
    {
        public AudioSource audioSource;
        public AnimationCurve fadeCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,1));
    }

    public class RhythmAudioMixer : MonoBehaviour
    {
        public List<FadeTrack> tracks = new List<FadeTrack>();

        [SerializeField]
        private RhythmTracker _rhythmTracker;

        void Update()
        {
            for(int i = 0; i < tracks.Count; i++)
            {
                tracks[i].audioSource.volume = Mathf.Lerp(tracks[i].audioSource.volume, tracks[i].fadeCurve.Evaluate(_rhythmTracker.power), Time.deltaTime * 2f);
            }
        }

        public void Play()
        {
            for(int i = 0; i < tracks.Count; i++)
            {
                tracks[i].audioSource.Play();
            }
        }
    }
}
