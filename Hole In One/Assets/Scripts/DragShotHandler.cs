using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class DragShotHandler : MonoBehaviour
    {
        [SerializeField] private float maxDragDistance = 5f; // Max drag distance
        
        private LineRenderer lineRenderer;
        private bool isDragging = false;
        private Vector2 dragStartPos;
        private Vector2 dragEndPos;

        public Action DragStarted;
        public Action<Vector2> DragEnded;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2; // We need two points for a line
            HideLine();
        }
        
        private void Update()
        {
            // Detect input (whether mouse or touch)
            if (Input.GetMouseButtonDown(0))
            {
                StartDrag(GetCurrentInputPosition());
            }
            if (isDragging)
            {
                UpdateDrag(GetCurrentInputPosition());
            }
            if (Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
        }
        
        private void StartDrag(Vector2 startPos)
        {
            isDragging = true;
            dragStartPos = Camera.main.ScreenToWorldPoint(startPos);
            lineRenderer.SetPosition(0, new Vector3(dragStartPos.x, dragStartPos.y, -0.1f)); // Line starts from initial click
            lineRenderer.SetPosition(1, new Vector3(dragStartPos.x, dragStartPos.y, -0.1f));
            ShowLine();
            
            DragStarted?.Invoke();
        }
        
        private void UpdateDrag(Vector2 currentPos)
        {
            dragEndPos = Camera.main.ScreenToWorldPoint(currentPos);
        
            // Clamp the drag distance
            if (Vector2.Distance(dragStartPos, dragEndPos) > maxDragDistance)
            {
                dragEndPos = dragStartPos + (dragEndPos - dragStartPos).normalized * maxDragDistance;
            }
        
            lineRenderer.SetPosition(1, new Vector3(dragEndPos.x, dragEndPos.y, -0.1f));
        }
        
        private Vector2 GetCurrentInputPosition()
        {
            // If on mobile platform and touch is detected, use touch position
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (Input.touchCount > 0)
                {
                    return Input.GetTouch(0).position;
                }
            }
        
            // Otherwise, use mouse position
            return Input.mousePosition;
        }
        
        private void EndDrag()
        {
            isDragging = false;
            var delta = dragStartPos - dragEndPos;
            
            
            HideLine();
            
            if (delta.magnitude < 0.1f)
            {
                return;
            }
            
            DragEnded?.Invoke(delta);
        }
        
        private void ShowLine()
        {
            lineRenderer.enabled = true;
        }

        private void HideLine()
        {
            lineRenderer.enabled = false;
        }
    }
}