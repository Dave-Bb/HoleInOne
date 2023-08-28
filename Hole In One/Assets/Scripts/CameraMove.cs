using System;
using System.Collections;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    private const float Buffer = 0.1f;
    
    [SerializeField] private MeshGen meshGen;
    [SerializeField] private float lerpDuration = 1f;
    [SerializeField] private bool AutoScroll;
    [SerializeField] private float autoScrollSpeed;
    
    private bool isScroling;
    
    public Action CameraMovementEnded;

    private void Awake()
    {
        var currentCameraPos = transform.position;
        currentCameraPos.x += Buffer;
    }

    private void Update()
    {
        if (!AutoScroll)
        {
            return;
        }
        
        var pos = transform.position;
        pos.x += autoScrollSpeed * Time.deltaTime;
        transform.position = pos;
    }

    public void HoleInOne(float holeDistance)
    {
        if (isScroling)
        {
            return;
        }
        
        StartCoroutine(WaitAndAdvance(holeDistance));
    }
    
    private IEnumerator WaitAndAdvance(float holeDistance)
    {
        isScroling = true;
        yield return new WaitForSeconds(1.0f); // Wait for 1 second

        AdvanceToNextHole(holeDistance);
        isScroling = false;
    }
    
    private void AdvanceToNextHole(float holeDistance)
    {
        if (meshGen == null)
        {
            Debug.LogError("Cant move to next hole, no mesh gen found");
            return;
        }

        var targetPosition = transform.position;
        //targetPosition.x += (meshGen.SegmentLength * (meshGen.VisibleMeshes * 0.5f));
        targetPosition.x += holeDistance;

        StartCoroutine(SmoothMove(transform.position, targetPosition, lerpDuration));
    }

    private IEnumerator SmoothMove(Vector3 start, Vector3 end, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(start, end, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = end; // Ensure the final position is set exactly to the end position.
        CameraMovementEnded?.Invoke();
    }
}
