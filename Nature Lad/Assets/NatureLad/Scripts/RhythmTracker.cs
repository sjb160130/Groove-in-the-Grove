﻿using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
//using UnityEditor;
//using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NatureLad
{
    [System.Serializable]
    public class IntEvent : UnityEvent<int>
    {
    }

    [System.Serializable]
    public class Beat
    {
        public Transform transform;

        public int idx;
        public bool isHit;

        public Beat(int i)
        {
            idx = i;
        }

        public Beat(Transform r, int i)
        {
            transform = r;
            idx = i;
        }
    }

    public class RhythmTracker : SerializedMonoBehaviour
    {
        /*        [TableMatrix(DrawElementMethod = "DrawCell")]
                public bool[,] sequence = new bool[64, 2];*/

        [ListDrawerSettings(NumberOfItemsPerPage = 16, ShowIndexLabels = true)]
        public bool[] pressSequence = new bool[64];

        public UnityEvent mOnMaxPowerHit = new UnityEvent();
        public UnityEvent mOnPowerDrained = new UnityEvent();
        public UnityEvent mOnHit = new UnityEvent();
        public IntEvent mOnHitIdx = new IntEvent();
        public UnityEvent mOnBeat = new UnityEvent();
        public UnityEvent mOnNewBeat = new UnityEvent();
        public UnityEvent mOnUpdateBeat = new UnityEvent();
        public UnityEvent mOnInit = new UnityEvent();

        public float power = 0f;
        [SerializeField]
        private float attenuation = .1f;
        [SerializeField]
        private float powerIncrease = .1f;

        [SerializeField]
        private bool ignoreProximity = false;
        [SerializeField]
        private float proximityRangeMin = 10f;
        [SerializeField]
        private float proximityRangeMax = 20f;

        public float proximityPower = 0f;

        private AudioSource _audioSource;

        private float _length = 0f;

        public float beatLength { get { return _beatLength; } }
        private float _beatLength;

/*        [SerializeField]
        private RectTransform lineParent = null;
        [SerializeField]
        private RectTransform visualIcon = null;*/
        //[SerializeField]
        //private float lineWidth = 1500f;
        //[SerializeField]
        //private float lineOffset = 250f;

        [SerializeField]
        private float beatAccuracy = 1f;
        [SerializeField]
        private float beatOffset = 0f;

        [SerializeField]
        private int beatPreview = 16;

        private float _timer = 0f;
        private float _lastHit = 0f;
        public float deltaAudioTime { get { return _deltaAudioTime; } }
        private float _deltaAudioTime = 0f;

        private bool _inHitWindow = false;
        private bool _inReleaseWindow = false;

        
/*        private float _iconMoveSpeed;
        private float _pixelsPerBeat;
        private float _pixelsPerSecond;*/
        private float _accuracyLength;
        public int currentIdx { get { return _idx; } }
        private int _idx = 0;

        [SerializeField]
        private float _aggregatePower = 0f;

        //public List<Beat> activeBeats = new List<Beat>();
        //public List<Beat> inactiveBeats = new List<Beat>();

        private GameObject _player;

        private bool _hasPower;
        [SerializeField]
        private bool _alwaysFollow;
        
        public bool isFollowing { get { return _isFollowing; } }
        private bool _isFollowing;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            if(_audioSource.clip != null)
            {
                PrecalculateData();
            }

            _isFollowing = _alwaysFollow;

            Play();

            if(!_player)
            {
                _player = GameObject.FindGameObjectWithTag("Player");
            }

            mOnInit.Invoke();
        }

        // Update is called once per frame
        void Update()
        {
            // process timers
            if(_audioSource.time < _timer)
            {
                _timer -= _audioSource.clip.length;
            }
            _deltaAudioTime = _audioSource.time - _timer;
            _timer = _audioSource.time;

            // on beat change
            int currentIdx = (int)Mathf.Floor(_timer / _beatLength);
            if (_idx != currentIdx)
            {
                _idx = currentIdx;
                UpdateBeat();
            }

            _inHitWindow = pressSequence[_idx];
            _inReleaseWindow = pressSequence[_idx];

            // Calculate proximity power
            proximityPower = 0f;
            if (_player)
            {
                float distance = (_player.transform.position - transform.position).magnitude;
                proximityPower = 1f - ((Mathf.Clamp(distance, proximityRangeMin, proximityRangeMax) - proximityRangeMin) / (proximityRangeMax - proximityRangeMin));
            }

            // Calculate aggregate power and icon size
            _aggregatePower = Mathf.Max(power, proximityPower);

            // attenuate power
            power = Mathf.Max(power - attenuation * Time.deltaTime, 0.0f);

            // change properties based on power
            // and invoke events
            if (power < .01f)
            {
                if(_hasPower)
                {
                    _hasPower = false;
                    mOnPowerDrained.Invoke();
                }


                if (_isFollowing && !_alwaysFollow)
                {
                    _isFollowing = false;
                }
            }

            // adjust volume based on aggregate power
            _audioSource.volume = Mathf.Lerp(_audioSource.volume, _aggregatePower, Time.deltaTime*2f);
        }

        void PrecalculateData()
        {
            _length = _audioSource.clip.length;
            _beatLength = _length / (float)pressSequence.Length;
            _accuracyLength = _beatLength * beatAccuracy;
            //Debug.Log("Pixels per beat: " + _pixelsPerBeat);
            //_iconMoveSpeed = (lineWidth - lineOffset) / (beatPreview * _beatLength);
        }

        /*        // Note: Doesn't work after the first iteration because of looping
                float GetPositionOnTimelineByIdx(int idx)
                {
                    int dif = (idx - _idx);

                    float wantedPos = (dif * _pixelsPerBeat + lineOffset) - ((_timer % _beatLength) * _pixelsPerSecond); //((_timer - (_idx * _beatLength)) * _pixelsPerBeat) + (dif * _pixelsPerBeat);  //(dif * _pixelsPerBeat) - ((_timer % _beatLength) * _pixelsPerBeat);

                    return wantedPos;
                }*/

        public void Hit()
        {
            /*            float hitTime = _audioSource.time + beatOffset;

                        int idx = (int)Mathf.Floor(hitTime / _beatLength);
                        float beatTime = idx * _beatLength;

                        float delta = beatTime - hitTime;
                        bool inHitWindow = false;
                        if (delta < _beatLength * beatAccuracy)
                        {
                            inHitWindow = true;
                        }*/

            if (Mathf.Approximately(proximityPower, 0f) && !ignoreProximity)
            {
                return;
            }

            float powerMult = ignoreProximity ? 1.0f : proximityPower;
            float wantedTime = _idx * _beatLength;
            float delta = Mathf.Min(_timer - wantedTime, (_length - (_timer - wantedTime))%_length );

            if (delta < _accuracyLength)
            {
                //Image img = activeIcons[i].icon.gameObject.GetComponent<Image>();
                //img.color = Color.green;
/*                Animator anm = activeBeats[i].rectTransform.gameObject.GetComponent<Animator>();
                anm.SetTrigger("hit");
                //activeIcons[i].icon.localScale = Vector3.one * 1.25f;
                activeBeats[i].isHit = true;*/
                _audioSource.volume = 1.0f;
                power = Mathf.Min(power + (powerIncrease * powerMult), 1.0f);
                if (power > .01f)
                {
                    _hasPower = true;
                }

                if (Mathf.Approximately(power, 1.0f))
                {
                    mOnMaxPowerHit.Invoke();
                    _isFollowing = true;
                }

                mOnHit.Invoke();
                mOnHitIdx.Invoke(_idx);
            }

/*            for (int i = 0; i < Mathf.Min(activeBeats.Count, 4); i++)
            {
                if(activeBeats[i].isHit)
                {
                    continue;
                }
                

                float wantedTime = activeBeats[i].idx * _beatLength;
                float delta = Mathf.Abs(wantedTime - _timer);
                //Debug.Log(delta);
                if (delta < _accuracyLength)
                {
                    //Image img = activeIcons[i].icon.gameObject.GetComponent<Image>();
                    //img.color = Color.green;
                    Animator anm = activeBeats[i].rectTransform.gameObject.GetComponent<Animator>();
                    anm.SetTrigger("hit");
                    //activeIcons[i].icon.localScale = Vector3.one * 1.25f;
                    activeBeats[i].isHit = true;
                    _audioSource.volume = 1.0f;
                    power = Mathf.Min(power + (powerIncrease * powerMult), 1.0f);
                    if(power > .01f)
                    {
                        _hasPower = true;
                    }

                    if(Mathf.Approximately(power, 1.0f))
                    {
                        mOnMaxPowerHit.Invoke();
                        _isFollowing = true;
                    }

                    mOnHit.Invoke();

                    break;
                }
            }*/
        }

        public void Play()
        {
            _audioSource.Play();
            _timer = 0f;

            //InvokeRepeating("UpdateBeat", 0f, _beatLength);
            //InvokeRepeating("ResetBeat", (beatAccuracy * _beatLength)-.04f, _beatLength);
        }


        public void Stop()
        {
            CancelInvoke();
        }

        void UpdateBeat()
        {
            if( pressSequence[_idx] )
            {
                mOnBeat.Invoke();
            }

            mOnUpdateBeat.Invoke();
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200, 20), _timer.ToString());
            GUI.Label(new Rect(10, 70, 200, 20), _idx.ToString());
            GUI.Label(new Rect(10, 30, 200, 20), _inHitWindow.ToString());
            GUI.Label(new Rect(10, 50, 200, 20), _inReleaseWindow.ToString());
        }
        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, proximityRangeMin);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, proximityRangeMax);
        }

/*        private static bool DrawCell(Rect rect, bool value)
        {
            if (Event.current.type == EventType.MouseDown &&
                rect.Contains(Event.current.mousePosition))
            {
                value = !value;
                GUI.changed = true;
                Event.current.Use();
            }

            EditorGUI.DrawRect(
                rect.Padding(1),
                value ? new Color(.1f, .8f, .2f)
                    : new Color(0, 0, 0, .5f));

            return value;
        }*/

        public void SetIgnoreProximity(bool v)
        {
            ignoreProximity = v;
        }
    }
}

