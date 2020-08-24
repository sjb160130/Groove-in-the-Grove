using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

namespace NatureLad
{
    public class Follower : MonoBehaviour
    {
        public Transform target;
        public float minDistance = 1f;

        public float damp = 1f;
        public float maxSpeed = 4f;

        public float radius = .25f;

        private Vector3 velocity = Vector3.zero;

        private Vector3 targetVelocity;
        private Vector3 _lastTargetPosition;

        private bool _isFollowing;

        private Vector3 _startingPosition;
        private Quaternion _startingRotation;

        // Start is called before the first frame update
        void Start()
        {
            _startingPosition = transform.position;
            _startingRotation = transform.rotation;
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            Vector3 delta = Vector3.zero;
            Vector3 wantedVelocity = Vector3.zero;
            Vector3 targetPosition = _startingPosition;
            
            if(target && _isFollowing)
            {
                // Check if character is off screen, if so, teleport them closer
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                if (screenPos.x < -100f || screenPos.y < -100f || screenPos.x > Screen.width+100f || screenPos.y > Screen.height+100f)
                {
                    transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Mathf.Clamp(screenPos.x, -10f, Screen.width + 10f), Mathf.Clamp(screenPos.y, -10f, Screen.height + 10f), screenPos.z));
                }

                targetPosition = target.position;

                delta = targetPosition - transform.position;
                targetVelocity = targetPosition - _lastTargetPosition;
                _lastTargetPosition = targetPosition;

                targetPosition = (target.position - (delta.normalized * minDistance));
                wantedVelocity = targetPosition - transform.position;
            }
            else
            {
                wantedVelocity = _startingPosition - transform.position;
                targetVelocity = wantedVelocity;
            }

            wantedVelocity = Vector3.ClampMagnitude(wantedVelocity, maxSpeed * Time.fixedDeltaTime) * Mathf.Min( targetVelocity.magnitude / Time.fixedDeltaTime, 1f);
            velocity = Vector3.Lerp( velocity, wantedVelocity, Time.fixedDeltaTime * damp);

            transform.position = transform.position + velocity;

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position + Vector3.up*1f, transform.TransformDirection(Vector3.down), out hit, 50f))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
                //Debug.Log("Did Hit");

                transform.position = hit.point + Vector3.up*radius;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 5f, Color.white);
                //Debug.Log("Did not Hit");
            }
            //transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime);
        }
        public void SetTarget(Transform t)
        {
            target = t;
        }

        [Button("Activate")]
        public void Follow()
        {
            _isFollowing = true;
        }

        [Button("Deactivate")]
        public void StopFollowing()
        {
            _isFollowing = false;
        }
    }
}

