using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace DaftAppleGames.Buildings
{
    public class BuildingLights : MonoBehaviour
    {
        /// <summary>
        /// Helper class to store list of lights and candle config
        /// </summary>
        private class LightConfig
        {
            public readonly string lightName;
            public readonly string lightParentName;
            private readonly GameObject _lightGameObject;
            private readonly GameObject _flameGameObject;
            private readonly Light _light;
            private int _index;
            private readonly bool _isCandleOrTorch;

            /// <summary>
            /// Turn on light
            /// </summary>
            public void TurnOn()
            {
                _lightGameObject.SetActive(true);
                if (_flameGameObject)
                {
                    _flameGameObject.SetActive(true);
                }
            }
            
            /// <summary>
            /// Turn off light
            /// </summary>
            public void TurnOff()
            {
                _lightGameObject.SetActive(false);
                if (_flameGameObject)
                {
                    _flameGameObject.SetActive(false);
                }
            }

            /// <summary>
            /// Returns true if light is a candle or torch
            /// </summary>
            /// <returns></returns>
            public bool IsCandleOrTorch()
            {
                return _isCandleOrTorch;
            }
            
            /// <summary>
            /// Set the light state
            /// </summary>
            /// <param name="state"></param>
            public void SetLightState(bool state)
            {
                _lightGameObject.SetActive(state);
                if (_flameGameObject)
                {
                    _flameGameObject.SetActive(state);
                }
            }

            /// <summary>
            /// Configure the Light Source
            /// </summary>
            /// <param name="range"></param>
            /// <param name="intensity"></param>
            /// <param name="radius"></param>
            public void ConfigureLightSource(float range, float intensity, float radius)
            {
                _light.range = range;
                _light.intensity = intensity;
#if HDPipeline
                HDAdditionalLightData lightData = _light.gameObject.GetComponent<HDAdditionalLightData>();
                if (lightData)
                {
                    lightData.shapeRadius = radius;
                }
#else                
#endif
            }
            

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="index"></param>
            /// <param name="lightName"></param>
            /// <param name="lightParentName"></param>
            /// <param name="lightGameObject"></param>
            /// <param name="light"></param>
            /// <param name="flameGameObject"></param>
            /// <param name="isCandleOrTorch"></param>
            public LightConfig(int index, string lightName, string lightParentName, GameObject lightGameObject, Light light, GameObject flameGameObject, bool isCandleOrTorch)
            {
                this.lightName = lightName;
                this.lightParentName = lightParentName;
                _index = index;
                _lightGameObject = lightGameObject;
                _light = light;
                _flameGameObject = flameGameObject;
                _isCandleOrTorch = isCandleOrTorch;
            }
            
            /// <summary>
            /// Simplified constructor
            /// </summary>
            /// <param name="index"></param>
            /// <param name="lightName"></param>
            /// <param name="lightParentName"></param>
            /// <param name="lightGameObject"></param>
            /// <param name="light"></param>
            /// <param name="isCandleOrTorch"></param>
            public LightConfig(int index, string lightName, string lightParentName, GameObject lightGameObject, Light light, bool isCandleOrTorch)
            {
                this.lightName = lightName;
                this.lightParentName = lightParentName;
                _index = index;
                _lightGameObject = lightGameObject;
                _light = light;
                _isCandleOrTorch = isCandleOrTorch;
            }
        }

        [Header("Building light behaviour")]
        [Tooltip("Set this for this buildings light config to override the global configuration.")]
        public bool overrideGlobal;
        [Tooltip("Set this to true to only impact candle and torch light sources.")]
        public bool candleAndTorchOnly;
        [Tooltip("Set this if you want only child Light components to be effected. Leaving this blank will assume all lights within the parent Game Object to be effected.")]
        public GameObject parentBuildingGameObject;
        [Header("Light configuration")]
        public float radius = 3;
        public float intensity = 600;
        public float range = 20;
        [Header("Events")]
        public UnityEvent lightsOnEvent;
        public UnityEvent lightsOffEvent;
        public UnityEvent lightsToggleEvent;
        
        // Useful for debugging
        private bool _isLightsOn;
        private List<LightConfig> _allLights;
        private int _lightCount;

        /// <summary>
        /// Configure all lights in the attached building object
        /// </summary>
        void Start()
        {
            ConfigureBuildingLights();
        }
    
        /// <summary>
        /// Turn on all lights
        /// </summary>
        public void TurnOnLights()
        {
            SetLightsState(true);
            _isLightsOn = true;
        }

        /// <summary>
        /// Turn off all lights
        /// </summary>
        public void TurnOffLights()
        {
            SetLightsState(false);
            _isLightsOn = false;
        }

        /// <summary>
        /// Toggle lights between on / off states
        /// </summary>
        public void ToggleLights()
        {
            SetLightsState(!_isLightsOn);
        }
        
        /// <summary>
        /// Sets the state of all lights in the building
        /// </summary>
        /// <param name="state"></param>
        public void SetLightsState(bool state)
        {
            foreach (LightConfig singleLight in _allLights)
            {
                if (!candleAndTorchOnly || (candleAndTorchOnly && singleLight.IsCandleOrTorch()))
                {
                    singleLight.SetLightState(state);
                }
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
        /// Configure all lights in the building with given properties
        /// </summary>
        private void ConfigureBuildingLights()
        {
            // Get all "Light" components from parent
            Light[] allLightsInBuilding;
            _allLights = new List<LightConfig>();
            
            if (parentBuildingGameObject)
            {
                allLightsInBuilding = parentBuildingGameObject.GetComponentsInChildren<Light>();
            }
            else
            {
                allLightsInBuilding = transform.parent.GetComponentsInChildren<Light>();
            }

            int currentIndex = 0;
            // Iterate through lights and add to global building list
            foreach (Light singleLight in allLightsInBuilding)
            {
                // Check if this is a candle or torch
                string lightName = singleLight.gameObject.name;
                string parentName = singleLight.gameObject.transform.parent.gameObject.name.ToLower();
                bool isCandleOrTorch = parentName.Contains("candle") || parentName.Contains("torch");
                
                // Look for the candle "Flame" particle GameObject
                LightConfig newLightConfig;
                if (isCandleOrTorch)
                {
                    ParticleSystem flameParticle = singleLight.gameObject.transform.parent.gameObject
                        .GetComponentInChildren<ParticleSystem>();
                    if (flameParticle)
                    {
                        newLightConfig = new LightConfig(currentIndex, lightName, parentName, singleLight.transform.parent.gameObject,
                            singleLight, flameParticle.gameObject, true);
                    }
                    else
                    {
                        newLightConfig = new LightConfig(currentIndex, lightName, parentName, singleLight.transform.parent.gameObject,
                            singleLight, true);
                    }
                }
                else
                {
                    newLightConfig = new LightConfig(currentIndex, lightName, parentName, singleLight.transform.parent.gameObject,
                        singleLight, false);
                }

                // Reconfigure light settings if overriding global controller settings
                if (overrideGlobal)
                {
                    newLightConfig.ConfigureLightSource(range, intensity, radius);
                    Debug.Log($"Local light source configured: {newLightConfig.lightParentName} / {newLightConfig.lightName}");
                }

                // Add to building list
                _allLights.Add(newLightConfig);
                currentIndex++;
            }

            _lightCount = _allLights.Count;
            if (parentBuildingGameObject)
            {
                Debug.Log($"Configured: {_lightCount} lights on {parentBuildingGameObject.name}");
            }
            else
            {
                Debug.Log($"Configured: {_lightCount} lights.");

            }
        }

        /// <summary>
        /// Apply new configuration for building lights and candles
        /// </summary>
        public void ReConfigureLightProperties(float newRange, float newIntensity, float newRadius)
        {
            if (!overrideGlobal)
            {
                foreach (LightConfig singleLight in _allLights)
                {
                    if (!candleAndTorchOnly || (candleAndTorchOnly && singleLight.IsCandleOrTorch()))
                    {
                        singleLight.ConfigureLightSource(newRange, newIntensity, newRadius);
                        Debug.Log($"Global light source configured: {singleLight.lightParentName} / {singleLight.lightName}");
                    }
                }
            }
        }
    }
}