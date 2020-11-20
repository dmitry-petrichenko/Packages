namespace RemoteApiCommands
{
    public enum Commands : byte
    {
        PING = 0b1010_1010,
        INCORRECT_COMMAND = 0b0000_0001,
        INCORRECT_COMMAND_PARAMETERS = 0b0000_0100 
    }
}