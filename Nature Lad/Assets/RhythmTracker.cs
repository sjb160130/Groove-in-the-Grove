using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
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
        [TableMatrix(DrawElementMethod = "DrawCell")]
        public bool[,] sequence = new bool[64, 2];

        [SerializeField]
        private float power = 0f;
        [SerializeField]
        private float attenuation = .1f;
        [SerializeField]
        private float powerIncrease = .1f;

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

        public List<BeatIcon> activeIcons = new List<BeatIcon>();

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            if(progression[0])
            {
                PrecalculateData();
            }

            Play();
        }

        // Update is called once per frame
        void Update()
        {
            if(_audioSource.time < _timer)
            {
                _timer -= _audioSource.clip.length;
            }
            _deltaAudioTime = _audioSource.time - _timer;
            _timer = _audioSource.time;

            //float delta = _timer - beatOffset;
            //if(delta < 0f)
            //{
            //    _timer = _length + delta;
            //}

            int currentIdx = (int)Mathf.Floor(_timer / _beatLength);
            if (_idx != currentIdx)
            {
                _idx = currentIdx;
                UpdateBeat();
            }

            _inHitWindow = sequence[_idx, 0];
            _inReleaseWindow = sequence[_idx, 1];

            for( int i = 0; i < activeIcons.Count; i++)
            {
                Vector3 wantedPosition = activeIcons[i].icon.anchoredPosition3D;
                wantedPosition.x -= _deltaAudioTime * _pixelsPerSecond; //GetPositionOnTimelineByIdx(activeIcons[i].idx);
                activeIcons[i].icon.anchoredPosition3D = wantedPosition; //activeIcons[i].icon.anchoredPosition3D + (Vector3.left * _iconMoveSpeed * Time.deltaTime);

                if (activeIcons[i].icon.anchoredPosition3D.x < 0)
                {
                    GameObject.Destroy(activeIcons[i].icon.gameObject);
                    activeIcons.RemoveAt(i);
                }
            }

            power = Mathf.Lerp(power, 0f, Time.deltaTime * attenuation);
            _audioSource.volume = Mathf.Lerp(_audioSource.volume, power, Time.deltaTime*2f);
        }

        void PrecalculateData()
        {
            _length = progression[0].length;
            _beatLength = _length / (sequence.Length / 2.0f);
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

            
            for (int i = 0; i < Mathf.Min(activeIcons.Count, 4); i++)
            {
                if(activeIcons[i].isHit)
                {
                    continue;
                }

                float wantedTime = activeIcons[i].idx * _beatLength;
                float delta = Mathf.Abs(wantedTime - _timer);
                //Debug.Log(delta);
                if (delta < _accuracyLength)
                {
                    Image img = activeIcons[i].icon.gameObject.GetComponent<Image>();
                    img.color = Color.green;
                    activeIcons[i].icon.localScale = Vector3.one * 1.25f;
                    activeIcons[i].isHit = true;
                    _audioSource.volume = 1.0f;
                    power = Mathf.Min(power + powerIncrease, 1.0f);
                    break;
                }
            }

            /*
                        if (inHitWindow)
                        {

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
            int wantedIdx = (_idx + beatPreview) % (sequence.Length / 2);
            if (visualIcon != null && sequence[ wantedIdx, 0])
            {
                RectTransform icon = Instantiate(visualIcon, lineParent);
                icon.anchoredPosition3D = new Vector3(lineWidth, 0, 0);
                activeIcons.Add(new BeatIcon(icon, wantedIdx));
            }
        }


        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200, 20), _timer.ToString());
            GUI.Label(new Rect(10, 70, 200, 20), _idx.ToString());
            GUI.Label(new Rect(10, 30, 200, 20), _inHitWindow.ToString());
            GUI.Label(new Rect(10, 50, 200, 20), _inReleaseWindow.ToString());
        }

        private static bool DrawCell(Rect rect, bool value)
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
        }
    }
}

