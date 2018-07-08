using UnityEngine;
using UnityEngine.UI;

public class LodObject : MonoBehaviour {

  public Slider lodValue;
  public Text lodText;
  public Toggle colliderToggle;
  public Button removeButton;
  public InputField distance;

  public void OnLodValueChanged(float value) {
    lodText.text = Mathf.RoundToInt(value).ToString();
  }


}
