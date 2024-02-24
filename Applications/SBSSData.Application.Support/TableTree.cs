using HtmlAgilityPack;

using SBSSData.Softball.Common;

namespace SBSSData.Application.Support
{
    /// <summary>
    /// Represents the table organization of the document as a non-binary tree, that is, a tree having a single root, and each
    /// node can have multiple or no child nodes (<see cref="TableNode"/>).
    /// </summary>
    public class TableTree
    {
        public TableTree()
        {
            Root = null;
        }

        public TableNode? Root
        {
            get;
            set;
        }

        public void Insert(TableNode tableNode)
        {
            if (Root == null)
            {
                Root = tableNode;
            }
            else
            {
                AddNodeToTree(Root, tableNode);
            }
        }

        private static bool AddNodeToTree(TableNode target, TableNode tableNode)
        {
            bool isAdded = false;
            //Console.WriteLine($"Calling AddNodeToTree({target.Id}, {tableNode.Id})");
            // If the target is the parent of the tableNode or if the node is added the tableNode.ChildNodes collection. 
            if (target.IsParent(tableNode) || target.ChildNodes.Count == 0)
            {
                isAdded = true;
                target.ChildNodes.Add(tableNode);
                //Console.WriteLine($"Added {tableNode.Id()} to child nodes of {target.Id()}");
                if (target.IsParent(tableNode))
                {
                    tableNode.Parent = target;
                }
            }
            else
            {
                IEnumerable<TableNode> children = target.ChildNodes;
                children = children.Reverse();
                foreach (TableNode childNode in children)
                {
                    if (AddNodeToTree(childNode, tableNode))
                    {
                        break;
                    }
                }
            }

            return isAdded;
        }

        public static void SetTableHeader(TableNode tableNode, Func<TableNode, string> callback)
        {
            HtmlNode header = tableNode.Header(); //table.SelectSingleNode("./thead/tr/td[@class='typeheader']/a");
            string html = header.OuterHtml;
            string text = html.Substring("</span>", "</a>", false, false);
            string headerText = callback(tableNode);

            html = html.Replace(text, headerText);
            header.ParentNode.ReplaceChild(HtmlNode.CreateNode(html), header);

            foreach (TableNode childTable in tableNode.ChildNodes)
            {
                SetTableHeader(childTable, callback);
            }
        }
    }
}
