using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    public class SoftFollow : MonoBehaviour
    {
        public Transform target;
        public float damp = 2f;

        // Update is called once per frame
        void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * damp);
            //transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * damp);
        }
    }


}
