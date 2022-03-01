using DaftAppleGames.UI;
using UnityEngine;

namespace DaftAppleGames.Buildings
{
    public class DoorActionPrompt : ActionPrompt
    {
        [Header("UI Config")]
        public float depthOffset;

        public override void Start()
        {
            // Set the prompt height
            Vector3 promptPosition = promptGameObject.transform.localPosition;
            promptPosition.z = depthOffset;
            promptGameObject.transform.localPosition = promptPosition;
        }
    }
}