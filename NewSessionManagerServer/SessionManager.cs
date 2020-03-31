using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NewSessionManagerServer.net
{   
    class Sessions
    {
        readonly List <Session> _Sessions = new List<Session>();
        public Sessions()
        {
        }

        public List<Session> SessionList
        {
            get => _Sessions; 
        }

        public void AddSession(Int32 NetworkID, string PlayerName)
        {
            //this is a new session so give unique identifier, just give it a unique identifier for now
            Session _Session = new Session();
            _Session.CreateSession(NetworkID, PlayerName);
            _Session.AddPlayer(NetworkID, PlayerName);
            _Sessions.Add(_Session);
        }

        public void AddNoSession() //create session of people who have no session 
        {
            Session _Session = new Session();
            _Session.SessionID = "NOSESSION";
            _Sessions.Add(_Session);
        }

        public void JoinSessionList(string SessionToJoin, Int32 ID, string PlayerName)
        {
            Session mySession = _Sessions.Find(item => item.SessionID == SessionToJoin);
            if (mySession != null)
            {
                //add the player to the sesssion
                mySession.AddPlayer(ID, PlayerName);
            }
        }

        //When called, this will check if the player is in the session, if they are then delete them.
        public void RemoveFromSession(string SessionToLeave, Int32 ID)
        {
            Session mySession = _Sessions.Find(item => item.SessionID == SessionToLeave);

            if (mySession != null)
            {
                mySession.RemovePlayer(ID);
            }
        }
    }

}
