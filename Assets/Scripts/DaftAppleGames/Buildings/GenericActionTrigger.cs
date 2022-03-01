using System;
using UnityEngine;
using UnityEngine.Events;

namespace DaftAppleGames.Buildings
{
    /// <summary>
    /// Generic trigger component for things like opening doors, chests etc..
    /// </summary>
    public class GenericActionTrigger : MonoBehaviour
    {
        [Header("Basic Config")]
        public string actionName;
        [Header("UI Config")]
        public KeyCode keyboardKey;
        public KeyCode gamePadButton;
        [Header("Events")]
        public UnityEvent actionEvent;
        public UnityEvent triggerEnterEvent;
        public UnityEvent triggerExitEvent;
        
        private bool _inTriggerArea;

        // Check we have everything set up in the prefab
        void Start()
        {
            if (!GetComponent<Collider>())
            {
                Debug.LogError("Generic Action Trigger: No collider found! Check your GameObject!");
            }

            _inTriggerArea = false;
        }

        /// <summary>
        /// Invoke event when player leaves the collider
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"In OnTriggerEnter ({gameObject.name} collider {other.gameObject.name} )");
                _inTriggerArea = true;
                triggerEnterEvent.Invoke();
            }
        }

        /// <summary>
        /// Invoke event when player enters the collider
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log($"In OnTriggerExit ({gameObject.name} collider {other.gameObject.name})");
                _inTriggerArea = false;
                triggerExitEvent.Invoke();
            }
        }

        /// <summary>
        /// Check for keypress and invoke event if player is in the collider
        /// </summary>
        public void Update()
        {
            if ((Input.GetKeyDown(keyboardKey) || Input.GetKeyDown(gamePadButton)) && _inTriggerArea)
            {
                Debug.Log($"In Trigger Action: ({gameObject.name})");
                _inTriggerArea = false;
                actionEvent.Invoke();
            }
        }
/*
        public void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (Input.GetKeyDown(keyboardKey) || Input.GetKeyDown(gamePadButton))
                {
                    actionEvent.Invoke();
                }
            }
        }
*/
    }

}