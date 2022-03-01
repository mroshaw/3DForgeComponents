using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    public class BuildingDoorSoundFX : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioClip openDoorClip;
        public AudioClip closeDoorClip;

        /// <summary>
        /// Play close door SFX.
        /// Called from Animation Controller Event
        /// </summary>
        public void PlayCloseDoorClip()
        {
            if (closeDoorClip)
            {
                audioSource.PlayOneShot(closeDoorClip);
            }
        }

        /// <summary>
        /// Play open door SFX.
        /// Called from Animation Controller Event
        /// </summary>
        public void PlayOpenDoorClip()
        {
            if (openDoorClip)
            {
                audioSource.PlayOneShot(openDoorClip);
            }
        }
    }
}