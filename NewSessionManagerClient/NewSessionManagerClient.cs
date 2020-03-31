using System;
using System.Collections.Generic;
using CitizenFX.Core;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;

namespace NewSessionManagerClient.net
{
    class NewSessionManagerClient : BaseScript
    {
        string strSessionID = string.Empty;
        int intPlayerID = 0;
        int intStatusPlayerID = 0;
        string strSessionPlayers = string.Empty;
        Boolean bSetInvis = false;

        public NewSessionManagerClient()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(ClientResourceStart);
            EventHandlers["onShowList"] += new Action<string>(ShowList);
            EventHandlers["SessionPlayers"] += new Action<string>(SessionPlayers);
            EventHandlers["StatusChanged"] += new Action<string,int>(StatusChanged);
            Tick += OnTick;
        }

        private async Task OnTick()
        {
            PlayerList pl = new PlayerList();
            Array SessionPlayers = strSessionPlayers.Split(',');
            //set the state for each player in the sessions
           foreach (Player player in pl) //only set the players in the session as visible            
           {
                //player.Character.IsVisible = true;                
                if (!strSessionPlayers.Contains(PlayerId().ToString()))
                {
                    SetPlayerInvisibleLocally(PlayerId(), true);
                    SetEntityNoCollisionEntity(GetPlayerPed(PlayerId()), GetPlayerPed(PlayerId()), false);
                    SetPlayerTalkingOverride(PlayerId(), true);
                }
                else
                {
                        SetPlayerInvisibleLocally(PlayerId(), false);
                        SetEntityNoCollisionEntity(GetPlayerPed(PlayerId()), GetPlayerPed(PlayerId()), false);
                        //SetPlayerTalkingOverride(PlayerId(), false);
                }
            }
        }


        private void ClientResourceStart(string resourceName)
        {
            Player p = Game.Player;

            if (strSessionID == string.Empty) //assume there is no SessionID
            {
                strSessionID = "NOSESSION";
                TriggerServerEvent("Join", "NOSESSION", p.ServerId, p.Name); //add the user to the session
                AddChatMessage("[Session Manager]", "initialising player " + p.ServerId + " - " + p.Name);
            }

            RegisterCommand("join", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                //When the user joins a session, either add them to the existing one, or create a new one for them
                if (args.Count != 0)
                {
                    AddChatMessage("[Session Manager]", "Adding to Session");
                    TriggerServerEvent("Join", args[0], p.ServerId, p.Name); //add the user to the session
                    strSessionID = p.Name.Replace(" ", string.Empty) + p.ServerId;
                    AddChatMessage("[Session Manager]", p.Name + " Joined " + strSessionID);
                }
                else
                {
                    TriggerServerEvent("Join", string.Empty, p.ServerId, p.Name);
                    strSessionID = p.Name.Replace(" ", string.Empty) + p.ServerId;
                    AddChatMessage("[Session Manager]", p.Name + " Joined " + strSessionID);
                }
            }), false);

            RegisterCommand("leave", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                //Code to remove the user from the session they were in and put them into NOSESSION
                if (args.Count != 0)
                {
                    TriggerServerEvent("Leave", args[0], p.ServerId);
                    AddChatMessage("[Session Manager]", p.Name + " Left " + strSessionID);
                    strSessionID = "NOSESSION";
                    TriggerServerEvent("Join", "NOSESSION", p.ServerId, p.Name);
                }
                else
                    AddChatMessage("Session Manager", "Usage: leave [Session Name]");
            }), false);

            RegisterCommand("show", new Action<int, List<object>, string>( (source, args, raw) =>
            {
                //Call to the server to get the list of sessions and their players. 
                //Server will return the information to be outputted to the user.
                TriggerServerEvent("SessionPlayerList");
            }), false);
        }
                
        //This function is called by the server to let the clients know the players in the sessions have changed.
        //The Session returned and Player ID will be unique to the player who made the change 
        //(so they will be updated, the others will use the old values)
        public void StatusChanged(string SessionJoined, int ChangedPlayerID)
        {
            if (Game.Player.ServerId == ChangedPlayerID) //if the server call was made by this player then update their info
            {
                strSessionID = SessionJoined;
                intStatusPlayerID = ChangedPlayerID;
            }
            TriggerServerEvent("GetPlayersInSession", strSessionID);
        }

        //Return a list of players in sessions created on the server. This function outputs them to the chat window
        public void ShowList(string SessionList)
        {

            Array strSessions = SessionList.Split(',');
            AddChatMessage("[Session Manager]", "List of Available Sessions:");
            foreach (string s in strSessions)
            {
                AddChatMessage("[Session Manager]", s);
            }
        }

        //This is called by the server to update the players in the session (this is based on this client's session)
        private void SessionPlayers(string SessionPlayers)
        {
            strSessionPlayers = SessionPlayers;

        }

        private void AddChatMessage(string Messager, string Message)
        {
            Screen.ShowNotification(Message);
            TriggerEvent("chat:addMessage", new
            {
                color = new[] { 255, 0, 0 },
                args = new[] { Messager, $"" + Message }
            });
        }


    }
}

