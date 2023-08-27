using UnityEngine;

public class BallOutOfBoundsMarker : MonoBehaviour
{
    [SerializeField] private RectTransform marker;

    [SerializeField] private GameObject ball;

    [SerializeField] private float offsetFromTop = 100f;
    
    

    private void Awake()
    {
        HideMarker();
    }

    private void Update()
    {
        if (Camera.current != null)
        {
            if (Camera.current.WorldToScreenPoint(ball.transform.position).y < Screen.height)
            {
                HideMarker();
            }
            else
            {
                UpdateMarkerPosition();
            }
        }
       
    }

    private void UpdateMarkerPosition()
    {
        if (!marker.gameObject.activeInHierarchy)
        {
            marker.gameObject.SetActive(true);
        }
        
        // Convert the object's position from world space to viewport space.
        Vector2 viewportPos = Camera.current.WorldToViewportPoint(ball.transform.position);

        // Determine on which edge of the screen to place the tracking object.
        if (viewportPos.x < 0f) viewportPos.x = 0f;
        if (viewportPos.x > 1f) viewportPos.x = 1f;
        if (viewportPos.y < 0f) viewportPos.y = 0f;
        if (viewportPos.y > 1f) viewportPos.y = 1f;

        // Convert the viewport position to a position on the screen/canvas.
        Vector2 screenPos = new Vector2(viewportPos.x * Camera.current.pixelWidth, viewportPos.y * Camera.current.pixelHeight);
        screenPos.y = Camera.current.pixelHeight - 50f;
        marker.SetPositionAndRotation(screenPos, Quaternion.identity);
    }

    private void HideMarker()
    {
        if (marker == null)
        {
            return;
        }
        if (marker.gameObject.activeInHierarchy)
        {
            marker.gameObject.SetActive(false);
        }
    }
}