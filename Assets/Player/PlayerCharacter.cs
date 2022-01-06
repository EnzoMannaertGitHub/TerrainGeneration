using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    private CharacterController _controller;
    private Rigidbody _rigidbody;
    [SerializeField] private float _speed = 5f;
    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _rigidbody = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3();

        if (Input.GetKey(KeyCode.W))
        {
            movement.x = _speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            movement.x = -_speed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            movement.z = _speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            movement.z = -_speed;
        }
        movement.y = _rigidbody.velocity.y;
        _controller.Move(movement * Time.deltaTime);
    }
}
