using TMPro;
using UnityEngine;

public class PopUpText : MonoBehaviour
{
	[SerializeField]
	private Transform _prefabPopUpText;
	[SerializeField]
	private float _moveSpeed = .5f;
	private TMP_Text _text;
	private float _disappearTimer;
	private Color _textColor;

	private void Awake()
	{
		_text = GetComponent<TMP_Text>();
	}

	private void Update()
	{
		transform.position += new Vector3(0, _moveSpeed * Time.deltaTime, 0);

		_disappearTimer -= Time.deltaTime;
		if (_disappearTimer < 0f)
		{
			float disappearSpeed = 3f;
			_textColor.a -= disappearSpeed * Time.deltaTime;
			_text.color = _textColor;
			if (_textColor.a < 0f)
			{
				Destroy(gameObject);
			}
		}
	}

	public PopUpText Create(Vector3 position, Quaternion rotation, int amount)
	{
		Transform PopUpTransform = Instantiate(_prefabPopUpText, position, rotation);
		PopUpText PopUpText = PopUpTransform.GetComponent<PopUpText>();
		PopUpText.Setup(amount);
		return PopUpText;
	}

	private void Setup(int amount)
	{
		_text.SetText(amount.ToString());
		_disappearTimer = .5f;
		_textColor = _text.color;
	}
}
