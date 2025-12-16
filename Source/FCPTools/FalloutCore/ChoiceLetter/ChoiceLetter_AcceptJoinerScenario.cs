namespace FCP.Core
{
    public class ChoiceLetter_AcceptJoinerScenario : ChoiceLetter_AcceptJoiner
    {

        public override void OpenLetter()
        {
            DiaNode diaNode = new DiaNode(Text);
            foreach (DiaOption choice in Choices)
            {
                diaNode.options.Add(choice);
            }
            Dialog_NodeTreeWithFactionInfo window = new Dialog_NodeTreeWithFactionInfo(diaNode, relatedFaction, delayInteractivity: false, radioMode, title);
            Find.WindowStack.Add(window);
        }
    }
}
