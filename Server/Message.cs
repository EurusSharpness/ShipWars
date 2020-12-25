namespace Server
{
    public enum Codes
    {
        WaitingForPlayer,
        GameIsReady,
        PlayerIsReady,
    }
    public class Message
    {
        public Codes Code;
        public int Row = -1, Column = -1;
        public bool[][] Matrix = null; // true if has a ship, false otherwise.
    }
}