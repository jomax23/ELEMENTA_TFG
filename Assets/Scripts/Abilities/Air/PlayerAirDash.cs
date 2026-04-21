using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(CharacterController))]
public class PlayerAirDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashSpeed    = 14f;
    [SerializeField] private float dashDuration = 0.6f;

    [Header("Air Ball")]
    [SerializeField] private GameObject airBallPrefab;
    [SerializeField] private float      airBallY      = 0.5f;
    [SerializeField] private float      playerYOnBall = 1.5f;

    /// <summary>Notifica cuando el dash finaliza (por timer o por ForceEndDash).</summary>
    public event Action OnDashEnded;

    private CharacterController controller;
    private PlayerMovement      movement;

    private bool   isDashing;
    private float  timer;
    private int    direction;
    private float  originalPlayerY;
    private GameObject airBall;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement   = GetComponent<PlayerMovement>();
    }

    public void StartDash()
    {
        if (isDashing) return;

        isDashing       = true;
        timer           = dashDuration;
        direction       = movement.FacingDirection;
        originalPlayerY = transform.position.y;

        movement.SetHorizontalMovementEnabled(false);
        SetPlayerYSafe(playerYOnBall);

        if (airBallPrefab != null)
        {
            airBall = Instantiate(airBallPrefab);
            SyncAirBallPosition();
        }
    }

    /// <summary>
    /// Termina el dash inmediatamente desde el exterior (p.ej. Cancel() ante un stun).
    /// </summary>
    public void ForceEndDash()
    {
        if (!isDashing) return;
        EndDash();
    }

    private void Update()
    {
        if (!isDashing) return;

        timer -= Time.deltaTime;

        controller.Move(Vector3.right * direction * dashSpeed * Time.deltaTime);
        SyncAirBallPosition();

        if (timer <= 0f)
            EndDash();
    }

    private void EndDash()
    {
        isDashing = false;

        movement.SetHorizontalMovementEnabled(true);
        SetPlayerYSafe(originalPlayerY);

        if (airBall != null)
        {
            Destroy(airBall);
            airBall = null;
        }

        OnDashEnded?.Invoke();
    }

    private void SyncAirBallPosition()
    {
        if (airBall == null) return;

        Vector3 pos = transform.position;
        pos.y = airBallY;
        airBall.transform.position = pos;
    }

    /// <summary>
    /// Modifica la Y del jugador de forma segura desactivando temporalmente el CharacterController.
    /// </summary>
    private void SetPlayerYSafe(float y)
    {
        controller.enabled = false;

        Vector3 pos = transform.position;
        pos.y = y;
        transform.position = pos;

        controller.enabled = true;
    }
}