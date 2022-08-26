using System.Collections.Generic;
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



    private static readonly List<IPEndPoint> NewEndPoints = new();

    // Start is called before the first frame update
    private void Start()
    {
        if (networkDiscovery == null) networkDiscovery = FindObjectOfType<NetworkDiscovery>();

        networkDiscovery.ServerFoundCallback += NetworkDiscoveryOnServerFoundCallback;
    }

    private void NetworkDiscoveryOnServerFoundCallback(IPEndPoint endPoint)
    {
        lock (NewEndPoints)
            if (!NewEndPoints.Contains(endPoint))
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
                        InstanceFinder.ClientManager.StartConnection(ipAddress);
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
        networkDiscovery.StopSearchingForServers();
        networkDiscovery.StartSearchingForServers();
    }
}
