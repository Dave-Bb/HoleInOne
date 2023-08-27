using UnityEngine;
using System.Collections;

public class ColorShifter : MonoBehaviour
{
    [Tooltip("The starting color.")]
    public Color initialColor;

    public Color CurrentColor { get; private set; }

    private float currentHue;
    private float targetHue;

    private void Start()
    {
        // Set the initial color
        CurrentColor = initialColor;

        // Extract its hue
        Color.RGBToHSV(CurrentColor, out currentHue, out _, out _);
    }

    // Public method to start the hue shift
    public void StartHueShift(float duration)
    {
        // Randomly choose a target hue
        targetHue = Random.Range(0f, 1f);
        
        StartCoroutine(ShiftHueCoroutine(duration));
    }
    
    

    private IEnumerator ShiftHueCoroutine(float duration)
    {
        yield return new WaitForSeconds(1.0f);
        
       

        float timePassed = 0f;
        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            float lerpFactor = timePassed / duration;

            // Lerp the hue value
            float newHue = Mathf.Lerp(currentHue, targetHue, lerpFactor);
            CurrentColor = Color.HSVToRGB(newHue, 1, 1); // Assuming full saturation and value

            yield return null;
        }

        // Ensure color is set to the target hue at the end of the lerp
        CurrentColor = Color.HSVToRGB(targetHue, 1, 1);
        currentHue = targetHue;
    }
}
