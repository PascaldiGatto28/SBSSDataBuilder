using HtmlAgilityPack;

namespace SBSSData.Application.Support
{
    /// <summary>
    /// Wraps an <see cref="HtmlNode"/> and is an object in the <see cref="TableTree"/> class, which replicates the structure
    /// of the HTML document's tables.
    /// </summary>
    public class TableNode(HtmlNode tableHtmlNode)
    {
        public HtmlNode TableHtmlNode
        {
            get;
            init;

        } = tableHtmlNode;

        public List<TableNode> ChildNodes
        {
            get;
            init;
        } = [];

        public TableNode? Parent
        {
            get;
            set;
        } = null;

        public string Id() => TableHtmlNode.GetAttributeValue("id", Guid.NewGuid().ToString());

        public HtmlNode Header() => TableHtmlNode.SelectSingleNode(".//a[@class='typeheader']");

        public int Depth() => TableHtmlNode.Ancestors("table").Count();

        public int Index()
        {
            int index = 0;
            if (Parent != null)
            {
                for (int i = 0; i < Parent.ChildNodes.Count; i++)
                {
                    if (Parent.ChildNodes[i].Id == Id)
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index;
        }

        public bool IsParent(TableNode tableNode)
        {
            HtmlNode? parentTable = tableNode?.TableHtmlNode.Ancestors("table").FirstOrDefault();
            return (parentTable != null) && (parentTable.GetAttributeValue("id", Guid.NewGuid().ToString()) == Id());
        }

        public override string ToString()
        {
            int numberOfChildNodes = ChildNodes.Count;
            string parentId = Parent == null ? "No parent" : Parent.Id();
            return $"ID={Id()}; Parent ID={parentId}; Depth={Depth()}; Index={Index()}; " +
                   $"Number of child nodes={numberOfChildNodes}\r\nHeader={Header().InnerText.Trim()}";
        }
    }
}
