using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CitizenFX.Core;

namespace NewSessionManagerServer.net
{
    class Server : BaseScript
    {
        private Sessions oSessionManager;
        public Server()
        {
            if(oSessionManager == null)
               oSessionManager = new Sessions();
            //check to make sure that "NOSESSION" session exists (this holds all players not in a session)
            Session mySession = oSessionManager.SessionList.Find(item => item.SessionID == "NOSESSION");
            if (mySession == null)
            {
                oSessionManager.AddNoSession();
            }

            EventHandlers["Join"] += new Action<Player,string, Int32, string>(Join);
            EventHandlers["Leave"] += new Action<Player, string, Int32>(LeaveSession);
            EventHandlers["SessionPlayerList"] += new Action<Player>(SessionPlayerList);
            EventHandlers["GetPlayersInSession"] += new Action<Player, string, int>(GetPlayersInSession);
        }

        

        //This function will either create a new session for the player, or push them into an existing one
        //it will also remove them from the "NOSESSION" session, which is the default session for all players.
        public void Join([FromSource] Player source, string SessionToJoin, Int32 netID, string PlayerName)
        {
            //checks to see if session already exists for this user before attempting to create another one.
            string strSession = PlayerName.Replace(" ", "") + netID;
            Session mySession = oSessionManager.SessionList.Find(item => item.SessionID == strSession);

            if(SessionToJoin == string.Empty)
            {
                if (mySession == null) //make sure that session doesn't already exist before making it
                {
                    oSessionManager.AddSession(netID, PlayerName);
                }
                else
                {
                    oSessionManager.JoinSessionList(strSession, netID, PlayerName);
                }
                oSessionManager.RemoveFromSession("NOSESSION", netID);
                TriggerClientEvent("StatusChanged", strSession, netID);
            }
            else //add user to the existing session
            {
                Session myExistingSession = oSessionManager.SessionList.Find(item => item.SessionID == SessionToJoin);

                if (myExistingSession != null)
                {
                    //make sure user does not exist in the session they will be joining
                    SessionPlayer myPlayer = myExistingSession.Players.Find(item => item.PlayerID == netID);
                    if (myPlayer == null)
                    {
                        oSessionManager.JoinSessionList(SessionToJoin, netID, PlayerName);
                        if(SessionToJoin != "NOSESSION")
                            oSessionManager.RemoveFromSession("NOSESSION", netID);
                        TriggerClientEvent("StatusChanged", SessionToJoin, netID);

                    }
                }
                else
                {
                    oSessionManager.JoinSessionList("NOSESSION", netID, PlayerName);
                    TriggerClientEvent("StatusChanged", "NOSESSION", netID);
                }
            }
        }

        //user will leave the session they are in, then be pushed into the "NOSESSION" session
        public void LeaveSession([FromSource] Player p, string SessionToLeave, Int32 PlayerToLeave)
        {
            //When user decides to leave the instance, then they will be sent back to the "overworld"
            Session mySession = oSessionManager.SessionList.Find(item => item.SessionID == SessionToLeave);

            oSessionManager.RemoveFromSession(SessionToLeave, PlayerToLeave);

            if (mySession != null)
            {
                if(mySession.Players.Count == 0)
                    oSessionManager.SessionList.Remove(mySession);
            }

            //Add this user to "NOSESSION" so that they are in the default state
            oSessionManager.JoinSessionList("NOSESSION", PlayerToLeave, "");
            TriggerClientEvent("StatusChanged", "NOSESSION", PlayerToLeave);
        }

        //This simply returns a list of sessions that will be output on client side
        public void SessionPlayerList([FromSource] Player p)
        {
            string strSessionList = string.Empty;
            if (oSessionManager != null)
            {
                foreach (Session s in oSessionManager.SessionList)
                {
                    strSessionList += s.SessionID + "(Players " + s.Players.Count + "),";
                    foreach (SessionPlayer sp in s.Players)
                    {
                        strSessionList += sp.PlayerID + " - " + sp.PlayerName + ",";
                    }
                }
                if (strSessionList.Length != 0)
                    strSessionList = strSessionList.Substring(0, strSessionList.Length - 1);
            }

            if(strSessionList != string.Empty)
                TriggerClientEvent(p,"onShowList", strSessionList);
            else
                TriggerClientEvent(p, "onShowList", "No Active Sessions");
        }

        //This function returns all players in a session, so the client can set the state and decide who should be visible or not.
        public void GetPlayersInSession([FromSource] Player p, string SessionID, int PlayerID)
        {
            string strPlayers = string.Empty;
            Session mySession = oSessionManager.SessionList.Find(item => item.SessionID == SessionID);
            
            if (mySession != null)
            {
                if(mySession.Players != null)
                { 
                    foreach(SessionPlayer player in mySession.Players)
                    {
                        strPlayers += player.PlayerID + ",";
                    }
                }
                if(strPlayers.Length != 0)
                    strPlayers = strPlayers.Substring(0, strPlayers.Length - 1);
            }
            TriggerClientEvent(p,"SessionPlayers", strPlayers, PlayerID);
        }
    }
}
