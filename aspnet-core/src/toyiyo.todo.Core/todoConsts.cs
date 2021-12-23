using toyiyo.todo.Debugging;

namespace toyiyo.todo
{
    public class todoConsts
    {
        public const string LocalizationSourceName = "todo";

        public const string ConnectionStringName = "ToyiyoDb";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "dbc3a9ba9ba942a3acc937803ea46e53";
    }
}
