namespace Server
{
    public enum Codes
    {
        WaitingForPlayer,
        GameIsReady,
        PlayerIsReady,
        YouGoFirst,
        YouGoSecond,
        CellClicked,
        NotYourTurn,
        YouWon,
        YouLost,
        CellDestroyed,
        Disconnected,
        UpdateBoard
    }
    public class Message
    {
        public Codes Code;
        public int Row = -1, Column = -1;
        public bool HasShip = false;
        public bool[][] Matrix = null; // true if has a ship, false otherwise.
    }
}