using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    public class NatureLadAnimator : MonoBehaviour
    {
        private Animator _animator;

        private float _wantedDrumLayer = 1f;

        // Start is called before the first frame update
        void Start()
        {
            _animator = GetComponent<Animator>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            float currentWeight = _animator.GetLayerWeight(6);
            if( Mathf.Abs(currentWeight - _wantedDrumLayer) > .01f )
            {
                _animator.SetLayerWeight(6, Mathf.Lerp(currentWeight, _wantedDrumLayer, Time.deltaTime*2f));
            }
            else if (!Mathf.Approximately(currentWeight, _wantedDrumLayer))
            {
                _animator.SetLayerWeight(6, _wantedDrumLayer);
            }
        }

        public void SetDrumLayerOpacity(float v)
        {
            _wantedDrumLayer = v;
        }
    }
}
