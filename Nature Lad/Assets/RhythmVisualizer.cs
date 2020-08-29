using NatureLad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{

}

[RequireComponent(typeof(RhythmTracker))]
public class RhythmVisualizer : MonoBehaviour
{
    private RhythmTracker _rhythmTracker;

    [SerializeField]
    private Transform parent;
    [SerializeField]
    private Transform visualTransform;
    [SerializeField]
    private bool isRectTransform;

    [SerializeField]
    private Vector3 line;

    [SerializeField]
    private Vector3 lineOffset;

    [SerializeField]
    private int beatPreview;

    [SerializeField]
    private List<Beat> activeBeats = new List<Beat>();
    [SerializeField]
    private List<Beat> inactiveBeats = new List<Beat>();

    private Vector3 _offsetPerSecond = Vector3.zero;
    private Vector3 _offsetPerBeat = Vector3.zero;

    private float _wantedSize;

    [SerializeField]
    private bool spawnVisualIfMin = false;

    [SerializeField]
    private float minSize = .5f;

    // Start is called before the first frame update
    void Awake()
    {
        _rhythmTracker = GetComponent<RhythmTracker>();
        _rhythmTracker.mOnUpdateBeat.AddListener(OnUpdateBeat);
        _rhythmTracker.mOnInit.AddListener(Init);
        _rhythmTracker.mOnHitIdx.AddListener(OnHit);
    }
    
    void Init()
    {
        _offsetPerBeat = (line - lineOffset) / (float)beatPreview;
        _offsetPerSecond = (line - lineOffset) / (beatPreview * _rhythmTracker.beatLength);
    }

    void Update()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        _wantedSize = Mathf.Lerp(minSize, 1f, _rhythmTracker.power);

        if (!_rhythmTracker.isFollowing)
        {
            _wantedSize = Mathf.Max((_rhythmTracker.proximityPower >= .95f ? minSize + .01f : 0f), _wantedSize);
        }

        // Move and scale icons
        for (int i = 0; i < activeBeats.Count; i++)
        {
            Vector3 wantedPosition = Vector3.zero;
            
            if(isRectTransform)
            {
                RectTransform rt = activeBeats[i].transform as RectTransform;
                wantedPosition = rt.anchoredPosition3D - (_rhythmTracker.deltaAudioTime * _offsetPerSecond);
                rt.anchoredPosition3D = wantedPosition; //activeIcons[i].icon.anchoredPosition3D + (Vector3.left * _iconMoveSpeed * Time.deltaTime);

                if ( Vector3.Dot(rt.anchoredPosition3D.normalized, line.normalized) < 0f )
                {
                    rt.gameObject.SetActive(false);
                    inactiveBeats.Add(activeBeats[i]);
                    activeBeats.RemoveAt(i);
                }
            }
            else 
            {
                wantedPosition = activeBeats[i].transform.localPosition - (_rhythmTracker.deltaAudioTime * _offsetPerSecond);
                activeBeats[i].transform.localPosition = wantedPosition;

                if ( Vector3.Dot(activeBeats[i].transform.localPosition.normalized, line.normalized) < 0f )
                {
                    activeBeats[i].transform.gameObject.SetActive(false);
                    inactiveBeats.Add(activeBeats[i]);
                    activeBeats.RemoveAt(i);
                }
            }
            
            if (!activeBeats[i].isHit)
            {
                activeBeats[i].transform.localScale = Vector3.one * _wantedSize;
            }
        }
    }

    public void OnHit(int idx)
    {
        if(!isActiveAndEnabled)
        {
            return;
        }

        for (int i = 0; i < Mathf.Min(activeBeats.Count, 4); i++)
        {
            if (activeBeats[i].isHit)
            {
                continue;
            }

            if(activeBeats[i].idx == idx)
            {
                //Image img = activeIcons[i].icon.gameObject.GetComponent<Image>();
                //img.color = Color.green;
                Animator anm = activeBeats[i].transform.gameObject.GetComponent<Animator>();
                anm.SetTrigger("hit");
                activeBeats[i].transform.localScale = Vector3.one * 1.25f;

                activeBeats[i].isHit = true;
                break;
            }
        }
    }

    public void OnUpdateBeat()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        int idx = _rhythmTracker.currentIdx;

        int wantedIdx = (idx + beatPreview) % _rhythmTracker.pressSequence.Length;
        bool shouldSpawn = _wantedSize > minSize || spawnVisualIfMin;//Mathf.Approximately(Mathf.Max(_wantedIconSize, minIconSize), minIconSize)

        if (visualTransform != null && _rhythmTracker.pressSequence[wantedIdx] && shouldSpawn)
        {
            //Beat lastBeat = _rhythmTracker.activeBeats[_rhythmTracker.activeBeats.Count - 1];

            Beat b;
            if (inactiveBeats.Count == 0)
            {
                Transform t = Instantiate(visualTransform, parent);
                b = new Beat(t, wantedIdx);
                activeBeats.Add(b);
            }
            else
            {
                b = inactiveBeats[0];
                inactiveBeats.RemoveAt(0);
                b.transform.gameObject.SetActive(true);
                b.idx = wantedIdx;
                b.isHit = false;
                activeBeats.Add(b);
            }

            if (visualTransform)
            {
                if (isRectTransform)
                {
                    RectTransform rt = b.transform as RectTransform;
                    rt.anchoredPosition3D = line;
                }
                else
                {
                    b.transform.localPosition = line;
                }
            }
        }
    }
}
