using UnityEngine;

public class PlayerItemsInteraction : MonoBehaviour
{
    private Rigidbody rb;

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if (collision.gameObject.layer == LayerMask.NameToLayer("Player Items"))
        // {
        //     // Debug.Log($"Force upon contact: {collision.relativeVelocity}");
        //     // rb.AddForce(collision.relativeVelocity, ForceMode.VelocityChange);
        //
        //     var collisionDir = (collision.GetContact(0).point - transform.position).normalized;
        //     var inlineSpeed = Vector3.Dot(collisionDir.normalized, rb.velocity);
        //
        //     Debug.Log($"Force upon contact: {collisionDir * inlineSpeed}");
        //     // rb.AddForce(collisionDir * inlineSpeed, ForceMode.Force);
        //     // rb.AddForce(collision.relativeVelocity, ForceMode.Force);
        //
        //     rb.AddForceAtPosition(collisionDir * inlineSpeed, collision.GetContact(0).point, ForceMode.Impulse);
        // }
    }
}