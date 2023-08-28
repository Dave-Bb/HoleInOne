using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class FlagPolePlacer : MonoBehaviour
    {
        [SerializeField] private EdgeCollider2D edgeCollider;
        public EdgeCollider2D EdgeCollider => edgeCollider;

        public void UpdatePositionAndColliders((List<Vector2>, Vector2) details)
        {
            SetColliderPoints(details.Item1);
            SetPosition(details.Item2);
        }
        
        private void SetPosition(Vector2 flatPos)
        {
            transform.position = flatPos;
        }

        private void SetColliderPoints(List<Vector2> points)
        {
            if (edgeCollider == null)
            {
                return;
            }

            edgeCollider.SetPoints(points);
        }
    }
}