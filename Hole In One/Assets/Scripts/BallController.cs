using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class BallController : MonoBehaviour
    {
        [SerializeField]
        private float maxDragDistance = 5f; // Max drag distance

        [SerializeField] 
        private float hitForceMultiplier;
        
        private Rigidbody2D rb;
        private Vector2 initialPosition;

        private float freeShotTime = 0.25f;
        private float timeSinceShot;
        private bool blockStop;

        public Action HoleInOne;

        private bool holdTime;
        private float sinkTime = 0.5f;
        private float timeInHole;
        
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

            /*if (holdTime)
            {
                if (timeInHole >= sinkTime)
                {
                    HoleInOne?.Invoke();
                    holdTime = false;
                    holeInOne = true;
                }
                else
                {
                    timeInHole += Time.deltaTime;
                }
            }*/
        }
        
        
        public void SetTOffPosition(Vector3 startingPosition)
        {
            Debug.Log("Set ball starting position to "+startingPosition);
            transform.position = startingPosition;
            initialPosition = rb.position;
            
            ResetBallPosition();
        }

        public void Shoot(Vector2 force)
        {
            rb.isKinematic = false;
            rb.AddForce(-force * hitForceMultiplier, ForceMode2D.Impulse);
            blockStop = true;
            timeSinceShot = 0.0f;
        }

        private bool holeInOne;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Debug.Log("Ball has hit the ground!");
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
                Debug.Log("Ball has hit the Hole!!");
                timeInHole = 0.0f;
                holdTime = true;
                holeInOne = true;
                HoleInOne?.Invoke();
            }
        }
        
        private void FreezeBall()
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
        
        private void ResetBallPosition()
        {
            holeInOne = false;
            FreezeBall();
            rb.position = initialPosition;
        }
    }
}