using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineDrawer : MonoBehaviour
    {
        private LineRenderer lineRenderer;

        public void StartLine(Vector2 position)
        {
            lineRenderer.SetPosition(0, position); // Line starts from initial click
            lineRenderer.SetPosition(1, position);
        }
    }
}