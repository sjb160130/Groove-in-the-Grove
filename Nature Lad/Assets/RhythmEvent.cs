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
        public UnityEvent mOnEnableInput = new UnityEvent();
        public UnityEvent mOnDisableInput = new UnityEvent();

        private bool _isActive = true;

        void Start()
        {
            if(_rhythmTracker)
            {
                _rhythmTracker.mOnBeat.AddListener(OnBeat);
                _rhythmTracker.mOnHit.AddListener(OnHit);
                _rhythmTracker.mOnSuccessHit.AddListener(OnSuccessHit);
                _rhythmTracker.mOnEnableInput.AddListener(OnEnableInput);
                _rhythmTracker.mOnDisableInput.AddListener(OnEnableInput);
            }
        }

        void OnBeat()
        {
            if (!_isActive)
            {
                return;
            }

            mOnBeat.Invoke();
        }

        void OnHit()
        {
            if(!_isActive)
            {
                return;
            }

            mOnHit.Invoke();
        }

        void OnSuccessHit()
        {
            if (!_isActive)
            {
                return;
            }

            _rhythmTracker.mOnSuccessHit.Invoke();
        }

        public void SetActive(bool val)
        {
            _isActive = val;
        }

        private void OnEnableInput()
        {
            mOnEnableInput.Invoke();
        }

        private void OnDisableInput()
        {
            mOnDisableInput.Invoke();
        }
    }
}
