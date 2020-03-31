using CitizenFX.Core;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace NewSessionManagerServer.net
{
    class Session
    {
        int _PlayerID; //creator of the sessions
        string _SessionID;
        readonly List<SessionPlayer> _SessionPlayers = new List<SessionPlayer>();
        public Session()
        {
        }

        public int SessionOwnerID
        {
            get => _PlayerID;
            set
            {
                _PlayerID = value;
            }
        }
        public string SessionID
        {
            get => _SessionID;
            set
            {
                _SessionID = value;
            }
        }
        public List<SessionPlayer> Players
        {
            get => _SessionPlayers; //return players in the session
        }
        public void CreateSession(int PlayerID, string PlayerName)
        {
            //create new session, with player ID of the owner
            _SessionID = PlayerName.Replace(" ", "") + PlayerID;
        }

        public void AddPlayer(int PlayerID, string PlayerName)
        {
            //Player can only exist in a session once, so if found, don't add player
            bool bPlayerExists = false;
            SessionPlayer myPlayer = _SessionPlayers.Find(item => item.PlayerID == PlayerID);
            
            if(myPlayer != null)
                bPlayerExists = true;

            if (!bPlayerExists)
            {
                SessionPlayer NewPlayer = new SessionPlayer();
                NewPlayer.PlayerID = PlayerID;
                NewPlayer.PlayerName = PlayerName;
                _SessionPlayers.Add(NewPlayer);
            }
        }

        //remove the player from the session
        public void RemovePlayer(int PlayerID)
        {
            SessionPlayer myPlayer = _SessionPlayers.Find(item => item.PlayerID == PlayerID);

            if (myPlayer != null)
            {
                _SessionPlayers.Remove(myPlayer);
            }
        }
    }
}