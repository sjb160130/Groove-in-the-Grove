using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    public class CameraFacing : MonoBehaviour
    {
        private Transform _cam;

        // Update is called once per frame
        void Update()
        {
            Vector3 aimPos = Camera.main.transform.position - transform.position;
            aimPos.y = 0f;

            aimPos.Normalize();

            transform.rotation = Quaternion.LookRotation(aimPos, Vector3.up);
        }
    }
}
