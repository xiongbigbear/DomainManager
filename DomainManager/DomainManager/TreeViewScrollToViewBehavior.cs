using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DomainManager
{
    public class TreeViewScrollToViewBehavior : DependencyObject
    {
        public static readonly DependencyProperty TreeViewSelectedItemProperty =
            DependencyProperty.RegisterAttached("TreeViewSelectedItem", typeof(object),
                typeof(TreeViewScrollToViewBehavior), new PropertyMetadata(false,
                    TreeViewSelectedItemPropertyChanged));
        public static object GetTreeViewSelectedItem(DependencyObject obj)
        {
            return obj.GetValue(TreeViewSelectedItemProperty);
        }
        public static void SetTreeViewSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(TreeViewSelectedItemProperty, value);
        }
        public static void TreeViewSelectedItemPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TreeView treeView = obj as TreeView;
            if (treeView == null)
            {
                return;
            }
            treeView.UpdateLayout();
            treeView.SelectedItemChanged -= new RoutedPropertyChangedEventHandler<object>(TreeViewScrollToViewBehavior.treeView_SelectedItemChanged);
            treeView.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(TreeViewScrollToViewBehavior.treeView_SelectedItemChanged);
            TreeViewItem treeViewItem = treeView.ItemContainerGenerator.ContainerFromItem(e.NewValue) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.BringIntoView();
                treeViewItem.IsSelected = true;
                return;
            }
            for (int i = 0; i < treeView.Items.Count; i++)
            {
                TreeViewItem treeViewItem2 = treeView.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (treeViewItem2 != null)
                {
                    bool isExpanded = treeViewItem2.IsExpanded;
                    bool flag = TreeViewScrollToViewBehavior.SelectItem(e.NewValue, treeViewItem2);
                    if (flag)
                    {
                        return;
                    }
                    treeViewItem2.IsExpanded = isExpanded;
                }
            }
        }
        private static void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetTreeViewSelectedItem(sender as TreeView, e.NewValue);
        }
        public static bool SelectItem(object obj, TreeViewItem parentItem)
        {
            if (parentItem == null)
            {
                return false;
            }
            if (!parentItem.IsExpanded)
            {
                parentItem.IsExpanded = true;
            }
            parentItem.UpdateLayout();
            TreeViewItem treeViewItem = null;
            foreach (var item in parentItem.ItemContainerGenerator.Items)
            {
                if (item == obj)
                {
                    parentItem.UpdateLayout();
                    treeViewItem = parentItem.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                    break;
                }
            }
            //TreeViewItem treeViewItem = parentItem.ItemContainerGenerator.ContainerFromItem(obj) as TreeViewItem;
            if (treeViewItem != null)
            {
                treeViewItem.BringIntoView();
                treeViewItem.IsSelected = true;
                return true;
            }
            bool result = false;
            for (int i = 0; i < parentItem.Items.Count; i++)
            {
                TreeViewItem treeViewItem2 = parentItem.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                if (treeViewItem2 != null)
                {
                    bool isExpanded = treeViewItem2.IsExpanded;
                    if (SelectItem(obj, treeViewItem2))
                    {
                        result = true;
                        break;
                    }
                    treeViewItem2.IsExpanded = isExpanded;
                }
            }
            return result;
        }
    }
}
