using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 7f;
    public LayerMask groundLayer;

    private Rigidbody _rb;
    private CapsuleCollider _collider;
    private PlayerState _currentState;

    // States
    private PlayerIdleState _idleState;
    private PlayerWalkState _walkState;
    private PlayerRunState _runState;
    private PlayerJumpState _jumpState;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<CapsuleCollider>();

        _idleState = new PlayerIdleState(this);
        _walkState = new PlayerWalkState(this);
        _runState = new PlayerRunState(this);
        _jumpState = new PlayerJumpState(this);

        _currentState = _idleState;
        _currentState.EnterState();
    }

    void Update()
    {
        _currentState.UpdateState();
    }

    void FixedUpdate()
    {
        _currentState.FixedUpdateState();
    }

    public void ChangeState(PlayerState newState)
    {
        _currentState.ExitState();
        _currentState = newState;
        _currentState.EnterState();
    }

    public bool IsGrounded()
    {
        float extraHeightText = 0.1f;
        return Physics.Raycast(_collider.bounds.center, Vector3.down, _collider.bounds.extents.y + extraHeightText, groundLayer);
    }

    public Rigidbody GetRigidbody() => _rb;
    public PlayerIdleState GetIdleState() => _idleState;
    public PlayerWalkState GetWalkState() => _walkState;
    public PlayerRunState GetRunState() => _runState;
    public PlayerJumpState GetJumpState() => _jumpState;
}

public abstract class PlayerState
{
    protected PlayerMovement player;

    public PlayerState(PlayerMovement player)
    {
        this.player = player;
    }

    public virtual void EnterState() { }
    public virtual void UpdateState() { }
    public virtual void FixedUpdateState() { }
    public virtual void ExitState() { }
}

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerMovement player) : base(player) { }

    public override void UpdateState()
    {
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            player.ChangeState(player.GetWalkState());
        }
        if (Input.GetButtonDown("Jump") && player.IsGrounded())
        {
            player.ChangeState(player.GetJumpState());
        }
    }
}

public class PlayerWalkState : PlayerState
{
    public PlayerWalkState(PlayerMovement player) : base(player) { }

    public override void UpdateState()
    {
        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
        {
            player.ChangeState(player.GetIdleState());
        }
        if (Input.GetButtonDown("Jump") && player.IsGrounded())
        {
            player.ChangeState(player.GetJumpState());
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            player.ChangeState(player.GetRunState());
        }
    }

    public override void FixedUpdateState()
    {
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        player.GetRigidbody().MovePosition(player.GetRigidbody().position + movement * player.walkSpeed * Time.fixedDeltaTime);
    }
}

public class PlayerRunState : PlayerState
{
    public PlayerRunState(PlayerMovement player) : base(player) { }

    public override void UpdateState()
    {
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            player.ChangeState(player.GetWalkState());
        }
        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
        {
            player.ChangeState(player.GetIdleState());
        }
        if (Input.GetButtonDown("Jump") && player.IsGrounded())
        {
            player.ChangeState(player.GetJumpState());
        }
    }

    public override void FixedUpdateState()
    {
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        player.GetRigidbody().MovePosition(player.GetRigidbody().position + movement * player.runSpeed * Time.fixedDeltaTime);
    }
}

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerMovement player) : base(player) { }

    public override void EnterState()
    {
        player.GetRigidbody().AddForce(Vector3.up * player.jumpForce, ForceMode.Impulse);
    }

    public override void FixedUpdateState()
    {
        if (player.IsGrounded() && player.GetRigidbody().velocity.y < 0.1f)
        { // Check if falling back to ground
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                player.ChangeState(player.GetWalkState());
            }
            else
            {
                player.ChangeState(player.GetIdleState());
            }
        }
    }
}
