namespace Queil.CodeRules.Analyzed.RemoveThis.Container
{
    public static class Container
    {
        public static T Resolve<T>() where T : class => null;
    }
}
