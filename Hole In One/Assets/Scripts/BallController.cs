using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class BallController : MonoBehaviour
    {
        [SerializeField] private float hitForceMultiplier;
        
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