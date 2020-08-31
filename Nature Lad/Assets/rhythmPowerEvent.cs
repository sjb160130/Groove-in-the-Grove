using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NatureLad
{
    public class rhythmPowerEvent : MonoBehaviour
    {
        public UnityEvent mOnPowerLevelReached = new UnityEvent();

        [SerializeField]
        private RhythmTracker _rhythmTracker;
        [SerializeField]
        private float powerForEvent;
        [SerializeField]
        private bool doOnce = true;

        private bool _hasFired = false;

        // Update is called once per frame
        void Update()
        {
            if(!_rhythmTracker || (_hasFired && doOnce))
            {
                return;
            }
            
            if(_rhythmTracker.power >= powerForEvent && !_hasFired)
            {
                mOnPowerLevelReached.Invoke();
                _hasFired = true;
            }

            if(_rhythmTracker.power < powerForEvent && _hasFired)
            {
                _hasFired = false;
            }            
        }
    }
}
