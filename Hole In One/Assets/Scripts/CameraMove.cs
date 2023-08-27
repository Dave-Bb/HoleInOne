using System;
using System.Collections;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [SerializeField] private MeshGen meshGen;
    [SerializeField] private float lerpDuration = 1f;
    [SerializeField] private bool AutoScroll;
    [SerializeField] private float autoScrollSpeed;

    private const float Buffer = 0.1f;

    public Action CameraMovementEnded;

    private void Awake()
    {
        var currentCameraPos = transform.position;
        currentCameraPos.x += Buffer;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceToNextHole();
        }

        if (AutoScroll)
        {
            var pos = transform.position;
            pos.x += autoScrollSpeed * Time.deltaTime;
            transform.position = pos;
        }
    }

    private bool isScroling;

    public void HoleInOne()
    {
        if (isScroling)
        {
            return;
        }
        
        StartCoroutine(WaitAndAdvance());
    }
    
    private IEnumerator WaitAndAdvance()
    {
        isScroling = true;
        yield return new WaitForSeconds(1.0f); // Wait for 1 second

        AdvanceToNextHole();
        isScroling = false;
    }

    public float holeLenth = 50;
    
    public void AdvanceToNextHole()
    {
        if (meshGen == null)
        {
            Debug.LogError("Cant move to next hole, no mesh gen found");
            return;
        }

        var targetPosition = transform.position;
        //targetPosition.x += (meshGen.SegmentLength * (meshGen.VisibleMeshes * 0.5f));
        targetPosition.x += holeLenth;

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
