using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using UnityEditor.UIElements;
using UnityEngine;

namespace NatureLad
{
    public class Follower : MonoBehaviour
    {
        [SerializeField]
        private FollowManager followManager;
        public Transform target;

        public float minDistance = 1f;

        public float damp = 1f;
        public float maxSpeed = 4f;

        public float returnHomeSpeed = 1f;

        public float radius = .25f;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Vector3 velocity = Vector3.zero;

        private Vector3 targetVelocity;
        private Vector3 _lastTargetPosition;
        private Vector3 _lastPosition;
        private Vector3 _velocity = Vector3.zero;

        [SerializeField]
        private bool _isFollowing = false;

        private Vector3 _startingPosition;
        private Quaternion _startingRotation;

        // Start is called before the first frame update
        void Start()
        {
            _startingPosition = transform.position;
            _startingRotation = transform.rotation;
            _lastPosition = transform.position;
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            Vector3 delta = Vector3.zero;
            Vector3 wantedVelocity = Vector3.zero;
            Vector3 targetPosition = _startingPosition;

            float distanceToTarget = 0f;
            Quaternion wantedRotation = transform.rotation;

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

                Vector3 lookAtVector = target.position - transform.position;
                lookAtVector.y = 0f;
                lookAtVector.Normalize();

                Quaternion velocityRotation = Quaternion.LookRotation(new Vector3(wantedVelocity.x, 0f, wantedVelocity.z).normalized, Vector3.up);
                Quaternion lookAtRotation = Quaternion.LookRotation(lookAtVector, Vector3.up);
                wantedRotation = Quaternion.Slerp(lookAtRotation, velocityRotation, _velocity.magnitude);
            }
            else
            {
                wantedVelocity = _startingPosition - transform.position;
                targetVelocity = wantedVelocity;

                wantedRotation = _velocity.magnitude > .1f ? Quaternion.LookRotation(new Vector3(wantedVelocity.x, 0f, wantedVelocity.x).normalized, Vector3.up) : _startingRotation;
            }
            
            transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.fixedDeltaTime * 2f);

            distanceToTarget = wantedVelocity.magnitude;
            wantedVelocity = Vector3.ClampMagnitude(wantedVelocity, ( (_isFollowing ? maxSpeed : returnHomeSpeed) * Time.fixedDeltaTime)) * Mathf.Min( distanceToTarget > radius*4f ? 1.0f : (targetVelocity.magnitude / Time.fixedDeltaTime), 1f);
            velocity = Vector3.Lerp( velocity, wantedVelocity, Time.fixedDeltaTime * damp);

            transform.position = transform.position + velocity;

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 50f))
            {
                Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.yellow);
                //Debug.Log("Did Hit: " + hit.collider);

                transform.position = hit.point + Vector3.up*radius;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 5f, Color.white);
                //Debug.Log("Did not Hit");
            }
            //transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime);

            _velocity = transform.position - _lastPosition;

            if (animator != null)
            {
                animator.SetFloat("speed", _velocity.magnitude / Time.fixedDeltaTime);
            }

            _lastPosition = transform.position;

        }
        public void SetTarget(Transform t)
        {
            target = t;
        }

        [Button("Follow")]
        public void Follow()
        {
            if(_isFollowing)
            {
                return;
            }
            _isFollowing = true;
            if(followManager)
            {
                followManager.Follow(this);
            }
        }

        [Button("Stop Following")]
        public void StopFollowing()
        {
            if (!_isFollowing)
            {
                return;
            }
            _isFollowing = false;
            if (followManager)
            {
                followManager.Unfollow(this);
            }
        }
    }
}

