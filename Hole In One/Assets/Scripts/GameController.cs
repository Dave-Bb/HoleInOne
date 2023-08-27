using Assets.Scripts.Controllers;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BallController ballController;
        [SerializeField] private MeshGen meshGen;
        [SerializeField] private DragShotHandler dragShotHandler;
        [SerializeField] private FlagPolePlacer flagPolePlacer;
        [SerializeField] private CameraMove cameraMove;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private ColorShifter colorManager;

        [SerializeField] private ColorController collorController;
        
        public float  targetTOffPosRatio = 0.075f;
        public float targetHolePositionRatio = 0.75f;

        private const string ScorePref = "Score";

        private int currentStcore = 0;

        private void Awake()
        {
            dragShotHandler.DragEnded += OnDragEnded;
            dragShotHandler.DragStarted += OnDragStarted;
            cameraMove.CameraMovementEnded += OnCameraMoveEnded;
            ballController.HoleInOne += OnHoleInOne;

            currentStcore = PlayerPrefs.GetInt(ScorePref, currentStcore);
            scoreText.text = currentStcore.ToString();
        }

        private void Update()
        {
            UpdateColors();
            
            
        }

        private void UpdateColors()
        {
            if (Camera.current != null)
            {
                Camera.current.backgroundColor = collorController.CurrentColor;
            }
            
            meshGen.UpdateColor(collorController.CurrentColor);
        }

        private void OnHoleInOne()
        {
            cameraMove.HoleInOne();

            currentStcore += 1;
            PlayerPrefs.SetInt(ScorePref, currentStcore);
            PlayerPrefs.Save();
            
            scoreText.text = currentStcore.ToString();
            collorController.SetNextColor();
        }

        private void OnCameraMoveEnded()
        {
           // meshGen.UpdateHolePosition();
           var holePositionDetails = meshGen.UpdateHolePosition(targetHolePositionRatio);
           flagPolePlacer.UpdatePositionAndColliders(holePositionDetails);
           
            ballController.SetTOffPosition(GetBallStartPosition(ballController.transform.localScale.y, targetTOffPosRatio));
        }

        private void Start()
        {
            meshGen.FireInitialGenFinished(OnMeshGenInited);
        }

        private void OnDragStarted()
        {
            ballController.SetTOffPosition(GetBallStartPosition(ballController.transform.localScale.y, targetTOffPosRatio));
          //  var flagStartingPos = GetFlagStartPosition();
            /*flagPolePlacer.SetPositon(flagStartingPos);
            meshGen.CreateHole(flagStartingPos);
            flagPolePlacer.SetPositon(GetFlagStartPosition());*/
        }

        private void OnMeshGenInited()
        {
            Debug.Log("Mesh geneted");
            ballController.SetTOffPosition(GetBallStartPosition(ballController.transform.localScale.y, targetTOffPosRatio));
            
            
            var holePositionDetails = meshGen.UpdateHolePosition(targetHolePositionRatio);
            flagPolePlacer.UpdatePositionAndColliders(holePositionDetails);
        }

        private void OnDragEnded(Vector2 dragDelta)
        {
            ballController.Shoot(dragDelta);
        }
        
        private Vector2 GetBallStartPosition(float yScale, float targetTOffPosRatio)
        {
            var rayPos = meshGen.CheckForPosition(targetTOffPosRatio);
            rayPos.y += +0.01f;
            return rayPos;
            var furthestLeft = meshGen.GetFurthestLeftSegment();
            var targetX = furthestLeft.transform.position.x;

            var edgeCollider = furthestLeft.GetComponent<EdgeCollider2D>();
            var edgePoint = edgeCollider.points[12];
            Vector2 targetPos = new Vector2(targetX + edgePoint.x, edgePoint.y + (yScale * 0.5f) + 0.01f);
            return targetPos;
        }
        
        private Vector2 GetFlagStartPosition()
        {
            var furthestRight = meshGen.GetFurthestRightSegmentGameObject();
            var targetX = furthestRight.transform.position.x;

            var edgeCollider = furthestRight.GetComponent<EdgeCollider2D>();
            var maxPoints = edgeCollider.points.Length;
            var randomIndex = 45;
            var edgePoint = edgeCollider.points[randomIndex];
            Vector2 targetPos = new Vector2(targetX + edgePoint.x, edgePoint.y);
            return targetPos;
        }
    }
}