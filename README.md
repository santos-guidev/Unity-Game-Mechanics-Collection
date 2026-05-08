# Mecânicas de Jogos em C# (Avançado)

Este repositório apresenta uma coleção de scripts C# para mecânicas de jogos, desenvolvidos com foco em padrões de projeto e boas práticas de programação. É ideal para desenvolvedores que buscam aprimorar seu portfólio com exemplos de código mais robustos e escaláveis, utilizando conceitos como **State Machines** e **Interfaces**.

## Mecânicas Incluídas:

### 1. PlayerMovement.cs (State Machine)

Este script implementa o movimento do jogador utilizando um padrão de **State Machine**. Isso permite uma organização clara e modular dos diferentes estados de movimento (Idle, Walk, Run, Jump), facilitando a adição de novos estados e a manutenção do código. O movimento é baseado em `Rigidbody` para interações físicas precisas.

**Características:**
*   **Estados de Movimento:** `Idle`, `Walk`, `Run`, `Jump`.
*   **Transições de Estado:** Gerenciadas por entrada do usuário e condições de jogo (ex: estar no chão).
*   **Controle de Velocidade:** Diferentes velocidades para andar e correr.
*   **Detecção de Chão:** Utiliza Raycast para verificar se o jogador está no chão antes de pular.

```csharp
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

// Abstract base class for player states
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

// Concrete state classes (Idle, Walk, Run, Jump) would follow here...
```

### 2. CameraFollow.cs (Câmera Suave com Funções de Utilidade)

Este script implementa uma câmera que segue suavemente um alvo, com opções para ajuste de offset e suavização de movimento e rotação. Inclui métodos públicos para alterar o alvo e o offset dinamicamente, oferecendo maior flexibilidade.

**Características:**
*   **Seguimento Suave:** Utiliza `Vector3.SmoothDamp` para um movimento de câmera orgânico.
*   **Rotação Suave:** `Quaternion.Slerp` para uma transição de rotação suave.
*   **Offset Configurável:** Permite definir a distância e ângulo da câmera em relação ao alvo.
*   **API Pública:** Métodos `SetTarget` e `SetOffset` para controle programático.

```csharp
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5f, -10f);
    public float smoothSpeed = 0.125f;
    public float rotationSpeed = 5f;

    private Vector3 _currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, smoothSpeed);

        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Sets a new target for the camera to follow.
    /// </summary>
    /// <param name="newTarget">The transform of the new target.</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Adjusts the camera's offset from the target.
    /// </summary>
    /// <param name="newOffset">The new offset vector.</param>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}
```

### 3. HealthSystem.cs (Interface IDamageable)

Um sistema de vida robusto que utiliza uma interface `IDamageable`. Isso promove o **polimorfismo** e o **baixo acoplamento**, permitindo que qualquer objeto que implemente `IDamageable` possa receber dano ou ser curado, sem a necessidade de conhecer a implementação interna do `HealthSystem`. Eventos são usados para notificar outros sistemas sobre mudanças na vida ou a morte do objeto.

**Características:**
*   **Interface `IDamageable`:** Define um contrato para objetos que podem sofrer dano.
*   **Eventos:** `OnDeath` e `OnHealthChanged` para comunicação desacoplada.
*   **Propriedade `IsAlive`:** Indica o estado de vida do objeto.
*   **Encapsulamento:** Campos privados (`_maxHealth`, `_currentHealth`) com acesso via propriedades e métodos.

```csharp
using UnityEngine;
using System;

public interface IDamageable
{
    void TakeDamage(float amount);
    void Heal(float amount);
    bool IsAlive { get; }
    event Action OnDeath;
    event Action<float> OnHealthChanged;
}

public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;

    public bool IsAlive => _currentHealth > 0;

    public event Action OnDeath;
    public event Action<float> OnHealthChanged;

    void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        _currentHealth -= amount;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        OnHealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died.");
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth);
    }

    public float GetCurrentHealth() => _currentHealth;
    public float GetMaxHealth() => _maxHealth;
    public float GetHealthPercentage() => _currentHealth / _maxHealth;
}
```

### 4. Projectile.cs (Interação com IDamageable)

Um script para gerenciar o comportamento de projéteis, incluindo movimento, dano e tempo de vida. Ele interage com qualquer objeto que implemente a interface `IDamageable`, tornando-o flexível para atingir diferentes tipos de entidades (jogadores, inimigos, objetos destrutíveis) sem acoplamento direto.

**Características:**
*   **Movimento Físico:** Utiliza `Rigidbody` para simulação de movimento.
*   **Dano Configurável:** `_damage` pode ser ajustado no Inspector.
*   **Tempo de Vida:** Projéteis são destruídos automaticamente após `_lifetime`.
*   **Efeito de Impacto:** Suporte opcional para instanciar um prefab de efeito visual no impacto.
*   **Interação com `IDamageable`:** Causa dano a qualquer objeto que implemente a interface.

```csharp
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private GameObject _impactEffectPrefab; // Optional impact effect

    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
            _rb.useGravity = false;
        }
        Destroy(gameObject, _lifetime);
    }

    void FixedUpdate()
    {
        _rb.MovePosition(transform.position + transform.forward * _speed * Time.fixedDeltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
        }

        if (_impactEffectPrefab != null)
        {
            Instantiate(_impactEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
```

## Como Contribuir

Sinta-se à vontade para fazer um fork deste repositório, adicionar suas próprias mecânicas ou melhorar as existentes. Pull requests são bem-vindos!

## Licença

Este projeto está licenciado sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.
