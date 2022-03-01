#if INVECTOR_BASIC
using Invector.vCharacterController.vActions;
#endif
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    // Looking from outside in, in which corner is the pivot?
    public enum DoorPivot {Left, Right}
    
    /// <summary>
    /// Class to provide door functionality to 3D forge buildings
    /// </summary>
    public class BuildingDoor : MonoBehaviour
    {
        [Header("Sound FX")]
        public AudioClip openDoorClip;
        public AudioClip closeDoorClip;

        [Header("Collider Config")]
        public float colliderHeight;
        public float colliderDepth;

        [Header("Interact Prompt Config")]
        public bool showPrompt;
        public float promptHeight;
        public float promptDepthOffset;

        // Door animator properties
        private Animator _referenceAnimator;
        private Animator _doorAnimator;
        private AudioSource _audioSource;

        // Parent door properties
        private GameObject _doorGameObject;
        private GameObject _doorFrameGameObject;
        private Transform _doorOriginalTransform;
        private float _doorHeight;
        private float _doorWidth;
        private float _doorDepth;
        
        [Header("Behaviour")] 
        [Tooltip("Check this if the door animation is moving in the wrong direction")]
        public bool toggleAnimationDirection;
        [Tooltip("Looking from outside in, in which corner is the pivot?")]
        public DoorPivot doorPivot = DoorPivot.Right;
        [Tooltip("Check this if you'd like the component to guess the pivot and animation direction, based on the name of the door")]
        public bool guessDoorBehaviour = false;
        
        // Start is called before the first frame update
        void Start()
        {
            // Initialise the door properties and setup the parent container
            InitDoorProperties();
            ParentAndZeroDoor();
            
            // If ticked, guess the behaviour config
            if (guessDoorBehaviour)
            {
                GuessDoorBehaviour();
            }
            
            // Set up the Action triggers and colliders
#if INVECTOR_BASIC
            ConfigureInvectorActionTriggers();
#else
            ConfigureGenericActionTriggers();
#endif
            // Configure the door colliders
            ConfigureDoorColliders();
            
            // Set up the audio FX
            ConfigureDoorAudio();
            
            // Set up the animators
            ConfigureDoorAnimator();
        }

        /// <summary>
        /// Based on the name of the door Game Object, guess where the pivot and animation
        /// should be.
        /// </summary>
        private void GuessDoorBehaviour()
        {
            string doorGameObjectName = _doorGameObject.name;
            if (doorGameObjectName.ToLower().Contains("left"))
            {
                doorPivot = DoorPivot.Left;
                toggleAnimationDirection = true;
            }
            else
            {
                doorPivot = DoorPivot.Right;
                toggleAnimationDirection = false;
            }
        }
        
#if INVECTOR_BASIC
        /// <summary>
        /// Configure the Invector vGenericAction components, if Invector in use
        /// </summary>
        private void ConfigureInvectorActionTriggers()
        {
            // Prepare to subscribe to the two action triggers
            var genericActions = GetComponentsInChildren<vTriggerGenericAction>();

            // Look for the open and close triggers. If found, assign the correct animation
            foreach (vTriggerGenericAction action in genericActions)
            {
                switch (action.actionName)
                {
                    case "OpenDoorInward":
                        if (!toggleAnimationDirection)
                        {
                            action.OnPressActionInput.AddListener(DoorOpenInwards);
                        }
                        else
                        {
                            action.OnPressActionInput.AddListener(DoorOpenOutwards);
                        }

                        break;
                    case "OpenDoorOutward":
                        if (!toggleAnimationDirection)
                        {
                            action.OnPressActionInput.AddListener(DoorOpenOutwards);
                        }
                        else
                        {
                            action.OnPressActionInput.AddListener(DoorOpenInwards);
                        }
                        break;
                }
            }
        }
#endif
 
        /// <summary>
        /// Configure a Generic Action Trigger
        /// </summary>
        private void ConfigureGenericActionTriggers()
        {
            // Prepare to subscribe to the two action triggers
            GenericActionTrigger[] genericActionTriggers = GetComponentsInChildren<GenericActionTrigger>();

            // Look for the open and close triggers. If found, assign the correct animation
            foreach (GenericActionTrigger action in genericActionTriggers)
            {
                switch (action.actionName)
                {
                    case "OpenDoorInward":
                        if (!toggleAnimationDirection)
                        {
                            action.actionEvent.AddListener(DoorOpenInwards);
                        }
                        else
                        {
                            action.actionEvent.AddListener(DoorOpenOutwards);
                        }

                        break;
                    case "OpenDoorOutward":
                        if (!toggleAnimationDirection)
                        {
                            action.actionEvent.AddListener(DoorOpenOutwards);
                        }
                        else
                        {
                            action.actionEvent.AddListener(DoorOpenInwards);
                        }
                        break;
                }
            }
        }
        
        /// <summary>
        /// Configure the door animator, using the reference animator in the prefab
        /// </summary>
        private void ConfigureDoorAnimator()
        {
            // Get ready to configure the door animator with our reference controller
            _referenceAnimator = GetComponent<Animator>();

            // Configure the door animator
            _doorAnimator = transform.parent.gameObject.AddComponent<Animator>();
            _doorAnimator.runtimeAnimatorController = _referenceAnimator.runtimeAnimatorController;
        }

        /// <summary>
        /// Set up the door opening and closing animation event handlers
        /// </summary>
        private void ConfigureDoorAudio()
        {
            // Add the source FX controller
            BuildingDoorSoundFX doorFX = transform.parent.gameObject.AddComponent<BuildingDoorSoundFX>();

            // Get the AudioSource
            _audioSource = GetComponent<AudioSource>();

            // Set up the FX component
            doorFX.audioSource = _audioSource;
            doorFX.openDoorClip = openDoorClip;
            doorFX.closeDoorClip = closeDoorClip;
        }
        
        /// <summary>
        /// Find the parent door Game Object and determine it's dimensions
        /// </summary>
        private void InitDoorProperties()
        {
            // Get the door GO
            _doorGameObject = gameObject.transform.parent.gameObject;

            // Get the dimensions of the door
            MeshRenderer doorMesh = _doorGameObject.GetComponent<MeshRenderer>();
            if (!doorMesh)
            {
                Debug.LogError("Couldn't establish dimensions of door!!");
            }

            // Get dimensions from Mesh bounds, in local terms
            Bounds doorBounds = doorMesh.localBounds;
            _doorWidth = doorBounds.size.z;
            _doorHeight = doorBounds.size.y;
            _doorDepth = doorBounds.size.x;
            
            // We don't know what plane, x or z, represents width or depth
            // so derive based on width being larger of the two
            if (_doorWidth < _doorDepth)
            {
                (_doorWidth, _doorDepth) = (_doorDepth, _doorWidth);
            }
            
            // Get the parent container Game Object
            _doorFrameGameObject = _doorGameObject.transform.parent.gameObject;
        }
        
        /// <summary>
        /// Display the "open" prompt
        /// </summary>
        public void ShowPrompt()
        {
            if (showPrompt)
            {
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the "open" prompt
        /// </summary>
        public void HidePrompt()
        {
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Call the open inwards animation
        /// </summary>
        public void DoorOpenInwards()
        {
            _doorAnimator.Play("door_open_in");
        }

        /// <summary>
        /// Call the open outwards animation
        /// </summary>
        public void DoorOpenOutwards()
        {
            _doorAnimator.Play("door_open_out");
        }

        /// <summary>
        /// Creates a parent GameObject that takes the doors current transform.
        /// The door is then "zero'd" to allow animators to assume as starting position of (0, 0, 0)
        /// </summary>
        private void ParentAndZeroDoor()
        {
            // This code "zeros" the parent door transform, and parents to a GameObject with the original
            // transform position and rotation.
            // The idea is that all animations and other calculations can safely assume the door is at 0,0,0
            _doorOriginalTransform = _doorGameObject.transform;
            Vector3 currentDoorPosition = _doorOriginalTransform.localPosition;
            Vector3 newParentPosition = new Vector3(currentDoorPosition.x, currentDoorPosition.y, currentDoorPosition.z);
            Quaternion currentDoorRotation = _doorOriginalTransform.localRotation;
            Quaternion newParentRotation = new Quaternion(currentDoorRotation.x, currentDoorRotation.y,
                currentDoorRotation.z, currentDoorRotation.w);
            
            // Create and set parent GameObject to allow us to "zero" the door
            GameObject parentGo = new GameObject("Door Parent");
            // Get the current door parent, as we'll insert the new zero'd GO in between
            parentGo.transform.SetParent(_doorFrameGameObject.transform);
            // Now set the new GO as the parent of the door
            _doorGameObject.transform.SetParent(parentGo.transform);

            // Reposition the new parent holder, to mimic the original position of the door
            parentGo.transform.localPosition = newParentPosition;
            parentGo.transform.localRotation = newParentRotation;

            // Now Zero the door and the BuildingDoor component GameObjects
            _doorGameObject.transform.localPosition = new Vector3(0, 0, 0);
            transform.localPosition = new Vector3(0, 0, 0);
            _doorGameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
            transform.localRotation = new Quaternion(0, 0, 0, 0);
        }
        
        /// <summary>
        /// Adjust the door and door triggers for consistent triggering
        /// and animation of open and close mechanic
        /// </summary>
        private void ConfigureDoorColliders()
        {
            // Get colliders to adjust
            BoxCollider[] colliders = GetComponentsInChildren<BoxCollider>();
            
            // Configure each collider, based on the name. Either inside or outside.
            foreach (BoxCollider currentCollider in colliders)
            {
                Vector3 colliderCenter = new Vector3();
                Vector3 colliderSize = new Vector3();
                string currentColliderName = currentCollider.gameObject.name;
                
                // Configure the collider, using the door width as a basis
                colliderSize.x = colliderDepth;
                colliderSize.y = colliderHeight;
                colliderSize.z = _doorWidth;
                currentCollider.size = colliderSize;

                // Zero the parent trigger game object
                Transform colliderTransform = currentCollider.transform.parent.transform;
                colliderTransform.localPosition = new Vector3(0, 0, 0);
                colliderTransform.localRotation = new Quaternion(0, 0, 0, 0);

                // Center the collider against the base of the door
                colliderCenter.y = 0.0f;
                // colliderCenter.x = toggleColliderPivot ? _doorDepth / 2 : -_doorDepth / 2;
                colliderCenter.x = (colliderDepth + _doorDepth) / 2;
                colliderCenter.z = doorPivot==DoorPivot.Left ? _doorWidth / 2 : -_doorWidth / 2;
                // colliderCenter.z = currentColliderName == "OpenOutwardTrigger" ? -colliderCenter.z : colliderCenter.z;
                colliderCenter.x = currentColliderName == "OpenOutwardTrigger" ? -colliderCenter.x : colliderCenter.x;
                currentCollider.center = colliderCenter;
                
                // Configure the Action Prompt
                SetupActionPrompt(currentCollider.gameObject, currentColliderName);
            }
        }
        
        /// <summary>
        /// Positions the "Action Prompt" UI element
        /// </summary>
        /// <param name="triggerGo"></param>
        /// <param name="currentColliderName"></param>
        private void SetupActionPrompt(GameObject triggerGo, string currentColliderName)
        {
            Canvas canvas = triggerGo.GetComponentInChildren<Canvas>(true);
            if (canvas)
            {
                RectTransform rect = canvas.GetComponent<RectTransform>();
                // rect.gameObject.transform.SetParent(_doorGameObject.transform);
                float promptz = doorPivot==DoorPivot.Left ? _doorWidth / 2 : -_doorWidth / 2;
                // float promptz = -_doorWidth / 2;
                float promptx = currentColliderName == "OpenOutwardTrigger" ? -promptDepthOffset : promptDepthOffset;
                rect.localPosition = new Vector3(promptx, promptHeight, promptz);
                Debug.Log($"Configured Action Prompt. ({triggerGo.name}");
            }
            else
            {
                Debug.LogError($"Unable to configure door prompt! Can't find canvas. Check the prefab! ({triggerGo.name}");
            }
        }
    }
}