using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable

{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
    }

    public void Start()
    {
    }

    public void Update()
    {
        // if (_photonView.IsMine)
        // {
        //     LeftHandTransform.gameObject.SetActive(false);
        //     RightHandTransform.gameObject.SetActive(false);
        //     HeadTransform.gameObject.SetActive(false);
        // }
    }
}