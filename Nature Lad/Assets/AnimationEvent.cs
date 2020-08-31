using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace NatureLad
{

}
public class AnimationEvent : MonoBehaviour
{
    public UnityEvent mOnHoot = new UnityEvent();

    public void Hoot()
    {
        mOnHoot.Invoke();
    }
}
