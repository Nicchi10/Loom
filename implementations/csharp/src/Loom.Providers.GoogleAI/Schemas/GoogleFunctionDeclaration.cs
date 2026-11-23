namespace Loom.Providers.GoogleAI.Schemas
{
    public class GoogleFunctionDeclaration
    {
        public string name { get; set; } = "";
        public string description { get; set; } = "";
        public GoogleFunctionSchema parameters { get; set; } = new GoogleFunctionSchema();
    }
}
