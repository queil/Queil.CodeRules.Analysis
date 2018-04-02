namespace Queil.CodeRules.Analyzed.LeaveThis
{
    public class Generic<T>
    {
        public T Member { get; set; }
    }

    public class Generic<T,T2>
    {
        public T Member { get; set; }
        public T2 Member2 { get; set; }
        public NestedContainer.NestedChild NestedMember { get; } = new NestedContainer.NestedChild();
    }
}