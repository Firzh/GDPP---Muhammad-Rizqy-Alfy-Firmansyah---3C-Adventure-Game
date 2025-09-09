using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _walkSpeed;
    [SerializeField]
    private InputManager _input;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _input.OnMoveInput += Move;
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 movemenDirection = new Vector3(axisDirection.x, 0, axisDirection.y);
        Debug.Log(movemenDirection);
        _rigidbody.AddForce(movemenDirection * _walkSpeed * Time.deltaTime);
    }
}
