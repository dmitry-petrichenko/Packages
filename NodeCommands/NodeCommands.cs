namespace C8F2740A.NetworkNode.Commands
{
    public enum NodeCommands : byte
    {
        RESPONSE_PREFIX = 0b1101_1001,
        COMMAND_PREFIX = 0b1100_1001, 
        
        RESPONSE_LOGIN_SUCCESS = 0b1101_0001,
        RESPONSE_LOGIN_FAIL = 0b1101_0010,
        RESPONSE_COMMAND_WRONG = 0b1101_0100,
    }
    
    public static class NodeCommandsExtensions 
    {
        public static byte[] ToBytesArray(this NodeCommands command)
        {
            return new[] {(byte) command};
        }
    }
}