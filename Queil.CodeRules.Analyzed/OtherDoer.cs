using BadContainerIncognito = Queil.CodeRules.Analyzed.RemoveThis.Container.Container;

namespace Queil.CodeRules.Analyzed
{
    public class OtherDoer
    {
        public OtherDoer()
        {
            var doer = BadContainerIncognito.Resolve<Doer>();
            doer.DoBoth();
        }
    }
}
