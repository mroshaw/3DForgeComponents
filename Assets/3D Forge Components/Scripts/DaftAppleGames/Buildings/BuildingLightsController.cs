using System;
using UnityEngine;
using UnityEngine.Events;

namespace DaftAppleGames.Buildings
{
    public class BuildingLightsController : MonoBehaviour
    {
        [Header("Global lighting overrides")]
        [Tooltip("If unselected, all lights will use their own configuration. Lights that have override = TRUE will not be affected by this setting.")]
        public bool reconfigureAllLights = true;
        public float radius = 3;
        public float intensity = 600;
        public float range = 20;

        [Header("Lights schedule")]
        public int[] lightsOnHours;
        public int[] lightsOffHours;

        [Header("Debug")]
        [Tooltip("Use this to toggle lights on and off for testing purposes")]
        public KeyCode debugToggleLights = KeyCode.L;

        [Header("Events")]
        public UnityEvent lightsOnEvent;
        public UnityEvent lightsOffEvent;
        public UnityEvent lightsToggleEvent;
        
        private BuildingLights[] _allBuildingLights;
        
        // Useful debug
        private bool _isLightsOn;
        
        /// <summary>
        /// Determine current time and set initial light state
        /// </summary>
        public void Start()
        {
            // Get list of all lights
            _allBuildingLights = FindObjectsOfType<BuildingLights>();
            
            // Configure using global settings. Note that any BuildingLights with Override = true,
            // will not be re-configured
            if (reconfigureAllLights)
            {
                ConfigureAllLights();
            }
            
            // Default to Lights On
            TurnOnLights();
        }

        /// <summary>
        /// Configure all lights with Global configuration
        /// </summary>
        public void ConfigureAllLights()
        {
            if (reconfigureAllLights)
            {
                foreach (BuildingLights buildingLight in _allBuildingLights)
                {
                    buildingLight.ReConfigureLightProperties(range, intensity, radius);
                }
            }
        }

        /// <summary>
        /// This public method can be called from an Event, such as Enviro's "OnHourPast" event.
        /// that passes in the current hour. This method will evaluate whether the light state should change
        /// and enact that change.
        /// </summary>
        /// <param name="hour"></param>
        public void UpdateLightStatus(int hour)
        {
            if (Array.Exists(lightsOnHours, onHour => onHour == hour))
            {
                TurnOnLights();
            }
            
            if (Array.Exists(lightsOffHours, offHour => offHour == hour))
            {
                TurnOffLights();
            }
        }
        
        /// <summary>
        /// Turn on all building lights
        /// </summary>
        private void TurnOnLights()
        {
            SetAllBuildingLightsState(true);
            _isLightsOn = true;
        }

        /// <summary>
        /// Turn off all building lights
        /// </summary>
        private void TurnOffLights()
        {
            SetAllBuildingLightsState(false);
            _isLightsOn = false;
        }

        /// <summary>
        /// Toggle light state
        /// </summary>
        private void ToggleLights()
        {
            SetAllBuildingLightsState(!_isLightsOn);
            lightsToggleEvent.Invoke();
        }

        /// <summary>
        /// Set the state of all building lights
        /// </summary>
        /// <param name="state"></param>
        private void SetAllBuildingLightsState(bool state)
        {
            // Debug.Log($"Updating {allLights.Length} buildings...");
            foreach (BuildingLights buildingLight in _allBuildingLights)
            {
                buildingLight.SetLightsState(state);
            }

            _isLightsOn = state;

            if (_isLightsOn)
            {
                lightsOnEvent.Invoke();
            }
            else
            {
                lightsOffEvent.Invoke();
            }
        }

        /// <summary>
        /// Check for debug key press
        /// </summary>
        public void Update()
        {
            // Debug
            if (Input.GetKeyDown(debugToggleLights))
            {
                // Debug.Log("Toggling lights...");
                ToggleLights();
            }
        }
    }
}