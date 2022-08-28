using System.Collections.Generic;
using System.Linq;
using System.Net;
using FishNet;
using FishNet.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FindGameScript : MonoBehaviour
{
    [SerializeField]
    private NetworkDiscovery networkDiscovery;

    [SerializeField]
    GameObject JoinGameButtonPrefab;

    [SerializeField]
    GameObject MatchButtonList;

    [SerializeField]
    GameObject Lobby;



    private static readonly List<IPEndPoint> EndPoints = new();
    private static readonly List<IPEndPoint> NewEndPoints = new();

    // Start is called before the first frame update
    private void Start()
    {
        if (networkDiscovery == null) networkDiscovery = FindObjectOfType<NetworkDiscovery>();

        networkDiscovery.ServerFoundCallback += NetworkDiscoveryOnServerFoundCallback;
    }

    public void OnEnable()
    {
        InstanceFinder.ClientManager.StopConnection();
        InstanceFinder.ServerManager.StopConnection(true);
    }

    private void NetworkDiscoveryOnServerFoundCallback(IPEndPoint endPoint)
    {
        lock (NewEndPoints)
            if (NewEndPoints.All(x => x.ToString() != endPoint.ToString()))
            {

                NewEndPoints.Add(endPoint);

                Debug.Log($"FOUND ENDPOINT {endPoint}. {NewEndPoints.Count}");
            }
    }

    // Update is called once per frame
    void Update()
    {

        lock (NewEndPoints)
        {
            foreach (var endPoint in NewEndPoints)
            {
                if (EndPoints.Any(x => x.ToString() == endPoint.ToString())) continue;
                EndPoints.Add(endPoint);
                var matchButtons = MatchButtonList;

                var button = Instantiate(JoinGameButtonPrefab);
                var textComponent = button.GetComponentInChildren<TMP_Text>();
                textComponent.text = endPoint.ToString();
                button.transform.SetParent(matchButtons.transform, false);

                var buttonComponent = button.GetComponent<Button>();
                buttonComponent.onClick.AddListener(
                    () =>
                    {
                        string ipAddress = endPoint.Address.ToString();
                        networkDiscovery.StopSearchingForServers();
                        InstanceFinder.ClientManager.StartConnection(ipAddress, (ushort) endPoint.Port);
                        GameObject.Find("FindGame").SetActive(false);
                        Lobby.SetActive(true);
                    }
                );
                Debug.Log("Added button to join list");
            }
            NewEndPoints.Clear();
        }

    }

    public void Scan()
    {
        lock (NewEndPoints)
        {
            NewEndPoints.Clear();
            EndPoints.Clear();
            foreach (Transform child in MatchButtonList.transform)
            {
                Destroy(child.gameObject);
            }
        }

        networkDiscovery.StopSearchingForServers();
        networkDiscovery.StartSearchingForServers();
    }
}
