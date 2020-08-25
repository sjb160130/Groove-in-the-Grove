using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
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
    public class BeatIcon
    {
        public RectTransform icon;
        public int idx;
        public bool isHit;

        public BeatIcon(RectTransform r, int i)
        {
            icon = r;
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
        public UnityEvent mOnBeat = new UnityEvent();

        [SerializeField]
        private float power = 0f;
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

        private float proximityPower = 0f;

        [SerializeField]
        private AudioClip[] progression;

        private AudioSource _audioSource;

        private float _length = 0f;

        private float _beatLength;

        [SerializeField]
        private RectTransform lineParent = null;
        [SerializeField]
        private RectTransform visualIcon = null;
        [SerializeField]
        private float lineWidth = 1500f;
        [SerializeField]
        private float lineOffset = 250f;

        [SerializeField]
        private float beatAccuracy = 1f;
        [SerializeField]
        private float beatOffset = 0f;

        [SerializeField]
        private int beatPreview = 16;

        private float _timer = 0f;
        private float _lastHit = 0f;
        private float _deltaAudioTime = 0f;

        private bool _inHitWindow = false;
        private bool _inReleaseWindow = false;

        
        private float _iconMoveSpeed;
        private float _pixelsPerBeat;
        private float _pixelsPerSecond;
        private float _accuracyLength;
        private int _idx = 0;

        [SerializeField]
        private bool spawnIconIfMin = false;

        [SerializeField]
        private float minIconSize = .5f;

        [SerializeField]
        private float _wantedIconSize = 0f;

        [SerializeField]
        private float _aggregatePower = 0f;

        [SerializeField]
        public List<BeatIcon> activeIcons = new List<BeatIcon>();
        [SerializeField]
        public List<BeatIcon> inactiveIcons = new List<BeatIcon>();

        private GameObject _player;

        private bool _hasPower;
        [SerializeField]
        private bool _alwaysFollow;
        private bool _isFollowing;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            if(progression[0])
            {
                PrecalculateData();
            }

            _isFollowing = _alwaysFollow;

            Play();

            if(!_player)
            {
                _player = GameObject.FindGameObjectWithTag("Player");
            }
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

            _wantedIconSize = Mathf.Lerp(minIconSize, 1f, power);
            if (!_isFollowing)
            {
                _wantedIconSize = Mathf.Max((proximityPower >= .95f ? minIconSize + .01f : 0f), _wantedIconSize);
            }

            // Move and scale icons
            for ( int i = 0; i < activeIcons.Count; i++)
            {
                Vector3 wantedPosition = activeIcons[i].icon.anchoredPosition3D;
                wantedPosition.x -= _deltaAudioTime * _pixelsPerSecond; //GetPositionOnTimelineByIdx(activeIcons[i].idx);
                activeIcons[i].icon.anchoredPosition3D = wantedPosition; //activeIcons[i].icon.anchoredPosition3D + (Vector3.left * _iconMoveSpeed * Time.deltaTime);
                if(!activeIcons[i].isHit)
                {
                    activeIcons[i].icon.localScale = Vector3.one * _wantedIconSize;
                }

                if (activeIcons[i].icon.anchoredPosition3D.x < 0)
                {
                    activeIcons[i].icon.gameObject.SetActive(false);
                    inactiveIcons.Add(activeIcons[i]);
                    activeIcons.RemoveAt(i);
                }
            }

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
            _length = progression[0].length;
            _beatLength = _length / (float)pressSequence.Length;
            _pixelsPerBeat = (lineWidth - lineOffset) / beatPreview;
            _pixelsPerSecond = (lineWidth - lineOffset) / (beatPreview * _beatLength);
            _accuracyLength = _beatLength * beatAccuracy;
            Debug.Log("Pixels per beat: " + _pixelsPerBeat);
            //_iconMoveSpeed = (lineWidth - lineOffset) / (beatPreview * _beatLength);
        }

        // Note: Doesn't work after the first iteration because of looping
        float GetPositionOnTimelineByIdx(int idx)
        {
            int dif = (idx - _idx);

            float wantedPos = (dif * _pixelsPerBeat + lineOffset) - ((_timer % _beatLength) * _pixelsPerSecond); //((_timer - (_idx * _beatLength)) * _pixelsPerBeat) + (dif * _pixelsPerBeat);  //(dif * _pixelsPerBeat) - ((_timer % _beatLength) * _pixelsPerBeat);

            return wantedPos;
        }

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
            
            if(Mathf.Approximately(proximityPower,0f) && !ignoreProximity)
            {
                return;
            }

            for (int i = 0; i < Mathf.Min(activeIcons.Count, 4); i++)
            {
                if(activeIcons[i].isHit)
                {
                    continue;
                }
                float powerMult = ignoreProximity ? 1.0f : proximityPower;

                float wantedTime = activeIcons[i].idx * _beatLength;
                float delta = Mathf.Abs(wantedTime - _timer);
                //Debug.Log(delta);
                if (delta < _accuracyLength)
                {
                    //Image img = activeIcons[i].icon.gameObject.GetComponent<Image>();
                    //img.color = Color.green;
                    Animator anm = activeIcons[i].icon.gameObject.GetComponent<Animator>();
                    anm.SetTrigger("hit");
                    //activeIcons[i].icon.localScale = Vector3.one * 1.25f;
                    activeIcons[i].isHit = true;
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
            }
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
            bool shouldSpawn = _wantedIconSize > minIconSize || spawnIconIfMin;//Mathf.Approximately(Mathf.Max(_wantedIconSize, minIconSize), minIconSize)
            int wantedIdx = (_idx + beatPreview) % pressSequence.Length;

            if( pressSequence[_idx] )
            {
                mOnBeat.Invoke();
            }

            if (visualIcon != null && pressSequence[ wantedIdx] && shouldSpawn)
            {
                if (inactiveIcons.Count == 0)
                {
                    RectTransform icon = Instantiate(visualIcon, lineParent);
                    icon.anchoredPosition3D = new Vector3(lineWidth, 0, 0);
                    activeIcons.Add(new BeatIcon(icon, wantedIdx));
                }
                else
                {
                    BeatIcon beatIcon = inactiveIcons[0];
                    inactiveIcons.RemoveAt(0);
                    beatIcon.icon.gameObject.SetActive(true);
                    beatIcon.isHit = false;
                    beatIcon.idx = wantedIdx;
                    beatIcon.icon.anchoredPosition3D = new Vector3(lineWidth, 0, 0);
                    activeIcons.Add(beatIcon);
                }
            }

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

