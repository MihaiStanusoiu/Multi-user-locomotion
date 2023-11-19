using Photon.Pun;
using Photon.Realtime;
using Unity.XR.CoreUtils;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private GameObject _runningTrack1;
    private GameObject _runningTrack2;
    private GameObject _spawnedPlayerPrefab;
    private GameObject _startingPlate1;
    private GameObject _startingPlate2;

    // Start is called before the first frame update
    private void Start()
    {
        ConnectToServer();
        _runningTrack1 = GameObject.Find("PlayerTrack 1");
        _startingPlate1 = _runningTrack1.GetNamedChild("StartingPlate");

        _runningTrack2 = GameObject.Find("PlayerTrack 2");
        _startingPlate2 = _runningTrack2.GetNamedChild("StartingPlate");
    }

    // Update is called once per frame
    private void Update()
    {
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

        Debug.Log("Count of playerS:" + PhotonNetwork.CountOfPlayers);
        switch (PhotonNetwork.CountOfPlayers)
        {
            case 1:

                _spawnedPlayerPrefab = PhotonNetwork.Instantiate("Prefabs/Player", _startingPlate1.transform.position,
                    _startingPlate1.transform.rotation);
                Debug.Log("Player1");
                break;
            case 2:

                _spawnedPlayerPrefab = PhotonNetwork.Instantiate("Prefabs/Player", _startingPlate2.transform.position,
                    _startingPlate2.transform.rotation);
                Debug.Log("Player2");

                break;
            case >= 3:
                //spawn an observer
                _spawnedPlayerPrefab =
                    PhotonNetwork.Instantiate("Prefabs/Observer",
                        Random.insideUnitSphere * 3 + new Vector3(-57, 0, 5), _startingPlate1.transform.rotation);
                Debug.Log("Observer");

                return;
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New player joined the room!");
        base.OnPlayerEnteredRoom(newPlayer);
    }
}