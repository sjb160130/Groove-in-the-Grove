using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    public class FollowManager : MonoBehaviour
    {
        public List<Transform> followLine = new List<Transform>();

        public void Unfollow(Follower f)
        {
            int idx = followLine.IndexOf(f.transform);
            if(idx < followLine.Count-1)
            {
                Follower child = followLine[idx + 1].gameObject.GetComponent<Follower>();
                child.SetTarget(followLine[Mathf.Max(idx-1,0)]);
            }

            followLine.RemoveAt(idx);
        }

        public void Follow(Follower f)
        {
            followLine.Add(f.transform);
        }
    }
}

