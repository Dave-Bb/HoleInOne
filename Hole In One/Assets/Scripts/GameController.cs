using Assets.Scripts.Controllers;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
    {
        private const string ScorePref = "Score";
        
        [SerializeField] private BallController ballController;
        [SerializeField] private MeshGen meshGen;
        [SerializeField] private DragShotHandler dragShotHandler;
        [SerializeField] private FlagPolePlacer flagPolePlacer;
        [SerializeField] private CameraMove cameraMove;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private ColorController collorController;
        
        [SerializeField] private float  targetTOffPosRatio = 0.075f;
        [SerializeField] private float holeOneDistance = 30f;
        [SerializeField] private float holeDistance = 100f;
        
        private int currentStcore = 0;
        private int holeNumber;

        private void Awake()
        {
            dragShotHandler.DragEnded += OnDragEnded;
            dragShotHandler.DragStarted += OnDragStarted;
            cameraMove.CameraMovementEnded += OnCameraMoveEnded;
            ballController.HoleInOne += OnHoleInOne;

            currentStcore = PlayerPrefs.GetInt(ScorePref, currentStcore);
            scoreText.text = currentStcore.ToString();
            
            meshGen.SetHolePosition(GetHoleDistance());
            meshGen.SetHoleEdgeCollider(flagPolePlacer.EdgeCollider, GetHoleDistance());
        }
        
        private void Start()
        {
            meshGen.FireInitialGenFinished(OnMeshGenInited);
        }

        private void Update()
        {
            UpdateColors();

            if (Input.GetKeyDown(KeyCode.A))
            {
                meshGen.SetHolePosition(GetHoleDistance());
                meshGen.SetHoleEdgeCollider(flagPolePlacer.EdgeCollider, GetHoleDistance());
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnHoleInOne();
            }
        }

        private float GetHoleDistance()
        {
            return holeOneDistance + (holeDistance * holeNumber);
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
            holeNumber += 1;
            cameraMove.HoleInOne(holeDistance);

            currentStcore += 1;
            PlayerPrefs.SetInt(ScorePref, currentStcore);
            PlayerPrefs.Save();
            
            scoreText.text = currentStcore.ToString();
            collorController.SetNextColor();
        }

        private void OnCameraMoveEnded()
        {
            meshGen.SetHolePosition(GetHoleDistance());
            meshGen.SetHoleEdgeCollider(flagPolePlacer.EdgeCollider, GetHoleDistance());
           
            ballController.SetTOffPosition(GetBallStartPosition(ballController.transform.localScale.y, targetTOffPosRatio));
        }

        private void OnDragStarted()
        {
            ballController.SetTOffPosition(GetBallStartPosition(ballController.transform.localScale.y, targetTOffPosRatio));
        }

        private void OnMeshGenInited()
        {
            ballController.SetTOffPosition(GetBallStartPosition(ballController.transform.localScale.y, targetTOffPosRatio));
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
        }
    }
}