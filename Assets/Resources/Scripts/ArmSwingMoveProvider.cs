using Photon.Pun;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ArmSwingMoveProvider : MonoBehaviourPunCallbacks
{
    private readonly Vector3 gravity = new(0, -9.8f, 0);
    private PhotonView _photonView;
    private Camera camera;
    private CharacterController characterController;

    private Vector3 currentVelocity, prevVelocity;
    [SerializeField] private float deceleration = 0.99f;
    private bool executingMotion;

    [SerializeField] private GameObject leftHand, rightHand;
    [SerializeField] private float maxAcceleration = 5;
    private float maxSwingAcceleration, maxPlayerAcceleration;
    private float motionElapsedTime;
    private float motionStartTime;


    private Vector3 previousPosLeft, previousPosRight, direction, velocitySteepStart, velocitySteepTop;

    private float prevSwingVelocityY;
    [SerializeField] private float speed = 4;
    [SerializeField] private uint swingAccelerationCompensationFactor = 100000;
    private XROrigin xrOrigin;

    // Start is called before the first frame update
    private void Start()
    {
        _photonView = transform.parent.GetComponent<PhotonView>();

        xrOrigin = GetComponent<LocomotionSystem>().xrOrigin;
        characterController = xrOrigin.gameObject.GetComponent<CharacterController>();
        camera = xrOrigin.Camera;

        prevVelocity = Vector3.zero;
        currentVelocity = characterController.velocity;
        velocitySteepStart = currentVelocity;
        motionStartTime = 0f;
        motionElapsedTime = 0f;
        prevSwingVelocityY = 0f;
        maxPlayerAcceleration = 0f;
        maxSwingAcceleration = 0f;
        executingMotion = false;

        SetPreviousPos();
    }

    // Update is called once per frame
    // private void Update()
    // {
    //     // if (!_photonView.IsMine) return;
    //
    //     var prevVelocity = currentVelocity;
    //     currentVelocity = characterController.velocity;
    //
    //     if (!executingMotion && currentVelocity.y > prevVelocity.y)
    //     {
    //         motionStartTime = Time.time;
    //         executingMotion = true;
    //         velocitySteepStart = currentVelocity;
    //     }
    //     else if (executingMotion && currentVelocity.y < prevVelocity.y)
    //     {
    //         executingMotion = false;
    //         motionElapsedTime = Time.time - motionStartTime;
    //         velocitySteepTop = currentVelocity;
    //     }
    //     else if (currentVelocity.y < prevVelocity.y)
    //     {
    //         motionElapsedTime -= Time.deltaTime;
    //     }
    //
    //     var leftHandVelocity = leftHand.transform.position - previousPosLeft;
    //     var rightHandVelocity = rightHand.transform.position - previousPosRight;
    //
    //     var swingVelocity = math.max(0, -(leftHandVelocity.y * rightHandVelocity.y) * 1000);
    //     // var swingVelocity = -(leftHandVelocity.y * rightHandVelocity.y) * 1000;
    //
    //     direction = camera.transform.forward;
    //     // characterController.SimpleMove(totalVelocity * Time.fixedDeltaTime *
    //     //                                Vector3.ProjectOnPlane(direction, Vector3.up));
    //
    //     // var motionVector = currentVelocity + speed * swingVelocity * Time.deltaTime *
    //     //     Vector3.ProjectOnPlane(direction, Vector3.up);
    //     var motionVector = speed * swingVelocity * Time.deltaTime * direction;
    //     // var smoothTime = executingMotion ? 2 : motionElapsedTime;
    //     var smoothTime = 2;
    //     var smoothenedMotion = Vector3.SmoothDamp(currentVelocity, motionVector,
    //         ref currentVelocity, smoothTime);
    //
    //     if (!executingMotion)
    //         smoothenedMotion = Vector3.SmoothDamp(currentVelocity, velocitySteepStart,
    //             ref currentVelocity, motionElapsedTime);
    //
    //
    //     // if (!(swingVelocity < 0 && characterController.velocity.magnitude < float.Epsilon))
    //     characterController.Move(motionVector);
    //     // characterController.Move(gravity * Time.deltaTime);
    //
    //     // Debug.Log($"Locomotion velocity: {swingVelocity}");
    //
    //     SetPreviousPos();
    // }

    private void Update()
    {
        direction = camera.transform.forward;

        currentVelocity = characterController.velocity;
        var leftHandVelocity = leftHand.transform.position - previousPosLeft;
        var rightHandVelocity = rightHand.transform.position - previousPosRight;

        var swingVelocityY = math.max(0,
            -(leftHandVelocity.y * rightHandVelocity.y));
        var swingAccelerationY = (swingVelocityY - prevSwingVelocityY) / Time.deltaTime;
        if (swingAccelerationY > maxSwingAcceleration) maxSwingAcceleration = swingAccelerationY;
        Debug.Log($"Max swing acceleration: {swingAccelerationY}");
        if (swingAccelerationY > 0)
            swingAccelerationY *= swingAccelerationCompensationFactor;

        var playerAcceleration = (currentVelocity.magnitude - prevVelocity.magnitude) / Time.deltaTime;
        if (playerAcceleration > maxPlayerAcceleration) maxPlayerAcceleration = playerAcceleration;
        // Debug.Log($"Max player acceleration: {maxPlayerAcceleration}");

        var decelerationTerm = 0f;
        if (swingAccelerationY < float.Epsilon)
            decelerationTerm = deceleration;

        playerAcceleration +=
            swingAccelerationY / maxAcceleration;

        // var motionVector =
        //     currentVelocity + speed * playerAcceleration * direction * Time.deltaTime;
        var motionVector = Vector3.one;
        if (swingAccelerationY > float.Epsilon)
        {
            motionVector =
                currentVelocity + speed * (swingAccelerationY / maxAcceleration) * direction * Time.deltaTime;
        }
        else
        {
            motionVector = currentVelocity + currentVelocity * decelerationTerm * Time.deltaTime;
            Debug.Log($"DECELERATING with delta {currentVelocity * decelerationTerm * Time.deltaTime}");
        }
        // Debug.Log($"PlayerAcceleration: {playerAcceleration}");
        // Debug.Log($"Motion Vector: {motionVector}");

        if (motionVector.magnitude > float.Epsilon)
            characterController.Move(motionVector * Time.deltaTime);


        prevSwingVelocityY = swingVelocityY;
        prevVelocity = currentVelocity;
        SetPreviousPos();
    }


    private void SetPreviousPos()
    {
        previousPosLeft = leftHand.transform.position;
        previousPosRight = rightHand.transform.position;
    }
}