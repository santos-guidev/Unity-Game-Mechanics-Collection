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
