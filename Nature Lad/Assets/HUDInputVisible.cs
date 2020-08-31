using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NatureLad
{
    public class HUDInputVisible : MonoBehaviour
    {
        public GameObject keyboard;
        public GameObject controller;

        vHUDController hud;

        // Start is called before the first frame update
        void Start()
        {
            InvokeRepeating("CheckUpdate", .2f, .3f);
        }

        void CheckUpdate()
        {
            if(!hud)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                hud = player.GetComponent<vThirdPersonInput>().hud;
            }

            if(hud.controllerInput)
            {
                if(!controller.activeSelf)
                {
                    controller.SetActive(true);
                    keyboard.SetActive(false);
                }
            }
            else
            {
                if (controller.activeSelf)
                {
                    controller.SetActive(false);
                    keyboard.SetActive(true);
                }
            }
        }
    }
}
