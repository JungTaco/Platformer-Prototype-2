using UnityEngine;

public class CameraFollowObj : MonoBehaviour
{
    private Transform _player;
	private CharacterController _characterController;
	private float _prevYpos;

    void Start()
    {
        _player = GameObject.FindWithTag("Player").transform;
		_characterController = _player.GetComponent<CharacterController>();
		_prevYpos = _player.position.y;
	}

    void Update()
    {
        Vector3 newPos = transform.position;
        newPos.x = _player.position.x;
		newPos.z = _player.position.z;
        
        if (_characterController.isGrounded && (_player.position.y != _prevYpos))
        {
            newPos.y = _player.position.y;
            _prevYpos = _player.position.y;
		}
		transform.position = newPos;
	}
}
