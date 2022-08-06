using FishNet.Connection;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class NameDisplayer : NetworkBehaviour
{
    [SerializeField]
    private TextMeshPro _text;

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetName();
        PlayerNameTracker.OnNameChange += PlayerNameTracker_OnNameChange;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        PlayerNameTracker.OnNameChange -= PlayerNameTracker_OnNameChange;
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        SetName();
    }


    private void PlayerNameTracker_OnNameChange(NetworkConnection conn, string newName)
    {
        if (conn != Owner)
            return;

        SetName();
    }


    /// <summary>
    /// Sets Text to the name for this objects owner.
    /// </summary>
    private void SetName()
    {
        string result = null;
        //Owner does nto exist.
        if (Owner.IsValid)
            result = PlayerNameTracker.GetPlayerName(Owner);

        if (string.IsNullOrEmpty(result))
            result = "Unset";

        _text.text = result;
    }
}