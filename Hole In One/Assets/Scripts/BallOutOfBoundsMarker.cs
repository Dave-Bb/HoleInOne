using UnityEngine;

public class BallOutOfBoundsMarker : MonoBehaviour
{
    [SerializeField] private RectTransform marker;
    [SerializeField] private GameObject ball;
    
    private void Awake()
    {
        HideMarker();
    }

    private void Update()
    {
        if (Camera.current != null)
        {
            var cameraScreenPos = Camera.current.WorldToScreenPoint(ball.transform.position);
            if (cameraScreenPos.y < Screen.height && cameraScreenPos.x < Screen.width)
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
        bool onXEdge = false;
        bool onYEdge = false;

        if (viewportPos.x < 0f)
        {
            viewportPos.x = 0f;
            onXEdge = true;
        }
        if (viewportPos.x > 1f)
        {
            viewportPos.x = 1f;
            onXEdge = true;
        }
        if (viewportPos.y < 0f)
        {
            viewportPos.y = 0f;
            onYEdge = true;
        }
        if (viewportPos.y > 1f)
        {
            viewportPos.y = 1f;
            onYEdge = true;
        }

        // Convert the viewport position to a position on the screen/canvas.
        Vector2 screenPos = new Vector2(viewportPos.x * Camera.current.pixelWidth, viewportPos.y * Camera.current.pixelHeight);

        // Adjust position to the edges if required.
        if (viewportPos.x >= 1f)
        {
            screenPos.x = Camera.current.pixelWidth - 50f;
        }
        if (viewportPos.y >= 1f)
        {
            screenPos.y = Camera.current.pixelHeight - 50f;
        }

        // Update marker position.
        marker.position = screenPos;

        // Determine marker rotation based on which edge it's on
        if (onXEdge && !onYEdge)
        {
            // Marker should rotate 90 degrees to the right when tracking along the Y-axis.
            marker.rotation = Quaternion.AngleAxis(-90, Vector3.forward);
        }
        else if (!onXEdge && onYEdge)
        {
            // Marker should point "up" when tracking along the X-axis.
            marker.SetPositionAndRotation(screenPos, Quaternion.identity);
            
        }
        else
        {
            // For other cases (corners or on-screen), make the marker face the ball.
            Vector2 directionToBall = ball.transform.position - marker.position;
            float angle = Mathf.Atan2(directionToBall.y, directionToBall.x) * Mathf.Rad2Deg;
            marker.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    
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