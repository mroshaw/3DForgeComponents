using UnityEngine;
using UnityEngine.UI;

namespace DaftAppleGames.UI
{
    public class ActionPrompt : MonoBehaviour
    {
        [Header("Config")]
        public bool displayPrompt;
        public GameObject promptGameObject;

        [Header("UI Config")]
        public string promptText;
        public Text promptTextObject;
        public float promptHeight;
        
        /// <summary>
        /// Initialise the prompt
        /// </summary>
        public virtual void Start()
        {
            // Set the prompt text
            promptTextObject.text = promptText;
            
            // Set the prompt height
            Vector3 promptPosition = promptGameObject.transform.localPosition;
            promptPosition.y = promptHeight;
            promptGameObject.transform.localPosition = promptPosition;
        }
        
        /// <summary>
        /// Show prompt, if configured to do so
        /// </summary>
        public virtual void ShowPrompt()
        {
            if (displayPrompt)
            {
                promptGameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hide prompt
        /// </summary>
        public virtual void HidePrompt()
        {
            promptGameObject.SetActive(false);
        }
    }
}