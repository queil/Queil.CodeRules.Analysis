using Queil.CodeRules.Analyzed.RemoveThis.Container;

namespace Queil.CodeRules.Analyzed
{
    public class OtherDoer
    {
        public OtherDoer()
        {
            var doer = Container.Resolve<Doer>();
            doer.DoBoth();
        }
    }
}
