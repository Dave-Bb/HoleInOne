using System.Collections;
using System.IO;
using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    [Header("Settings")]
    public string savePath = "Screenshots/";
    
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(TakeScreenshot());
        }
    }

    private IEnumerator TakeScreenshot()
    {
        yield return new WaitForEndOfFrame();

        // Create a texture and read pixels from the screen
        Texture2D screenshotTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshotTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshotTexture.Apply();

        // Calculate target width and height to maintain aspect ratio
        float targetWidth = Screen.width;
        float targetHeight = Screen.width / mainCamera.aspect;

        // Crop the texture to maintain aspect ratio
        int yOffset = Mathf.FloorToInt((Screen.height - targetHeight) / 2);
        Color[] pixels = screenshotTexture.GetPixels(0, yOffset, (int)targetWidth, (int)targetHeight);

        Texture2D croppedTexture = new Texture2D((int)targetWidth, (int)targetHeight);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        // Save the cropped texture as an image
        byte[] bytes = croppedTexture.EncodeToPNG();
        string filePath = Path.Combine(Application.dataPath, savePath, "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllBytes(filePath, bytes);

        Debug.Log("Screenshot saved to: " + filePath);
    }
}