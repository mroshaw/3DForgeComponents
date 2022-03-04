using System.Collections.Generic;
using UnityEngine;
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
            private readonly GameObject _lightGameObject;
            private GameObject _flameGameObject;
            private readonly Light _light;
            private int _index;
            private bool _isCandle;

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

            public bool IsCandle()
            {
                return _isCandle;
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

            public void ConfigureLightSource(float range, float intensity, float radius)
            {
                _light.range = range;
                _light.intensity = intensity;
#if HDPipeline
                HDAdditionalLightData lightData = _light.gameObject.GetComponentInChildren<HDAdditionalLightData>();
                if (lightData)
                {
                    lightData.shapeRadius = 3.0f;
                }
#else                
#endif
            }
            
            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="index"></param>
            /// <param name="lightGameObject"></param>
            /// <param name="light"></param>
            /// <param name="flameGameObject"></param>
            /// <param name="isCandle"></param>
            public LightConfig(int index, GameObject lightGameObject, Light light, GameObject flameGameObject, bool isCandle)
            {
                _index = index;
                _lightGameObject = lightGameObject;
                _light = light;
                _isCandle = isCandle;
            }
            
            /// <summary>
            /// Simplified constructor
            /// </summary>
            /// <param name="index"></param>
            /// <param name="lightGameObject"></param>
            /// <param name="light"></param>
            /// <param name="isCandle"></param>
            public LightConfig(int index, GameObject lightGameObject, Light light, bool isCandle)
            {
                _index = index;
                _lightGameObject = lightGameObject;
                _light = light;
                _isCandle = isCandle;
            }
            
        }

        [Header("Building light behaviour")]
        [Tooltip("Set this for this buildings light config to override the global configuration.")]
        public bool overrideGlobal;
        [Tooltip("Set this to true to only impact candle light sources.")]
        public bool candlesOnly;
        [Tooltip("Set this if you want only child Light components to be effected. Leaving this blank will assume all lights within the parent Game Object to be effected.")]
        public GameObject parentBuildingGameObject;
        [Header("Light configuration")]
        public float radius = 3;
        public float intensity = 600;
        public float range = 20;

        
        private bool _isLightsOn;
        private List<LightConfig> _allLights;

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
                if (!candlesOnly || (candlesOnly && singleLight.IsCandle()))
                {
                    singleLight.SetLightState(state);
                }
            }
            _isLightsOn = state;
        }

        /// <summary>
        /// Configure all lights in the building with given properties
        /// </summary>
        public void ConfigureBuildingLights()
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
                // Check if this is a candle
                bool isCandle = singleLight.gameObject.name.ToLower().Contains("candle");
                
                // Look for the candle "Flame" particle GameObject
                LightConfig newLightConfig;
                if (isCandle)
                {
                    ParticleSystem flameParticle = singleLight.gameObject.transform.parent.gameObject
                        .GetComponentInChildren<ParticleSystem>();
                    if (flameParticle)
                    {
                        newLightConfig = new LightConfig(currentIndex, singleLight.transform.parent.gameObject,
                            singleLight,
                            flameParticle.gameObject, isCandle);
                    }
                    else
                    {
                        newLightConfig = new LightConfig(currentIndex, singleLight.transform.parent.gameObject,
                            singleLight, isCandle);
                    }
                }
                else
                {
                    newLightConfig = new LightConfig(currentIndex, singleLight.transform.parent.gameObject,
                        singleLight, isCandle);
                }

                // Reconfigure light settings if overriding global controller settings
                if (overrideGlobal)
                {
                    newLightConfig.ConfigureLightSource(range, intensity, radius);
                }

                // Add to building list
                _allLights.Add(newLightConfig);
                currentIndex++;
            }
            Debug.Log($"Configured: {_allLights.Count} lights");
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
                    if (!candlesOnly || (candlesOnly && singleLight.IsCandle()))
                    {
                        singleLight.ConfigureLightSource(newRange, newIntensity, newRadius);
                    }
                }
            }
        }
    }
}