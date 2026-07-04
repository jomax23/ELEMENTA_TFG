using System;
using UnityEngine;
using System.Collections;

// Handles the physical dash movement and the visual "mounting" onto the air ball prefab.
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(CharacterController))]
public class PlayerAirDash : MonoBehaviour
{
    [Header("Air Ball")]
    [SerializeField] private GameObject airBallPrefab;
    [SerializeField] private float airBallY = 0.5f;
    [SerializeField] private float playerYOnBall = 1.5f;

    private float currentDashSpeed;

    [Header("Smooth Mount")]
    [SerializeField] private float mountDuration = 0.15f;

    public event Action OnDashEnded;

    private CharacterController controller;
    private PlayerMovement movement;
    
    private bool isDashing;
    private float timer;
    private int direction;
    private float originalPlayerY;
    private GameObject airBall;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
    }

    // Kicks off the dash, spawns the visual ball, and locks standard player input
    public void StartDash(float speed, float duration)
    {
        if (isDashing) return;
        
        isDashing = true;
        timer = duration; 
        direction = movement.FacingDirection;
        originalPlayerY = transform.position.y;

        movement.SetHorizontalMovementEnabled(false);
        movement.SetGravityEnabled(false);
        movement.SetDashing(true);

        if (airBallPrefab != null)
        {
            airBall = Instantiate(airBallPrefab);
            SyncAirBallPosition();
        }

        StartCoroutine(SmoothMountY(playerYOnBall));
        currentDashSpeed = speed; 
    }

    public void ForceEndDash()
    {
        if (!isDashing) return;
        StopAllCoroutines();
        EndDash();
    }

    private void Update()
    {
        if (!isDashing) return;

        timer -= Time.deltaTime;
        controller.Move(Vector3.right * direction * currentDashSpeed * Time.deltaTime);
        SyncAirBallPosition();

        if (timer <= 0f)
            EndDash();
    }

    private void EndDash()
    {
        isDashing = false;
        
        movement.SetHorizontalMovementEnabled(true);
        movement.SetGravityEnabled(true); 
        movement.SetDashing(false);

        StartCoroutine(SmoothMountY(originalPlayerY));

        if (airBall != null)
        {
            Destroy(airBall);
            airBall = null;
        }
        
        OnDashEnded?.Invoke();
    }

    // CharacterController ignores direct transform.position changes, so we temporarily 
    // disable it to smoothly interpolate the player's Y position up and down.
    private IEnumerator SmoothMountY(float targetY)
    {
        float startY = transform.position.y;
        float elapsed = 0f;
        
        while (elapsed < mountDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / mountDuration);
            
            controller.enabled = false;
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(startY, targetY, t);
            transform.position = pos;
            controller.enabled = true;
            
            yield return null;
        }

        // Snap to exact final position
        controller.enabled = false;
        Vector3 finalPos = transform.position;
        finalPos.y = targetY;
        transform.position = finalPos;
        controller.enabled = true;
    }

    private void SyncAirBallPosition()
    {
        if (airBall == null) return;
        
        Vector3 pos = transform.position;
        pos.y = airBallY;
        airBall.transform.position = pos;
    }
}