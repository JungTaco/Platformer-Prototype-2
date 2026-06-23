using UnityEngine;

public class PopUpScoreCanvas : MonoBehaviour
{
	private GameObject _camera;

	void Start()
    {
		_camera = GameObject.FindGameObjectWithTag("MainCamera");
	}

    void Update()
    {
		Quaternion lookRotation = _camera.transform.rotation;
		transform.rotation = lookRotation;
	}
}
