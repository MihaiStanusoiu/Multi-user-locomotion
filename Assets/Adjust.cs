using UnityEngine;
using UnityEngine.InputSystem;

public class Adjust : MonoBehaviour
{
    public InputActionReference ScaleControl;

    // Start is called before the first frame update
    private void Start()
    {
        ScaleControl.action.performed += ActionOnperformed;
    }

    private void ActionOnperformed(InputAction.CallbackContext obj)
    {
        // Scale object by magnitude of vector of stick/trackpad input
        var value = obj.action.ReadValue<Vector2>();
        var endY = value.sqrMagnitude;
        var scaleFactor = value.y > 0 ? endY : -endY;

        gameObject.transform.localScale += Vector3.one * (scaleFactor * 0.1f);

        // Scale mass by scale
        gameObject.GetComponent<Rigidbody>().mass = gameObject.transform.localScale.x * 3;
    }

    // Update is called once per frame
    private void Update()
    {
    }
}