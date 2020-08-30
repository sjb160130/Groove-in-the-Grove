using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NatureLad
{
    public class FollowManager : MonoBehaviour
    {
        public List<Transform> followLine = new List<Transform>();

        public UnityEvent mOnFollow = new UnityEvent();
        public UnityEvent mOnUnfollow = new UnityEvent();

        public void Unfollow(Follower f)
        {
            int idx = followLine.IndexOf(f.transform);
            if(idx < followLine.Count-1)
            {
                Follower child = followLine[idx + 1].gameObject.GetComponent<Follower>();
                child.SetTarget(followLine[Mathf.Max(idx-1,0)]);
            }

            followLine.RemoveAt(idx);

            mOnUnfollow.Invoke();
        }

        public void Follow(Follower f)
        {
            f.SetTarget(followLine[followLine.Count - 1]);
            followLine.Add(f.transform);

            mOnFollow.Invoke();
        }
    }
}

