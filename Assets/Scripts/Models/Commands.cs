namespace Stan.Osmos
{
    public enum CommandType
    {
        GameStart,
        MainMenu
    }

    public class ApplicationCommand
    {
        public CommandType Type;
        public CommandParams Params;
    }

    public abstract class CommandParams
    {
    }

    public class GameStartParams : CommandParams
    {
    }

}
