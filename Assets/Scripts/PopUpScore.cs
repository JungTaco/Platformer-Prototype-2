using Unity.VisualScripting;
using UnityEngine;

public class PopUpScore : MonoBehaviour
{
	[SerializeField]
	private PopUpText _prefabPopUpText;
	[SerializeField]
	private Canvas _canvas;

	private void OnEnable()
	{
		Collectible.OnCollected += CreatePopUpScoreText;
		// ...+=SaveScoretoList;
	}

	private void OnDisable()
	{
		Collectible.OnCollected -= CreatePopUpScoreText;
		// ...-=SaveScoretoList;
	}

	void Start()
    {
        //_pos = GetComponent<Transform>().position;
	}

    void Update()
    {
		
	}

	public void CreatePopUpScoreText(int score)
	{
		Vector3 pos = GetComponent<Transform>().position;
		Quaternion rot = GetComponent<Transform>().rotation;
		PopUpText popUpText = _prefabPopUpText.Create(pos, rot, score);
		popUpText.GetComponent<RectTransform>().SetParent(_canvas.transform);
	}
}
