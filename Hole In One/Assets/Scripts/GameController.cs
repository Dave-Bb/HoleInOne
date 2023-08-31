using System;
using Assets.Scripts.Controllers;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour, IAdvancer
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
       // [SerializeField] private float holeOneDistance = 30f;
        [SerializeField] private float holeDistance = 100f;

        public Action<float, bool> HoleInOne;
        
        public int CurrentScore { get; private set; }
        public bool IsPaused { get; private set; }
        
        private int holeNumber;
        

        private void Awake()
        {
            dragShotHandler.DragEnded += OnDragEnded;
            dragShotHandler.DragStarted += OnDragStarted;
            cameraMove.CameraMovementEnded += OnCameraMoveEnded;
            ballController.HoleInOne += OnHoleInOne;

           // CurrentScore = PlayerPrefs.GetInt(ScorePref, CurrentScore);
            scoreText.text = CurrentScore.ToString();
            
            meshGen.SetHolePosition(GetHoleDistance());
            meshGen.SetHoleEdgeCollider(flagPolePlacer.EdgeCollider, GetHoleDistance());
            
            HoleInOne?.Invoke(0, true);
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

        public void SetPause(bool paused)
        {
            IsPaused = paused;
        }

        private float GetHoleDistance()
        {
            var holePos = cameraMove.transform.position.x;
            holePos += holeDistance;
            return holePos;
           // return holeOneDistance + (holeDistance * holeNumber);
        }

        private void UpdateColors()
        {
            if (Camera.current != null)
            {
                Camera.current.backgroundColor = collorController.CurrentColor;
            }
            
            meshGen.UpdateColor(collorController.CurrentColor);
        }

        public void OnHoleInOne()
        {
            holeNumber += 1;
            cameraMove.HoleInOne(holeDistance * 3f);

            CurrentScore += 1;
            PlayerPrefs.SetInt(ScorePref, CurrentScore);
            PlayerPrefs.Save();
            
            scoreText.text = CurrentScore.ToString();
            collorController.SetNextColor();
            
            HoleInOne?.Invoke(CurrentScore, false);
        }
        
        public void SkipHole()
        {
            holeNumber += 1;
            cameraMove.HoleInOne(holeDistance * 3f, true);

            CurrentScore += 1;
            PlayerPrefs.SetInt(ScorePref, CurrentScore);
            PlayerPrefs.Save();
            
            scoreText.text = CurrentScore.ToString();
            collorController.SetNextColor();
            
            HoleInOne?.Invoke(CurrentScore, true);
        }

        public float testHoleDistance;
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

        public void OnAdvance(float advanceValueOne)
        {
            holeDistance = advanceValueOne;
        }

        public float CurrentAdvanceValue()
        {
            return holeDistance;
        }
    }
}