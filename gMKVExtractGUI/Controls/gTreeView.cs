using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace gMKVToolNix
{

    public class gTreeView : TreeView
    {
        private const int TVIF_STATE = 0x8;
        private const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETITEM = TV_FIRST + 63;

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        private struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam,
                                                 ref TVITEM lParam);
        
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

        /// <summary>
        /// Hides the checkbox for the specified node on a TreeView control.
        /// </summary>
        public void HideCheckBox(TreeNode node)
        {
            TVITEM tvi = new TVITEM();
            tvi.hItem = node.Handle;
            tvi.mask = TVIF_STATE;
            tvi.stateMask = TVIS_STATEIMAGEMASK;
            tvi.state = 0;
            SendMessage(this.Handle, TVM_SETITEM, IntPtr.Zero, ref tvi);
        }

        public gTreeView() : base()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;
        }
    }
}
