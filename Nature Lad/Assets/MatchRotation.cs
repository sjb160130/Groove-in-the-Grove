using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    public class MatchRotation : MonoBehaviour
    {
        public Transform target;
        public float damp = 1f;
        public bool useVelocityMult = true;
        public Vector2 velocityRange = new Vector2(0f, 2f);

        private Vector3 _lastPosition;
        [SerializeField]
        private float _velocityMult = 0f;

        // Update is called once per frame
        void FixedUpdate()
        {
            float velocityMult = Mathf.Clamp((target.transform.position - _lastPosition).magnitude / Time.fixedDeltaTime, velocityRange.x, velocityRange.y) / velocityRange.y;
            _velocityMult = Mathf.Lerp(_velocityMult, velocityMult, Time.fixedDeltaTime);

            _lastPosition = target.transform.position;

            float dotMult = Mathf.Max(1f + Vector3.Dot(transform.forward, target.forward), 1.0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, damp * velocityMult * dotMult * Time.deltaTime);
        }
    }
}

