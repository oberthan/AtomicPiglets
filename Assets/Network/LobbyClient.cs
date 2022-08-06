using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Network
{
    public class LobbyClient : MonoBehaviour
    {
        public PlayerInfo Player = new PlayerInfo { PlayerName = "Player" };
        public bool IsReady { get; set; }


    }
}
