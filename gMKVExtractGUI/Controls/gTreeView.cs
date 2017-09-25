using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace gMKVToolNix
{
    public class gTreeView : TreeView
    {
        /// <summary>
        /// Returns a list with all the nodes of this TreeView
        /// </summary>
        public List<TreeNode> AllNodes
        {
            get
            {
                List<TreeNode> nodes = new List<TreeNode>();
                GetAllNodes(this, nodes);
                return nodes;
            }
        }

        private void GetAllNodes(Object rootNode, List<TreeNode> nodeList)
        {
            if(rootNode == null)
            {
                return;
            }
            if (nodeList == null)
            {
                nodeList = new List<TreeNode>();
            }
            if (rootNode is TreeView)
            {
                foreach (var node in (rootNode as TreeView).Nodes)
                {
                    GetAllNodes(node, nodeList);
                }
            }
            else if (rootNode is TreeNode)
            {
                nodeList.Add(rootNode as TreeNode);
                if ((rootNode as TreeNode).Nodes != null && (rootNode as TreeNode).Nodes.Count > 0)
                {
                    foreach (var node in (rootNode as TreeNode).Nodes)
                    {
                        GetAllNodes(node, nodeList);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list with all the checked nodes of this TreeView
        /// </summary>
        public List<TreeNode> CheckedNodes
        {
            get
            {
                List<TreeNode> nodes = new List<TreeNode>();
                GetCheckedNodes(this, nodes);
                return nodes;
            }
        }

        private void GetCheckedNodes(Object rootNode, List<TreeNode> nodeList)
        {
            if (nodeList == null)
            {
                nodeList = new List<TreeNode>();
            }
            if (rootNode is TreeView)
            {
                foreach (var node in (rootNode as TreeView).Nodes)
                {
                    GetCheckedNodes(node, nodeList);
                }
            }
            else if (rootNode is TreeNode)
            {
                if ((rootNode as TreeNode).Checked)
                {
                    nodeList.Add(rootNode as TreeNode);
                }
                if ((rootNode as TreeNode).Nodes != null && (rootNode as TreeNode).Nodes.Count > 0)
                {
                    foreach (var node in (rootNode as TreeNode).Nodes)
                    {
                        GetCheckedNodes(node, nodeList);
                    }
                }
            }
        }

        public gTreeView() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;
        }
    }
}
