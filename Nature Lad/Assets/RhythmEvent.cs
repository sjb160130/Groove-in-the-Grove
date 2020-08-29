using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NatureLad
{
    public class RhythmEvent : MonoBehaviour
    {
        [SerializeField]
        private RhythmTracker _rhythmTracker;

        public UnityEvent mOnBeat = new UnityEvent();
        public UnityEvent mOnHit = new UnityEvent();
        public UnityEvent mOnSuccessHit = new UnityEvent();

        void Start()
        {
            if(_rhythmTracker)
            {
                _rhythmTracker.mOnBeat.AddListener(OnBeat);
                _rhythmTracker.mOnHit.AddListener(OnHit);
                _rhythmTracker.mOnSuccessHit.AddListener(OnSuccessHit);
            }
        }

        void OnBeat()
        {
            mOnBeat.Invoke();
        }

        void OnHit()
        {
            mOnHit.Invoke();
        }

        void OnSuccessHit()
        {
            _rhythmTracker.mOnSuccessHit.Invoke();
        }
    }
}
