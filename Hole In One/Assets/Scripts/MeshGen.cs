using UnityEngine;
using System.Collections.Generic;
using System;

public class MeshGen : MonoBehaviour
{
    public Transform ground;
    
    public float ScalingFactor = 1f;

    // the length of segment (world space)
    public float SegmentLength = 5;

    // the segment resolution (number of horizontal points)
    public int SegmentResolution = 32;

    // the size of meshes in the pool
    public int MeshCount = 4;

    // the maximum number of visible meshes. Should be lower or equal than MeshCount
    public int VisibleMeshes = 4;

    public float baseHeight = 5f;

    // the prefab including MeshFilter and MeshRenderer
    public MeshFilter SegmentPrefab;

    // helper array to generate new segment without further allocations
    private Vector3[] _vertexArray;

    // the pool of free mesh filters
    private List<MeshFilter> _freeMeshFilters = new List<MeshFilter>();

    // the list of used segments
    private List<Segment> _usedSegments = new List<Segment>();

    public bool InitialMeshGenFinished { get; private set; }

    private List<Action> onCompleteActions = new List<Action>();

    private Vector2 holePosition;

    private Segment holeSegment;

    public Material groundMaterial;
    
    private float holeWorldPositionX;
    public float holeWidth = 5.0f;  // you can adjust this width for your requirement
    public float holeDepth = 3.0f;
    
    // Larger constant values (e.g., 2.5f or 3f) for higher peaks.
    public float amplitude = 2.5f;

    // Smaller values (e.g., 0.5f or 0.3f) for wider peaks.
    public float frequency1 = 0.5f;
    public float frequency2 = 0.4f;
    
    public GameObject marker;
    
    void Awake()
    {
        // Create vertex array helper
        _vertexArray = new Vector3[SegmentResolution * 2];

        // Build triangles array. For all meshes this array always will
        // look the same, so I am generating it once 
        int iterations = _vertexArray.Length / 2 - 1;
        var triangles = new int[(_vertexArray.Length - 2) * 3];

        for (int i = 0; i < iterations; ++i)
        {
            int i2 = i * 6;
            int i3 = i * 2;

            triangles[i2] = i3 + 2;
            triangles[i2 + 1] = i3 + 1;
            triangles[i2 + 2] = i3 + 0;

            triangles[i2 + 3] = i3 + 2;
            triangles[i2 + 4] = i3 + 3;
            triangles[i2 + 5] = i3 + 1;
        }

        // Create game objects (with MeshFilter) instances.
        // Assign vertices, triangles, deactivate and add to the pool.
        for (int i = 0; i < MeshCount; ++i)
        {
            MeshFilter filter = Instantiate(SegmentPrefab);
            filter.gameObject.name = "Segment " + i;
            filter.transform.localScale *= ScalingFactor;

            Mesh mesh = filter.mesh;
            mesh.Clear();

            mesh.vertices = _vertexArray;
            mesh.triangles = triangles;

            filter.gameObject.SetActive(false);
            _freeMeshFilters.Add(filter);
        }

        UpdateMesheshes();

        InitialMeshGenFinished = true;

        if (onCompleteActions.Count > 0)
        {
            foreach (var onCompleteAction in onCompleteActions)
            {
                onCompleteAction?.Invoke();
            }

            onCompleteActions.Clear();
        }
    }

    void Update()
    {
        UpdateMesheshes();
    }

    private void UpdateMesheshes()
    {
        Vector3 worldCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        int currentSegment = (int)(worldCenter.x / SegmentLength);

        // test for invisibility
        for (int i = 0; i < _usedSegments.Count;)
        {
            int segmentIndex = _usedSegments[i].Index;
            if (!IsSegmentInSight(segmentIndex))
            {
                EnsureSegmentNotVisible(segmentIndex);
            }
            else
            {
                // EnsureSegmentNotVisible will remove the segment from the list
                // that's why I increase the counter based on that condition
                ++i;
            }
        }

        // test for visibility
        for (int i = currentSegment - VisibleMeshes / 2; i < currentSegment + VisibleMeshes / 2; ++i)
        {
            if (IsSegmentInSight(i))
            {
                EnsureSegmentVisible(i);
            }
        }

        var currentGroundPos = ground.transform.localPosition;
        currentGroundPos.x = Camera.main.transform.position.x * 0.5f;
        ground.transform.localPosition = currentGroundPos;
    }

    public void FireInitialGenFinished(Action onComplete)
    {
        if (InitialMeshGenFinished)
        {
            onComplete?.Invoke();
            return;
        }

        onCompleteActions.Add(onComplete);
    }


    private void EnsureSegmentVisible(int index)
    {
        if (!IsSegmentVisible(index))
        {
            // make visible
            int meshIndex = _freeMeshFilters.Count - 1;
            MeshFilter filter = _freeMeshFilters[meshIndex];
            _freeMeshFilters.RemoveAt(meshIndex);

            GenerateSegment(index, filter); // Adjusted call here

            filter.transform.position = new Vector3(index * SegmentLength, 0, 0);
            filter.gameObject.SetActive(true);

            EdgeCollider2D edgeCollider = filter.gameObject.GetComponent<EdgeCollider2D>();

            // register as segment
            var segment = new Segment();
            segment.Index = index;
            segment.MeshFilter = filter;
            segment.EdgeCollider = edgeCollider;

            _usedSegments.Add(segment);
        }
    }

    private void EnsureSegmentNotVisible(int index)
    {
        if (!IsSegmentVisible(index))
        {
            return;
        }
        
        int listIndex = SegmentCurrentlyVisibleListIndex(index);
        Segment segment = _usedSegments[listIndex];
        _usedSegments.RemoveAt(listIndex);

        MeshFilter filter = segment.MeshFilter;
        filter.gameObject.SetActive(false);
        _freeMeshFilters.Add(filter);
    }

