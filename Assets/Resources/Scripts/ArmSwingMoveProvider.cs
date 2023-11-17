using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ArmSwingMoveProvider : MonoBehaviour
{
    private readonly Vector3 gravity = new(0, -9.8f, 0);
    [SerializeField] private readonly float speed = 4;
    private Camera camera;
    private CharacterController characterController;

    private Vector3 currentVelocity;
    private bool executingMotion;

    [SerializeField] private GameObject leftHand, rightHand;
    private float motionElapsedTime;

    private float motionStartTime;


    private Vector3 previousPosLeft, previousPosRight, direction, velocitySteepStart, velocitySteepTop;
    private XROrigin xrOrigin;

    // Start is called before the first frame update
    private void Start()
    {
        xrOrigin = GetComponent<LocomotionSystem>().xrOrigin;
        characterController = xrOrigin.gameObject.GetComponent<CharacterController>();
        camera = xrOrigin.Camera;

        currentVelocity = characterController.velocity;
        velocitySteepStart = currentVelocity;
        motionStartTime = 0f;
        motionElapsedTime = 0f;
        executingMotion = false;

        SetPreviousPos();
    }

    // Update is called once per frame
    private void Update()
    {
        var prevVelocity = currentVelocity;
        currentVelocity = characterController.velocity;

        if (!executingMotion && currentVelocity.y > prevVelocity.y)
        {
            motionStartTime = Time.time;
            executingMotion = true;
            velocitySteepStart = currentVelocity;
        }
        else if (executingMotion && currentVelocity.y < prevVelocity.y)
        {
            executingMotion = false;
            motionElapsedTime = Time.time - motionStartTime;
            velocitySteepTop = currentVelocity;
        }
        else if (currentVelocity.y < prevVelocity.y)
        {
            motionElapsedTime -= Time.deltaTime;
        }

        var leftHandVelocity = leftHand.transform.position - previousPosLeft;
        var rightHandVelocity = rightHand.transform.position - previousPosRight;

        var swingVelocity = math.max(0, -(leftHandVelocity.y * rightHandVelocity.y) * 1000);
        // var swingVelocity = -(leftHandVelocity.y * rightHandVelocity.y) * 1000;

        direction = camera.transform.forward;
        // characterController.SimpleMove(totalVelocity * Time.fixedDeltaTime *
        //                                Vector3.ProjectOnPlane(direction, Vector3.up));

        // var motionVector = currentVelocity + speed * swingVelocity * Time.deltaTime *
        //     Vector3.ProjectOnPlane(direction, Vector3.up);
        var motionVector = speed * swingVelocity * Time.deltaTime * direction;
        // var smoothTime = executingMotion ? 2 : motionElapsedTime;
        var smoothTime = 2;
        var smoothenedMotion = Vector3.SmoothDamp(currentVelocity, motionVector,
            ref currentVelocity, smoothTime);

        if (!executingMotion)
            smoothenedMotion = Vector3.SmoothDamp(currentVelocity, velocitySteepStart,
                ref currentVelocity, motionElapsedTime);


        // if (!(swingVelocity < 0 && characterController.velocity.magnitude < float.Epsilon))
        characterController.Move(motionVector);
        // characterController.Move(gravity * Time.deltaTime);

        Debug.Log($"Locomotion velocity: {swingVelocity}");

        SetPreviousPos();
    }


    private void SetPreviousPos()
    {
        previousPosLeft = leftHand.transform.position;
        previousPosRight = rightHand.transform.position;
    }
}