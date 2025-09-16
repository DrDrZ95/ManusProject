namespace Agent.McpGateway
{
    public static class ClaudeJsonRpc
    {
        public static class Methods
        {
            public const string StartThread = "claude.startThread";
            public const string ContinueThread = "claude.continueThread";
            public const string GetThreadMessages = "claude.getThreadMessages";
        }

        public static class Params
        {
            public class StartThreadParams
            {
                public string? InitialMessage { get; set; }
            }

            public class ContinueThreadParams
            {
                public string? ThreadId { get; set; }
                public string? NewMessage { get; set; }
            }

            public class GetThreadMessagesParams
            {
                public string? ThreadId { get; set; }
            }
        }

        public static class Results
        {
            public class StartThreadResult
            {
                public string? ThreadId { get; set; }
                public string? Message { get; set; }
            }

            public class ContinueThreadResult
            {
                public string? ThreadId { get; set; }
                public string? Message { get; set; }
            }

            public class GetThreadMessagesResult
            {
                public string? ThreadId { get; set; }
                public string? Messages { get; set; }
            }
        }
    }
}

