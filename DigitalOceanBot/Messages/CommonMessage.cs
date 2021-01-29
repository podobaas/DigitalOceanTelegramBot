namespace DigitalOceanBot.Messages
{
    public static class CommonMessage
    {
        public static string GetMainMenuMessage()
        {
            return "You in main menu";
        }
        
        public static string GetDoneMessage()
        {
            return "Done \U0001F44C";
        }

        public static string GetErrorMessage()
        {
            return "Error \U0000274C";
        }
        public static string GetDigitalOceanApiErrorMessage(string message)
        {
            return $"DigitalOcean API Error: {message}";
        }
        
        public static string GetOperationCanceledErrorMessage()
        {
            return $"Error: Long polling task has been canceled";
        }
        
        public static string GetCriticalErrorMessage(string message)
        {
            return $"Error: {message}";
        }
    }
}