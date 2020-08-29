using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    public class CameraFacing : MonoBehaviour
    {
        private Transform _cam;
        [SerializeField]
        private bool _lockOnYPlane = true;
        [SerializeField]
        private bool _reverseLook = false;
        [SerializeField]
        private bool _reverseUp = false;

        // Update is called once per frame
        void Update()
        {
            Vector3 aimPos = Camera.main.transform.position - transform.position;
            if(_lockOnYPlane)
            {
                aimPos.y = 0f;
            }            

            aimPos.Normalize();

            if(_reverseLook)
            {
                aimPos *= -1f;
            }

            transform.rotation = Quaternion.LookRotation(aimPos, _reverseUp ? Vector3.down : Vector3.up);
        }
    }
}
