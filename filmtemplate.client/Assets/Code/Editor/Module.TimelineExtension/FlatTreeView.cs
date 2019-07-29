using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MWU.Utilities
{

    // ============== TREEVIEW ITEM =====================
    public class FlatTreeItem : TreeViewItem
    {
        public AnimationClip animclip;
        public FlatTreeItem(AnimationClip _clip, int _id)
        {
            animclip = _clip;
            displayName = animclip.name;
            id = _id;
        }
    }

    // ============== TREEVIEW =====================
    public class FlatTreeView : TreeView
    {
        public TreeViewItem m_root;
        public List<FlatTreeItem> m_itemList = new List<FlatTreeItem>();
        public event Action<IList<TreeViewItem>> beforeDroppingDraggedItems;

        //const float kRowHeights = 20f;
        //const float kToggleWidth = 0f;//18f;
        //const float kColumnPadding = 30f;
        //const float kLabelBreath = 10f;

        public FlatTreeView(TreeViewState state) : base(state)
        {
            // Important! - set which column will foldout be drawn
            //columnIndexForTreeFoldouts = 1;
            // Center foldout in the row since we also center content
            //customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
            //customFoldoutYOffset = 5f;

            //rowHeight = kRowHeights;
            //extraSpaceBeforeIconAndLabel = kToggleWidth;
            cellMargin = 30;
            //treeViewRect = new Rect(20, 110, 500, 800);
            //rowHeight = 30;
            showAlternatingRowBackgrounds = true;
            showBorder = true;

            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            //return new TreeViewItem<T>(m_TreeModel.root.id, depthForHiddenRoot, m_TreeModel.root.name, m_TreeModel.root);
            m_root = new TreeViewItem(0, -1, "Root");

            for (int i = 0; i < m_itemList.Count; i++)
            {
                m_root.AddChild(m_itemList[i]);
            }

            //SetupDepthsFromParentsAndChildren(m_root);
            return m_root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }


        //protected override void RowGUI(RowGUIArgs args)
        //{
        //    var item = (FlatTreeItem)args.item;
        //    GUILayout.Label(item.displayName);
        //}

        //public override void OnGUI(Rect rect)
        //{
        //    // Background
        //    if (Event.current.type == EventType.Repaint)
        //        DefaultStyles.backgroundOdd.Draw(rect, false, false, false, false);

        //    // TreeView
        //    base.OnGUI(rect);
        //}


        public void SetList(List<AnimationClip> _list)
        {
            var baseCount = m_itemList.Count;
            for (int i = 0; i < _list.Count; ++i)
            {
                m_itemList.Add(new FlatTreeItem(_list[i], baseCount + i));
            }
            Reload();
        }

        //-----------
        // Dragging
        //-----------
        const string k_GenericDragID = "GenericDragColumnDragging";

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (hasSearch)
                return;

            DragAndDrop.PrepareStartDrag();
            var draggedRows = GetRows().Where(item => args.draggedItemIDs.Contains(item.id)).ToList();
            DragAndDrop.SetGenericData(k_GenericDragID, draggedRows);
            DragAndDrop.objectReferences = new UnityEngine.Object[] { }; // this IS required for dragging to work
            string title = draggedRows.Count == 1 ? draggedRows[0].displayName : "< Multiple >";
            DragAndDrop.StartDrag(title);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            // Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
            var draggedRows = DragAndDrop.GetGenericData(k_GenericDragID) as List<TreeViewItem>;
            if (draggedRows == null)
                return DragAndDropVisualMode.None;

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.BetweenItems:
                    {
                        bool validDrag = ValidDrag(args.parentItem, draggedRows);
                        if (args.performDrop && validDrag)
                        {
                            OnDropDraggedElementsAtIndex(draggedRows, args.insertAtIndex == -1 ? 0 : args.insertAtIndex);
                        }
                        return validDrag ? DragAndDropVisualMode.Move : DragAndDropVisualMode.None;
                    }
                case DragAndDropPosition.UponItem:
                    {
                        return DragAndDropVisualMode.None;
                    }
                case DragAndDropPosition.OutsideItems:
                    {
                        return DragAndDropVisualMode.None;
                    }
                default:
                    Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                    return DragAndDropVisualMode.None;
            }
        }

        public virtual void OnDropDraggedElementsAtIndex(List<TreeViewItem> draggedRows, int insertIndex)
        {
            if (beforeDroppingDraggedItems != null)
                beforeDroppingDraggedItems(draggedRows);

            var draggedElements = new List<FlatTreeItem>();
            foreach (var x in draggedRows)
                draggedElements.Add((FlatTreeItem)x);
            var selectedIDs = draggedElements.Select(x => x.id).ToArray();

            if (insertIndex > 0)
            {
                insertIndex -= m_itemList.GetRange(0, insertIndex).Count(draggedElements.Contains);
            }

            foreach (var item in draggedElements)
            {
                m_itemList.Remove(item);
            }
            m_itemList.InsertRange(insertIndex, draggedElements);

            SetSelection(selectedIDs, TreeViewSelectionOptions.RevealAndFrame);
            Reload();
        }

        bool ValidDrag(TreeViewItem parent, List<TreeViewItem> draggedItems)
        {
            TreeViewItem currentParent = parent;
            while (currentParent != null)
            {
                if (draggedItems.Contains(currentParent))
                    return false;
                currentParent = currentParent.parent;
            }
            return true;
        }
    }
}