/*
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(CharacterController))]
public class PlayerAirDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.6f;

    [Header("Air Ball")]
    [SerializeField] private GameObject airBallPrefab;
    [SerializeField] private float airBallY = 0.5f;
    [SerializeField] private float playerYOnBall = 1.5f;

    private CharacterController controller;
    private PlayerMovement movement;

    private bool isDashing;
    private float timer;
    private int direction;

    private GameObject airBall;
    private float originalPlayerY;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = GetComponent<PlayerMovement>();
    }

    public void StartDash()
    {
        if (isDashing)
            return;

        isDashing = true;
        timer = dashDuration;
        direction = movement.FacingDirection;

        // Guardar Y original del jugador
        originalPlayerY = transform.position.y;

        // Subir jugador encima de la bola
        SetPlayerY(playerYOnBall);

        // Crear bola (vehículo)
        if (airBallPrefab != null)
        {
            airBall = Instantiate(airBallPrefab);
            UpdateAirBallPosition();
        }
    }

    private void Update()
    {
        if (!isDashing)
            return;

        timer -= Time.deltaTime;

        // Dash del jugador (root)
        Vector3 move = Vector3.right * direction * dashSpeed * Time.deltaTime;
        controller.Move(move);

        // La bola SIGUE al jugador
        UpdateAirBallPosition();

        if (timer <= 0f)
            EndDash();
    }

    private void EndDash()
    {
        isDashing = false;

        // Restaurar Y original
        SetPlayerY(originalPlayerY);

        // Destruir bola
        if (airBall != null)
            Destroy(airBall);
    }

    private void UpdateAirBallPosition()
    {
        if (airBall == null)
            return;

        Vector3 pos = transform.position;
        pos.y = airBallY;
        airBall.transform.position = pos;
    }

    private void SetPlayerY(float y)
    {
        Vector3 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }
}
*/

using System;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(CharacterController))]
public class PlayerAirDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float dashDuration = 0.6f;

    [Header("Air Ball")]
    [SerializeField] private GameObject airBallPrefab;
    [SerializeField] private float airBallY = 0.5f;
    [SerializeField] private float playerYOnBall = 1.5f;

    /// <summary>Notifica cuando el dash finaliza.</summary>
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
        movement   = GetComponent<PlayerMovement>();
    }

    public void StartDash()
    {
        if (isDashing) return;

        isDashing  = true;
        timer      = dashDuration;
        direction  = movement.FacingDirection;
        originalPlayerY = transform.position.y;

        movement.SetHorizontalMovementEnabled(false);

        
        SetPlayerYSafe(playerYOnBall);

        if (airBallPrefab != null)
        {
            airBall = Instantiate(airBallPrefab);
            SyncAirBallPosition();
        }
    }

    private void Update()
    {
        if (!isDashing) return;

        timer -= Time.deltaTime;

        Vector3 dashMove = Vector3.right * direction * dashSpeed * Time.deltaTime;
        Debug.Log($"[Dash] direction={direction} | dashMove={dashMove} | timer={timer:F2}");

        controller.Move(dashMove);
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
    /// Modifica la Y del jugador de forma segura con CharacterController.
    /// El CC debe desactivarse antes de mover el transform directamente;
    /// de lo contrario, el componente corrompe su estado interno y puede
    /// ignorar o revertir el cambio de posición.
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