    private bool IsSegmentVisible(int index)
    {
        return SegmentCurrentlyVisibleListIndex(index) != -1;
    }

    private int SegmentCurrentlyVisibleListIndex(int index)
    {
        for (int i = 0; i < _usedSegments.Count; ++i)
        {
            if (_usedSegments[i].Index == index)
            {
                return i;
            }
        }

        return -1;
    }
    

    // Gets the height of terrain at current position.
    public float GetHeight(float position)
    {
        // Calculate what the terrain height would be without the hole.
        float baseTerrainHeight = amplitude * (Mathf.Sin(position * frequency1) + Mathf.Sin(position * frequency2 + Mathf.PI / 3) + 2) / 4 + baseHeight;

        float distanceFromHole = Mathf.Abs(position - holeWorldPositionX);

        if (distanceFromHole < holeWidth / 2)
        {
            // Adjust holeDepth calculation to give it a rounded bottom
            float normalizedDistance = distanceFromHole / (holeWidth / 2);
            float holeDepthRelativeToTerrain = holeDepth * Mathf.SmoothStep(1, 0, normalizedDistance);

            // Return the terrain height adjusted by the hole's depth.
            return baseTerrainHeight - holeDepthRelativeToTerrain;
        }
        
        return baseTerrainHeight;
    }
    
    public void SetHoleEdgeCollider(EdgeCollider2D edgeCollider, float holePositionX, float samplingResolution = 0.01f)
    {
        // Determine the start and end positions for sampling the hole
        float startPosition = holePositionX - holeWidth / 2;
        float endPosition = holePositionX + holeWidth / 2;

        // Use a List to dynamically collect the points
        List<Vector2> colliderPoints = new List<Vector2>();

        // Sample the height along the width of the hole
        for (float x = startPosition; x <= endPosition; x += samplingResolution)
        {
            float y = GetHeight(x);
            colliderPoints.Add(new Vector2(x, y));
        }

        // Set the points for the EdgeCollider2D
        edgeCollider.points = colliderPoints.ToArray();
    }
 
    // Call this method to set the hole position based on world x position
    public void SetHolePosition(float worldPosX)
    {
        holeWorldPositionX = worldPosX;

        // Refresh surrounding segments to immediately reflect the hole
        int affectedSegmentStart = Mathf.FloorToInt((worldPosX - holeWidth / 2) / SegmentLength);
        int affectedSegmentEnd = Mathf.CeilToInt((worldPosX + holeWidth / 2) / SegmentLength);

        for(int i = affectedSegmentStart; i <= affectedSegmentEnd; i++)
        {
            int segmentIndex = SegmentCurrentlyVisibleListIndex(i);
            if (segmentIndex == -1)
            {
                continue;
            }
            
            Segment segment = _usedSegments[segmentIndex];
            GenerateSegment(segment.Index, segment.MeshFilter);
        }
    }

    // This function generates a mesh segment.
    // Index is a segment index (starting with 0).
    // Mesh is a mesh that this segment should be written to.
    // Modify the method signature
    public void GenerateSegment(int index, MeshFilter filter)
    {
        float startPosition = index * SegmentLength;
        float step = SegmentLength / (SegmentResolution - 1);

        Mesh mesh = filter.mesh;

        Vector2[] colliderPoints = new Vector2[SegmentResolution]; // Points for EdgeCollider2D
    
        for (int i = 0; i < SegmentResolution; ++i)
        {
            float xPos = step * i;
        
            // top vertex
            float yPosTop = GetHeight(startPosition + xPos);
            yPosTop += baseHeight;
            _vertexArray[i * 2] = new Vector3(xPos, yPosTop, 0);

            colliderPoints[i] = new Vector2(xPos, yPosTop); // Store top vertex for EdgeCollider2D

            // bottom vertex always at y=0
            _vertexArray[i * 2 + 1] = new Vector3(xPos, 0, 0);
        }

        mesh.vertices = _vertexArray;
        
        // Attach or update the EdgeCollider2D
        EdgeCollider2D edgeCollider = filter.gameObject.GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
        {
            edgeCollider = filter.gameObject.AddComponent<EdgeCollider2D>();
        }
        edgeCollider.points = colliderPoints;

        // need to recalculate bounds, because mesh can disappear too early
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    
    public void UpdateColor(Color color)
    {
        groundMaterial.color = color;
    }
    
    public Vector3 CheckForPosition(float targetPosRatio)
    {
        var xPosition = Screen.width * targetPosRatio;
        int groundLayer = LayerMask.GetMask("Ground");
        // Convert screen position to world position
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(xPosition, Screen.height, Camera.main.nearClipPlane));
        
        // Create a ray going downwards from the top of the screen at the given X position
        Ray ray = new Ray(worldPosition, Vector3.down);
        
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, groundLayer);

        // If it hits something on the "Ground" layer
        if (hit.collider != null)
        {
           
            if (marker != null)
            {
                marker.transform.position = hit.point;
            }
            
            return hit.point;
        }

        return Vector2.zero;
    }
    
    private bool IsSegmentInSight(int index)
    {
        Vector3 worldLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 worldRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        
        // check left and right segment side
        float x1 = index * SegmentLength;
        float x2 = x1 + SegmentLength;
        
        return x1 <= worldRight.x && x2 >= worldLeft.x;
    }
    
    public struct Segment
    {
        public int Index { get; set; }
        public MeshFilter MeshFilter { get; set; }
        public EdgeCollider2D EdgeCollider { get; set; }
    }
}
