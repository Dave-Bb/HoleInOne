using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameAdvanceManager : MonoBehaviour
    {
        [Header("Control")] 
        [SerializeField] private float lerpTime = 3;
        
        [Header("Camera")]
        [SerializeField] private CameraMove camera;
        [SerializeField] private float cameraMinSize = 25f;
        [SerializeField] private float cameraMaxSize = 40f;

        [Header("Hole")] 
        [SerializeField] private GameController gameController;
        [SerializeField] private float holeDistanceMin = 100f;
        [SerializeField] private float holeDistanceMax = 180;

        [Header("Shot")] 
        [SerializeField] private DragShotHandler dragShotHandler;
        [SerializeField] private float shotDistanceMin = 100f;
        [SerializeField] private float shotDistanceMax = 300f;

        [Header("Game")]
        [SerializeField] private float startingProgression = 0;
        [SerializeField] private float endingProgression = 10f;


        private void Start()
        {
            gameController.HoleInOne += OnHoleInOne;
            
            OnHoleInOne(gameController.CurrentScore, true);
        }

        private IEnumerator AdvanceCoroutine(IAdvancer advancer,
            float minValue, float maxValue, float currentProgression, float duration, float wait)
        {
            yield return new WaitForSeconds(wait);
            
            float startTime = Time.time;
            float startValue = minValue + (maxValue - minValue) * Mathf.InverseLerp(minValue, maxValue, currentProgression);
            float endValue = Mathf.Lerp(minValue, maxValue, currentProgression);

            while (Time.time - startTime < duration)
            {
                float t = (Time.time - startTime) / duration;
                float nextValue = Mathf.Lerp(startValue, endValue, t);
                advancer.OnAdvance(nextValue);
                yield return null;
            }
            
            advancer.OnAdvance(endValue);
        }

        public void OnHoleInOne(float score, bool imediate)
        {
            float currentLerpRatio = Mathf.InverseLerp(startingProgression, endingProgression, score);

            var waitTime = imediate ? 0f : 1f;
            StartCoroutine(AdvanceCoroutine(dragShotHandler, dragShotHandler.CurrentAdvanceValue(), shotDistanceMax, currentLerpRatio, lerpTime, waitTime));
            StartCoroutine(AdvanceCoroutine(camera, camera.CurrentAdvanceValue(), cameraMaxSize, currentLerpRatio, lerpTime, waitTime));
            Advance(gameController, gameController.CurrentAdvanceValue(), holeDistanceMax, currentLerpRatio);
        }

        private void Advance(IAdvancer advancer, float minValue, float maxValue, float currentProgression)
        {
            float nextValue = Mathf.Lerp(minValue, maxValue, currentProgression);
            advancer.OnAdvance(nextValue);
        }
    }
}