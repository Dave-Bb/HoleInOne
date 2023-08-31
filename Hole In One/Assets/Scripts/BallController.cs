using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class BallController : MonoBehaviour
    {
        [SerializeField] private float hitForceMultiplier;
        [SerializeField] private TrailRenderer trailRednerer;
        [SerializeField] private float trailRendererStartingLifeTime = 0.35f;
        [SerializeField] private float trailShutdownTime = 0.25f;
        
        private Rigidbody2D rb;
        private Vector2 initialPosition;

        private float freeShotTime = 0.25f;
        private float timeSinceShot;
        private bool blockStop;
        private bool holeInOne;
        
        public Action HoleInOne;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
        }

        private void Update()
        {
            if (timeSinceShot >= freeShotTime)
            {
                blockStop = false;
            }
            else
            {
                timeSinceShot += Time.deltaTime;
            }
        }

        public void SetTOffPosition(Vector3 startingPosition)
        {            
            
            trailRednerer.enabled = false;

        
            transform.position = startingPosition;
            initialPosition = rb.position;
            
            ResetBallPosition();
        }

        public void Shoot(Vector2 force)
        {
            ResetTrailRenderer();
            
            rb.isKinematic = false;
            rb.AddForce(-force * hitForceMultiplier, ForceMode2D.Impulse);
            blockStop = true;
            timeSinceShot = 0.0f;
        }

        private void ResetTrailRenderer()
        {
            trailRednerer.enabled = true;
            trailRednerer.time = trailRendererStartingLifeTime;
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                if (!blockStop && !holeInOne)
                {
                    FreezeBall();
                }
            }
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (holeInOne)
            {
                return;
            }
            
            if (collision.gameObject.layer == LayerMask.NameToLayer("Hole"))
            {
                holeInOne = true;
                HoleInOne?.Invoke();
            }
        }
        
        // Coroutine to lerp the float value to zero over the given time
        IEnumerator LerpRendererTimeToZero()
        {
            float startTime = Time.time;
            float startValue = trailRednerer.time;

            while (Time.time - startTime < trailShutdownTime)
            {
                // Calculate how far along the duration we are as a percentage
                float t = (Time.time - startTime) / trailShutdownTime;

                // Lerp the value towards 0
                trailRednerer.time = Mathf.Lerp(startValue, 0.0f, t);
                // Yield execution to the next frame
                yield return null;
            }
        }
        
        private void FreezeBall()
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
            StartCoroutine(LerpRendererTimeToZero());
        }
        
        private void ResetBallPosition()
        {
            holeInOne = false;
            FreezeBall();
            rb.position = initialPosition;
        }
    }
}