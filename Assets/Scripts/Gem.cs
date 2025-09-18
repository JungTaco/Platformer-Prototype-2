using UnityEngine;

public class Gem : MonoBehaviour
{
    [SerializeField] 
    private int rotateSpeed = 2;
    //[SerializeField]
    //private AudioSource collectSound;

	// Update is called once per frame
	void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0, Space.World);
    }

	private void OnCollisionEnter(Collision collision)
	{
        //collectSound.Play();
        Destroy(gameObject);
	}
}
