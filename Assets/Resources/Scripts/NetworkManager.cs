using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Unity.XR.CoreUtils;
using UnityEngine;

public struct PlayerData
{
    public GameObject Instance { get; set; }
    public int AsssignedTrackId { get; set; }

    public int ActorId { get; set; }

    public PlayerData(int asssignedTrackId, int actorId) : this()
    {
        AsssignedTrackId = asssignedTrackId;
        ActorId = actorId;
    }
}

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private List<PlayerData> _players;
    private GameObject _runningTrack1;
    private GameObject _runningTrack2;
    private GameObject _spawnedPlayerPrefab;
    private GameObject _startingPlate1;
    private GameObject _startingPlate2;
    private List<GameObject> _startingPlates;
    private List<GameObject> _tracks;
    private Queue<int> _unassigedTracksIds;

    private int assignedTrackId;

    private GameObject Camera;
    private GameObject LeftController;
    private GameObject RightController;
    private bool roundStarted;

    private bool waitingForPlayers = true;


    [SerializeField] private XROrigin XRRig;

    // Start is called before the first frame update
    private void Start()
    {
        _players = new List<PlayerData>();
        _tracks = new List<GameObject>();
        _startingPlates = new List<GameObject>();
        Camera = GameObject.FindGameObjectWithTag("MainCamera");
        LeftController = GameObject.FindGameObjectWithTag("LeftController");
        RightController = GameObject.FindGameObjectWithTag("RightController");

        ConnectToServer();
        _runningTrack1 = GameObject.Find("PlayerTrack 1");
        _startingPlate1 = _runningTrack1.GetNamedChild("StartingPlate");

        _runningTrack2 = GameObject.Find("PlayerTrack 2");
        _startingPlate2 = _runningTrack2.GetNamedChild("StartingPlate");

        _tracks.Add(_runningTrack1);
        _tracks.Add(_runningTrack2);
        _startingPlates.Add(_startingPlate1);
        _startingPlates.Add(_startingPlate2);

        _unassigedTracksIds = new Queue<int>();
        _unassigedTracksIds.Enqueue(0);
        _unassigedTracksIds.Enqueue(1);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!waitingForPlayers && !roundStarted) StartRound();
    }

    private void StartRound()
    {
        roundStarted = true;
        var raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        PhotonNetwork.RaiseEvent(1, null, raiseEventOptions, SendOptions.SendReliable);
    }

    private void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Connecting to server...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to server!");
        base.OnConnectedToMaster();
        PhotonNetwork.JoinOrCreateRoom("Main Room", new RoomOptions {MaxPlayers = 4, IsVisible = true, IsOpen = true},
            TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined a room");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Count of players:" + PhotonNetwork.CountOfPlayers);

            if (_unassigedTracksIds.Count > 0)
            {
                var trackId = _unassigedTracksIds.Dequeue();

                // _spawnedPlayerPrefab = PhotonNetwork.Instantiate("Prefabs/Player",
                //     _startingPlates[trackId].transform.position,
                //     _startingPlates[trackId].transform.rotation);

                SpawnPlayerPrefab(trackId, _startingPlates[trackId].transform.position,
                    _startingPlates[trackId].transform.rotation);
                assignedTrackId = trackId;
            }
            else
            {
                SpawnObserver();

                // _spawnedPlayerPrefab =
                //     PhotonNetwork.Instantiate("Prefabs/Observer",
                //         Random.insideUnitSphere * 3 + new Vector3(-57, 0, 5), _startingPlate1.transform.rotation);
            }

            var playerManager = _spawnedPlayerPrefab.GetComponent<PlayerManager>();
            // var xrRigs = GameObject.FindGameObjectsWithTag("XRRig");
            // XRRig = xrRigs.First(xr => xr.GetComponent<PhotonView>().IsMine).GetComponent<XROrigin>();
            playerManager.AttachToXRRig(XRRig);

            if (PhotonNetwork.CountOfPlayers >= 2)
                waitingForPlayers = false;
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        if (PhotonNetwork.IsMasterClient)
        {
            _unassigedTracksIds.Enqueue(assignedTrackId);
            assignedTrackId = 0;
        }

        PhotonNetwork.Destroy(_spawnedPlayerPrefab);
    }

    [PunRPC]
    public void SpawnPlayerPrefab(int assignedTrackId, Vector3 position, Quaternion rotation)
    {
        _spawnedPlayerPrefab = PhotonNetwork.Instantiate("Prefabs/Player",
            position,
            rotation);
        this.assignedTrackId = assignedTrackId;
        var playerManager = _spawnedPlayerPrefab.GetComponent<PlayerManager>();
        // var xrRigs = GameObject.FindGameObjectsWithTag("XRRig");
        // XRRig = xrRigs.First(xr => xr.GetComponent<PhotonView>().IsMine).GetComponent<XROrigin>();
        playerManager.AttachToXRRig(XRRig);
    }

    [PunRPC]
    public void SpawnObserver()
    {
        _spawnedPlayerPrefab =
            PhotonNetwork.Instantiate("Prefabs/Observer",
                Random.insideUnitSphere * 3 + new Vector3(-57, 0, 5), _startingPlate1.transform.rotation);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player joined the room!");
        base.OnPlayerEnteredRoom(newPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Count of players:" + PhotonNetwork.CountOfPlayers);

            if (_unassigedTracksIds.Count > 0)
            {
                var trackId = _unassigedTracksIds.Dequeue();

                // _spawnedPlayerPrefab = PhotonNetwork.Instantiate("Prefabs/Player",
                //     _startingPlates[trackId].transform.position,
                //     _startingPlates[trackId].transform.rotation);

                photonView.RPC("SpawnPlayerPrefab", newPlayer, trackId, _startingPlates[trackId].transform.position,
                    _startingPlates[trackId].transform.rotation);
                _players.Add(new PlayerData(trackId, newPlayer.ActorNumber));
            }
            else
            {
                photonView.RPC("SpawnObserver", newPlayer);

                // _spawnedPlayerPrefab =
                //     PhotonNetwork.Instantiate("Prefabs/Observer",
                //         Random.insideUnitSphere * 3 + new Vector3(-57, 0, 5), _startingPlate1.transform.rotation);
            }

            var playerManager = _spawnedPlayerPrefab.GetComponent<PlayerManager>();
            // var xrRigs = GameObject.FindGameObjectsWithTag("XRRig");
            // XRRig = xrRigs.First(xr => xr.GetComponent<PhotonView>().IsMine).GetComponent<XROrigin>();
            playerManager.AttachToXRRig(XRRig);

            if (PhotonNetwork.CountOfPlayers >= 2)
                waitingForPlayers = false;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            var player = _players.FirstOrDefault(pl => pl.ActorId == otherPlayer.ActorNumber);
            _unassigedTracksIds.Enqueue(player.AsssignedTrackId);
            _players.Remove(player);
        }
    }
}