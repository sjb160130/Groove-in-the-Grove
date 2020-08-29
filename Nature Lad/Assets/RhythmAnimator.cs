using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    public class RhythmAnimator : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private AudioSource _audioSource;

        // Update is called once per frame
        void Update()
        {
            if (_animator)
            {
                _animator.SetFloat("time", _audioSource.time / _audioSource.clip.length);
            }
        }
    }
}
