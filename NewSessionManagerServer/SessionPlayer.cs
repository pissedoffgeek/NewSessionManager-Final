using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewSessionManagerServer.net
{
    //This class is used to store information about each player held within a session.
    //If any other data needs to be stored, then just add properties to make them accessible
    class SessionPlayer
    {
        string _PlayerName;
        Int32 _PlayerID;

        public SessionPlayer()
        {
        }

        public string PlayerName
        {
            get => _PlayerName;
            set
            {
                _PlayerName = value;
            }
        }

        public Int32 PlayerID
        {
            get => _PlayerID;
            set
            {
                _PlayerID = value;
            }
        }

    }
}
