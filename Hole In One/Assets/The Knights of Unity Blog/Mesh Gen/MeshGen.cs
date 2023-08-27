using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class MeshGen : MonoBehaviour
{
    public float ScalingFactor = 1f;

    // the length of segment (world space)
    public float SegmentLength = 5;

    public float height;

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

        // Create colors array. For now make it all white.
        /*var colors = new Color32[_vertexArray.Length];
        for (int i = 0; i < colors.Length; ++i)
        {
            colors[i] = new Color32(255, 255, 255, 255);
        }*/

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

    private int visibleSegments;

    private void EnsureSegmentVisible(int index)
    {
        if (!IsSegmentVisible(index))
        {
            // make visible
            int meshIndex = _freeMeshFilters.Count - 1;
            MeshFilter filter = _freeMeshFilters[meshIndex];
            _freeMeshFilters.RemoveAt(meshIndex);

            Mesh mesh = filter.mesh;

            GenerateSegment(index, ref mesh, ref filter);
            
            filter.transform.position = new Vector3(index * SegmentLength, 0, 0);

            filter.gameObject.SetActive(true);

            EdgeCollider2D edgeCollider = filter.gameObject.GetComponent<EdgeCollider2D>();

            // register as segment
            var segment = new Segment();
            segment.Index = index;
            segment.MeshFilter = filter;
            segment.EdgeCollider = edgeCollider;
            segment.GameObject = filter.gameObject;

            _usedSegments.Add(segment);

            visibleSegments += 1;


        }
    }

    public int flagIndex = 45;
    
    
    public (List<Vector2>, Vector3) UpdateHolePosition(float xEdgeRatio)
    {
        var hitDetails = RaycastForSegmentFromTop(xEdgeRatio);
        Segment? segment = hitDetails.Item1;
        Vector2 hitPoint = hitDetails.Item2;

        if(segment == null)
        {
            Debug.LogWarning("No segment found using raycast.");
            return (new List<Vector2>(), Vector3.zero);
        }
        else
        {
           Debug.Log("egment found "+segment.GetValueOrDefault().GameObject.name);
        }

        MeshFilter meshFilter = segment.Value.MeshFilter;
        Vector3 segmentPosition = meshFilter.transform.position;

        var vertices = meshFilter.sharedMesh.vertices;

        float closestDistance = Vector3.Distance(vertices[^1], hitPoint);
        
       var checkLength = segment.GetValueOrDefault().EdgeCollider.points.Length -1;
       for (int i = checkLength; i > 0; i--)
        {
            float distance = Vector2.Distance(segment.GetValueOrDefault().EdgeCollider.points[i], hitPoint);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                flagIndex = i / 2; // Because you're iterating by 2
            }
        }
        if (marker != null)
        {
            marker.transform.position = hitPoint;
        }
        int range = 6; // Number of vertices to modify on either side of the flag's index
        if (flagIndex % 2 != 0)
        {
            flagIndex += 1;
        }

        while ((flagIndex * 2) + range > vertices.Length)
        {
            flagIndex -= 1;
            Debug.Log("Flag index was too high");
        }
        
        int startingIndex = (flagIndex * 2) - range;
        int endingIndex = (flagIndex * 2) + range;

      

        float radius = 3f; // Adjust for desired hole depth and shape

        float lowestY = Screen.height;
        List<Vector2> holeColliderPoints = new List<Vector2>();
        holeColliderPoints.Add(vertices[startingIndex - 2]);
        for (int i = startingIndex; i <= endingIndex; i += 2)
        {
            // Calculate the offset x value from the center of the semicircle
            float xOffset = (i - flagIndex * 2) * 0.5f; // Assuming a spacing of 0.5 units between vertices
            //float yValue = Mathf.Sqrt(radius * radius - xOffset * xOffset) - radius; // The y value based on semicircle formula
            float yValue = -Mathf.Sqrt(radius * radius - xOffset * xOffset) - radius;
          
            Vector3 updatedVertex = vertices[i];
            Vector3 holeColliderPoint = vertices[i];
            holeColliderPoint.y += yValue * 0.1f;
            holeColliderPoints.Add(holeColliderPoint);
            updatedVertex.y += yValue; // Adjusting the terrain vertex
            if (updatedVertex.y < lowestY)
            {
                lowestY = updatedVertex.y;
            }
            vertices[i] = updatedVertex;
        }
        
        holeColliderPoints.Add(vertices[endingIndex + 2]);
        
        meshFilter.sharedMesh.vertices = vertices;
        meshFilter.sharedMesh.RecalculateBounds();
        meshFilter.sharedMesh.RecalculateNormals(); // Also recalculate normals for proper lighting
        

        // Create a list to store the top edge vertices
        List<Vector2> topEdgePoints = new List<Vector2>();

        // Assuming the top vertices are the even indices, e.g., 0, 2, 4,...
        for (int i = 0; i < vertices.Length; i += 2)
        {
            // Convert the 3D vertex to 2D point
            topEdgePoints.Add(new Vector2(vertices[i].x, vertices[i].y));
        }

        // Get or add the EdgeCollider2D component
        EdgeCollider2D edgeCollider = meshFilter.gameObject.GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
        {
            edgeCollider = meshFilter.gameObject.AddComponent<EdgeCollider2D>();
        }

        // Assign the updated points to the EdgeCollider2D
        edgeCollider.points = topEdgePoints.ToArray();

        return (holeColliderPoints, segmentPosition);
    }
    
    Vector2 GetVertexInWorldSpace(Vector2 localVertex, Transform objectTransform)
    {
        return objectTransform.TransformPoint(localVertex);
    }
    
    public (Segment?, Vector2) RaycastForSegmentFromTop(float xPosition)
    {
        int groundLayer = LayerMask.GetMask("Ground");
        
        // Convert screen position to world position
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * xPosition, Screen.height, Camera.main.nearClipPlane));
    
        // Create a ray going downwards from the top of the screen at the given X position
        Ray ray = new Ray(worldPosition, Vector3.down);
    
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, groundLayer);

        // If it hits something on the "Ground" layer
        if (hit.collider != null)
        {
            foreach (var usedSegment in _usedSegments)
            {
                if (hit.collider.gameObject == usedSegment.GameObject)
                {
                    return (usedSegment, hit.point);
                }
            }
        }

        Debug.Log("Ray did not hit any ground object.");
        return (null, Vector2.zero);
    }
    

    public Vector2 GetFlagStartPosition()
    {
        var furthestRight = GetFurthestRightSegmentGameObject();
        var targetX = furthestRight.transform.position.x;

        var edgeCollider = furthestRight.GetComponent<EdgeCollider2D>();
        var maxPoints = edgeCollider.points.Length;
        var randomIndex = flagIndex;
        var edgePoint = edgeCollider.points[randomIndex];
        Vector2 targetPos = new Vector2(targetX + edgePoint.x, edgePoint.y);
        return targetPos;
    }

    private void EnsureSegmentNotVisible(int index)
    {
        if (IsSegmentVisible(index))
        {
            int listIndex = SegmentCurrentlyVisibleListIndex(index);
            Segment segment = _usedSegments[listIndex];
            _usedSegments.RemoveAt(listIndex);

            MeshFilter filter = segment.MeshFilter;
            filter.gameObject.SetActive(false);


            _freeMeshFilters.Add(filter);

            visibleSegments -= 1;
        }
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
    

    // Larger constant values (e.g., 2.5f or 3f) for higher peaks.
    public float amplitude = 2.5f;

    // Smaller values (e.g., 0.5f or 0.3f) for wider peaks.
    public float frequency1 = 0.5f;
    public float frequency2 = 0.4f;

    // Gets the height of terrain at current position.
    public float GetHeight(float position)
    {


        return amplitude * (Mathf.Sin(position * frequency1) + Mathf.Sin(position * frequency2 + Mathf.PI / 3) + 2) / 4;
    }
    public GameObject GetFurthestLeftSegment()
    {
        GameObject furthestLeft = null;
        float furhestLeftX = 0f;
        foreach (var usedSegment in _usedSegments)
        {
            if (!usedSegment.GameObject.activeInHierarchy)
            {
                continue;
            }

            if (furthestLeft == null)
            {
                furthestLeft = usedSegment.GameObject;
                furhestLeftX = usedSegment.GameObject.transform.position.x;
                continue;
            }

            if (usedSegment.GameObject.transform.position.x < furhestLeftX)
            {
                furthestLeft = usedSegment.GameObject;
                furhestLeftX = usedSegment.GameObject.transform.position.x;
            }
        }

        return furthestLeft;
    }

    public GameObject GetFurthestRightSegmentGameObject()
    {
        GameObject furthestRight = null;
        float furhestRightX = 0f;
        foreach (var usedSegment in _usedSegments)
        {
            if (!usedSegment.GameObject.activeInHierarchy)
            {
                continue;
            }

            if (furthestRight == null)
            {
                furthestRight = usedSegment.GameObject;
                furhestRightX = usedSegment.GameObject.transform.position.x;
                continue;
            }

            if (usedSegment.GameObject.transform.position.x > furhestRightX)
            {
                furthestRight = usedSegment.GameObject;
                furhestRightX = usedSegment.GameObject.transform.position.x;
            }
        }

        return furthestRight;
    }
    
    public Segment GetFurthestRightSegment()
    {
        Segment furthestRight = _usedSegments[0];
        float furhestRightX = 0f;
        foreach (var usedSegment in _usedSegments)
        {
            if (!usedSegment.GameObject.activeInHierarchy)
            {
                continue;
            }
            

            if (usedSegment.GameObject.transform.position.x > furhestRightX)
            {
                furthestRight = usedSegment;
                furhestRightX = usedSegment.GameObject.transform.position.x;
            }
        }

        return furthestRight;
    }


    public float holeDepth = 0.5f;
        public void CreateHole(Vector2 position)
        {
            GameObject furthestRight = GetFurthestRightSegmentGameObject();
            EdgeCollider2D edgeCollider = furthestRight.GetComponent<EdgeCollider2D>();

            if (edgeCollider != null)
            {
                Vector2[] points = edgeCollider.points;

                // Locate closest point to the desired position.
                int closestIndex = 0;
                float minDistance = float.MaxValue;

                for (int i = 0; i < points.Length; i++)
                {
                    float distance =
                        (new Vector2(furthestRight.transform.position.x + points[i].x, points[i].y) - position)
                        .sqrMagnitude;
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestIndex = i;
                    }
                }

                // Modify terrain to create indentation.
                if (closestIndex > 0 && closestIndex < points.Length - 1)
                {
                    float averageY = (points[closestIndex - 1].y + points[closestIndex + 1].y) / 2f;
                    points[closestIndex] =
                        new Vector2(points[closestIndex].x,
                            averageY - holeDepth); // the 0.5f value can be adjusted based on how deep you want the hole
                    edgeCollider.points = points;
                }
            }
        }
    

    // This function generates a mesh segment.
    // Index is a segment index (starting with 0).
    // Mesh is a mesh that this segment should be written to.
    public void GenerateSegment(int index, ref Mesh mesh, ref MeshFilter filter)
    {
        float startPosition = index * SegmentLength;
        float step = SegmentLength / (SegmentResolution - 1);

        Vector2[] colliderPoints = new Vector2[SegmentResolution]; // Points for EdgeCollider2D
        
        for (int i = 0; i < SegmentResolution; ++i)
        {
            float xPos = step * i;

            /*// top vertex
            float yPosTop = GetHeight(startPosition + xPos);
            _vertexArray[i * 2] = new Vector3(xPos, yPosTop, 0);*/
            
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

    public GameObject marker;
    
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
                //marker.transform.position = hit.point;
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

        public GameObject GameObject;
    }
}
