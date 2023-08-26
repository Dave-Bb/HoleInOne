using UnityEngine;

namespace Assets.Scripts
{
    public class FlagPolePlacer : MonoBehaviour
    {
        [SerializeField] private float offset;
        public void SetPositon(Vector2 flatPos)
        {
            transform.position = flatPos;
            var pos = transform.position;
            pos.y += offset;
            transform.position = pos;

        }
    }
}