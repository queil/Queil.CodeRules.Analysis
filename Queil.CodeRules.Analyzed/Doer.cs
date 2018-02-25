namespace Queil.CodeRules.Analyzed
{
    public class Doer
    {
        private readonly LeaveThis.Container _container;
        private static readonly string[] Container = new LeaveThis.StringContainer()
                                                                  .Container;
        public Doer()
        {
            _container = RemoveThis.Container.Container.Resolve<LeaveThis.Container>();
        }

        public void DoBoth()
        {
            _container.Do();
            RemoveThis.Container.Container.Resolve<LeaveThis.Container>().DoToo();
            Container[0] = string.Empty;
        }
    }
}
