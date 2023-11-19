using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviourPun, IPunObservable, IOnEventCallback
{
    private PhotonView _photonView;
    private GameObject _spawnedPlayerPrefab;

    private int assignedTrackId;

    public bool CrossedFinishLine;
    private GameObject Head;

    private GameObject hud;
    private GameObject LeftHand;
    private GameObject panel;

    private Rigidbody rb;
    private GameObject resetBtn;

    public InputActionReference restartInput;
    private GameObject RightHand;
    private GameObject startTimer;
    private Text timerText;

    private float valStartTimer = 5;
    private bool waitingToStart;
    private GameObject winnerText;

    private XROrigin xrOrigin;

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 1) ResetLocal();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(CrossedFinishLine);
        }
        else
        {
            var otherWon = (bool) stream.ReceiveNext();
            if (otherWon && !CrossedFinishLine)
            {
                var hud = GameObject.Find("HUD");
                var text = hud.GetNamedChild("WinnerNotification");
                text.GetComponent<Text>().text = "LOSER";
                hud.SetActive(true);
            }
        }
    }

    private void Start()
    {
        // rb.constraints = RigidbodyConstraints.FreezePosition;
    }

    // Start is called before the first frame update
    private void Awake()
    {
        Head = gameObject.GetNamedChild("Head");
        LeftHand = gameObject.GetNamedChild("LeftHand");
        RightHand = gameObject.GetNamedChild("RightHand");
        _photonView = GetComponent<PhotonView>();

        rb = GetComponent<Rigidbody>();
        hud = SceneManager.GetActiveScene().GetRootGameObjects().First(ob => ob.name == "HUD");
        panel = hud.GetNamedChild("Panel");
        winnerText = panel.GetNamedChild("WinnerNotification");
        resetBtn = panel.GetNamedChild("Reset");
        startTimer = panel.GetNamedChild("StartTimer");
        timerText = startTimer.GetComponent<Text>();

        // panel.SetActive(false);
        winnerText.SetActive(false);
        resetBtn.SetActive(false);
        startTimer.SetActive(false);

        waitingToStart = false;
    }

    // Update is called once per frame
    private void Update()
    {
        timerText.text = valStartTimer.ToString();
        if (waitingToStart)
        {
            var runningTrack = assignedTrackId == 0
                ? GameObject.Find("PlayerTrack 1")
                : GameObject.Find("PlayerTrack 2");
            var startingPlate = runningTrack.GetNamedChild("StartingPlate");
            transform.SetPositionAndRotation(startingPlate.transform.position, startingPlate.transform.rotation);
            xrOrigin.transform.SetPositionAndRotation(startingPlate.transform.position,
                startingPlate.transform.rotation);


            valStartTimer -= Time.deltaTime;
            if (valStartTimer <= 0)
            {
                valStartTimer = 0;
                StartRace();
            }
        }
    }

    private void StartRace()
    {
        // rb.constraints = RigidbodyConstraints.None;
        startTimer.SetActive(false);
        valStartTimer = 5;
        waitingToStart = false;
    }

    [PunRPC]
    public void ResetLocal()
    {
        resetBtn.SetActive(false);
        winnerText.SetActive(false);
        var runningTrack = assignedTrackId == 0 ? GameObject.Find("PlayerTrack 1") : GameObject.Find("PlayerTrack 2");
        var startingPlate = runningTrack.GetNamedChild("StartingPlate");
        transform.SetPositionAndRotation(startingPlate.transform.position, startingPlate.transform.rotation);
        xrOrigin.transform.SetPositionAndRotation(startingPlate.transform.position, startingPlate.transform.rotation);

        // rb.constraints = RigidbodyConstraints.FreezePosition;
        valStartTimer = 5;
        startTimer.SetActive(true);

        waitingToStart = true;
    }

    [PunRPC]
    public void ResetAll()
    {
        photonView.RPC("ResetLocal", RpcTarget.AllViaServer);
    }

    public void AttachToXRRig(XROrigin XRRig)
    {
        var camera = XRRig.Camera;
        var LeftController = XRRig.transform.Find("Camera Offset/Left Controller");
        var RightController = XRRig.transform.Find("Camera Offset/Right Controller");

        // camera.transform.SetPositionAndRotation(Head.transform.position, Head.transform.rotation);
        XRRig.gameObject.transform.parent = transform;
        var position = Vector3.zero;
        XRRig.gameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
        // XRRig.gameObject.transform.SetLocalPositionAndRotation(position, Quaternion.identity);
        // XRRig.Origin = gameObject;
        Head.transform.parent = camera.transform;
        LeftHand.transform.parent = LeftController.transform;
        RightHand.transform.parent = RightController.transform;
        LeftHand.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        RightHand.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        xrOrigin = XRRig;
        // transform.position += new Vector3(0, 6.3f, 0);
    }

    //Upon collision with FinishLine
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name != "FinishLine") return;

        CrossedFinishLine = true;

        winnerText.GetComponent<Text>().text = "WINNER";
        winnerText.SetActive(true);

        resetBtn.GetComponent<Button>().onClick.AddListener(delegate { ResetAll(); });
        resetBtn.SetActive(true);

        restartInput.action.started += context => ResetAll();
    }
}