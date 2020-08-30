using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    public class GameMaster : MonoBehaviour
    {
        public FollowManager followManager { get { return _followManager; } }
        
        [SerializeField]
        private FollowManager _followManager;
        [SerializeField]
        private RhythmTracker _owlRhythmTracker;
        [SerializeField]
        private Animator _owlAnimator;
        [SerializeField]
        private List<RhythmTracker> _followerRhythmTrackers = new List<RhythmTracker>();

        public float endGameMinRange;
        public float endGameMaxRange;

        [SerializeField]
        private AnimationCurve _wakeBlendCurve = new AnimationCurve(new Keyframe(0,0), new Keyframe(1,1));

        // Start is called before the first frame update
        void Start()
        {
            _followManager.mOnFollow.AddListener(AddFollower);
            _followManager.mOnUnfollow.AddListener(RemoveFollower);
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void EnterOwlStage()
        {
            // Let's check if all of the animals are with us. If so, then we'll turn
            // off all of the other animal attrition settings so we don't leave
        
            
        }

        public void AddFollower()
        {
            HandleFollowerChange();
        }

        public void RemoveFollower()
        {
            HandleFollowerChange();
        }

        void HandleFollowerChange()
        {
            float percentageOfFollowers = (_followManager.followLine.Count - 1f) / (float)_followerRhythmTrackers.Count;
            _owlAnimator.SetFloat("Blend", _wakeBlendCurve.Evaluate(percentageOfFollowers));

            if (Mathf.Approximately(percentageOfFollowers, 1f))
            {
                _owlRhythmTracker.SetProximityMin(endGameMinRange);
                _owlRhythmTracker.SetProximityMax(endGameMaxRange);
            }
            else
            {
                _owlRhythmTracker.SetProximityMin(.1f);
                _owlRhythmTracker.SetProximityMax(.2f);
            }
        }
    }
}
