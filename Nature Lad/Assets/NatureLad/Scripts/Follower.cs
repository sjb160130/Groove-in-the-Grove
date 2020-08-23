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

        

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Vector3 delta = target.position - transform.position;
            targetVelocity = target.position - _lastTargetPosition;

            velocity = Vector3.Lerp( velocity, Vector3.ClampMagnitude((target.position - (delta.normalized * minDistance) ) - transform.position, maxSpeed * Time.fixedDeltaTime) * Mathf.Min(targetVelocity.magnitude / Time.fixedDeltaTime, 1f), Time.fixedDeltaTime * damp);

            _lastTargetPosition = target.position;

            transform.position = transform.position + velocity;

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position + Vector3.up*1f, transform.TransformDirection(Vector3.down), out hit, 5f))
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
    }
}

