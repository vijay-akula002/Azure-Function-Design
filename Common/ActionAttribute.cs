namespace Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ActionAttribute : Attribute
    {
        public string Action { get; }

        public ActionAttribute(string action)
        {
            Action = action;
        }
    }
}