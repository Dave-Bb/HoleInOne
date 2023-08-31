using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Controllers
{
    public class ColorController : MonoBehaviour
    {
        [SerializeField] private Color startColor = Color.green; // Starting color
        
        [SerializeField] private float hueChangeSpeed = 0.05f; // Controls how quickly we move to the next hue
        
        private Color currentColor;
        private Color targetColor;
        private float hue, saturation, value;
        
        public Color CurrentColor => currentColor;
        
        
        void Awake()
        {
            Color.RGBToHSV(startColor, out hue, out saturation, out value);
            currentColor = startColor;
        }

        public void SetNextColor()
        {
            //currentColor = GetNextColor();
            StartCoroutine(TransitionToNextColor());
        }

        public Color GetNextColor()
        {
            // Update hue
            hue += hueChangeSpeed;
            if (hue > 1) hue -= 1; // Keep hue in [0,1] range

            // Convert HSV back to RGB
            currentColor = Color.HSVToRGB(hue, saturation, value);

            return currentColor;
        }
        
        private IEnumerator TransitionToNextColor()
        {
            yield return new WaitForSeconds(1);
            
            Color startColor = currentColor;

            // Update hue
            hue += hueChangeSpeed;
            if (hue > 1) hue -= 1; // Keep hue in [0,1] range

            Color endColor = Color.HSVToRGB(hue, saturation, value);

            float duration = 1f; // Transition over 1 second
            float timePassed = 0f;

            while (timePassed < duration)
            {
                timePassed += Time.deltaTime;
                float lerpFactor = timePassed / duration;

                currentColor = Color.Lerp(startColor, endColor, lerpFactor);
                
                yield return null;
            }

            // Ensure the color is set to the target at the end of the transition
            currentColor = endColor;
        }

        // Optional: If you want to jump to a new target hue
        public void SetNewTargetHue()
        {
            hue += Random.Range(0.1f, 0.3f); // Change this range for bigger/smaller jumps
            if (hue > 1) hue -= 1; // Keep hue in [0,1] range
        }
    }
}