using System.Collections.Generic;
using DigitalOceanBot.MongoDb.Models;

namespace DigitalOceanBot
{
    public class Router
    {
        public IEnumerable<Command> Commands { get; set; }
        public IEnumerable<State> States { get; set; }
    }

    public class Command
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
    
    public class State
    {
        public List<SessionState> SessionStates { get; set; }
        public string Type { get; set; }
    }

}

