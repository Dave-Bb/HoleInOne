using UnityEngine;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BallController ballController;
        [SerializeField] private MeshGen meshGen;
        [SerializeField] private DragShotHandler dragShotHandler;
        [SerializeField] private FlagPolePlacer flagPolePlacer;

        private void Awake()
        {
            dragShotHandler.DragEnded += OnDragEnded;
            dragShotHandler.DragStarted += OnDragStarted;

            meshGen.FireInitialGenFinished(OnMeshGenInited);
        }

        private void OnDragStarted()
        {
            ballController.SetTOffPosition(GetBallStartPosition(ballController.transform.localScale.y));
            flagPolePlacer.SetPositon(GetFlagStartPosition());
        }

        private void OnMeshGenInited()
        {
            ballController.SetTOffPosition(GetBallStartPosition(ballController.transform.localScale.y));
            flagPolePlacer.SetPositon(GetFlagStartPosition());
        }

        private void OnDragEnded(Vector2 dragDelta)
        {
            ballController.Shoot(dragDelta);
        }
        
        private Vector2 GetBallStartPosition(float yScale)
        {
            var furthestLeft = meshGen.GetFurthestLeftSegment();
            var targetX = furthestLeft.transform.position.x;

            var edgeCollider = furthestLeft.GetComponent<EdgeCollider2D>();
            var edgePoint = edgeCollider.points[12];
            Vector2 targetPos = new Vector2(targetX + edgePoint.x, edgePoint.y + (yScale * 0.5f) + 0.01f);
            return targetPos;
        }
        
        private Vector2 GetFlagStartPosition()
        {
            var furthestRight = meshGen.GetFurthestRightSegment();
            var targetX = furthestRight.transform.position.x;

            var edgeCollider = furthestRight.GetComponent<EdgeCollider2D>();
            var edgePoint = edgeCollider.points[45];
            Vector2 targetPos = new Vector2(targetX + edgePoint.x, edgePoint.y);
            return targetPos;
        }
    }
}