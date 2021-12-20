using UnityEngine;
using UnityEngine.UI;
public class DropdownHandler : MonoBehaviour
{
    [SerializeField] private Dropdown _dropDown;
    [SerializeField] private NoiseGenerator _NoiseGenerator;
    // Start is called before the first frame update
    void Start()
    {
        _dropDown.onValueChanged.AddListener(delegate
        {
            ValueChangedEvent(_dropDown);
        });
    }

    public void ValueChangedEvent(Dropdown sender)
    {
        Debug.Log($"Value changed to " + (NoiseGenerator.NoiseFunction)sender.value);
        _NoiseGenerator.UsedNoise = (NoiseGenerator.NoiseFunction)sender.value;
    }
}