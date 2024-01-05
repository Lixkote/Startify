using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Shell.Interface.StartMenu
{
    /// <summary>
    /// The <see cref="GridViewEx"/> class implements drag&drop for cases which are not supported by the <see cref="GridView"/> control:
    /// - for ItemsPanels other than StackPanel, WrapGrid, VirtualizingStackPanel;
    /// - for cases when grouping is set.
    /// It also allows adding new groups to the underlying datasource if end-user drags some item to the left-most or the rigt-most sides of the control.
    /// </summary>
    /// <remarks>
    /// To allow new group creation by the end-user, set <see cref="GridViewEx.AllowNewGroup"/> property to true.
    /// To add new group, handle <see cref="GridViewEx.BeforeDrop"/> event. The <see cref="BeforeDropItemsEventArgs.RequestCreateNewGroup"/> 
    /// property value defines whether the new group creation has been requested by the end-user actions.
    /// If this property is true, create the new data group and insert it into the groups collection at the positions, specified by the 
    /// <see cref="BeforeDropItemsEventArgs.NewGroupIndex"/> property value. Then the <see cref="GridViewEx"/> will insert dragged item
    /// into the newly added group. 
    /// Note: you should create new group from your code, as the <see cref="GridViewEx"/> control knows nothing about your data structure.
    /// </remarks>
    [TemplatePart(Name = GridViewEx.NewGroupPlaceholderFirstName, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = GridViewEx.NewGroupPlaceholderLastName, Type = typeof(FrameworkElement))]
    public class GridViewEx : GridView
    {
        //-------------------------------------------------------------
        #region ** Template Parts
        private const string NewGroupPlaceholderFirstName = "NewGroupPlaceholderFirst";
        private FrameworkElement _NewGroupPlaceholderFirst;

        private const string NewGroupPlaceholderLastName = "NewGroupPlaceholderLast";
        private FrameworkElement _NewGroupPlaceholderLast;

        #endregion

        //----------------------------------------------------------------------
        #region ** dependency properties
        /// <summary>
        /// Gets or sets the <see cref="Boolean"/> value determining whether new group should be created at dragging the item to the empty space.
        /// This is a dependency property. The default value is false.
        /// </summary>
        public bool AllowNewGroup
        {
            get { return (bool)GetValue(AllowNewGroupProperty); }
            set { SetValue(AllowNewGroupProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="AllowNewGroup"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowNewGroupProperty =
            DependencyProperty.Register("AllowNewGroup", typeof(bool), typeof(GridViewEx), new PropertyMetadata(false));
        #endregion

        //----------------------------------------------------------------------
        #region ** events
        /// <summary>
        /// Occurs before performing drop operation,
        /// </summary>
        public event EventHandler<BeforeDropItemsEventArgs> BeforeDrop;
        /// <summary>
        /// Rizes the <see cref="BeforeDrop"/> event.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected virtual void OnBeforeDrop(BeforeDropItemsEventArgs e)
        {
            if (null != BeforeDrop)
            {
                BeforeDrop(this, e);
            }
        }
        /// <summary>
        /// Occurs when control prepares UI container for some data item.
        /// </summary>
        public event EventHandler<PreparingContainerForItemEventArgs> PreparingContainerForItem;
        /// <summary>
        /// Rizes the <see cref="PreparingContainerForItem"/> event.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="item"></param>
        protected virtual void OnPreparingContainerForItem(Windows.UI.Xaml.DependencyObject element, object item)
        {
            if (null != PreparingContainerForItem)
            {
                PreparingContainerForItem(this, new PreparingContainerForItemEventArgs(element, item));
            }
        }
        #endregion

        //----------------------------------------------------------------------
        #region ** fields

        int _lastIndex = -1;  // index of the currently dragged item
        int _currentOverIndex = -1; // index which should be used if we drop immediately
        int _topReorderHintIndex = -1; // index of element which has been moved up (need it to restore item visual state later)
        int _bottomReorderHintIndex = -1; // index of element which has been moved down (need it to restore item visual state later)

        int _lastGroup = -1;  // index of the currently dragged item group
        int _currentOverGroup = -1; // index of the group under the pointer

        #endregion

        //----------------------------------------------------------------------
        #region ** ctor & initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="GridViewEx"/> control.
        /// </summary>
        public GridViewEx()
        {
            DefaultStyleKey = typeof(GridViewEx);
            this.DragItemsStarting += GridViewEx_DragItemsStarting;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _NewGroupPlaceholderFirst = GetTemplateChild(NewGroupPlaceholderFirstName) as FrameworkElement;
            _NewGroupPlaceholderLast = GetTemplateChild(NewGroupPlaceholderLastName) as FrameworkElement;
        }
        #endregion

        //----------------------------------------------------------------------
        #region ** overrides
        // set VariableSizedWrapGrid.ColumnSpan and RowSpan properties on element if they are set on item.
        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            element.SetValue(ContentControl.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            element.SetValue(ContentControl.VerticalContentAlignmentProperty, VerticalAlignment.Stretch);

            UIElement el = item as UIElement;
            if (el != null)
            {
                int colSpan = Windows.UI.Xaml.Controls.VariableSizedWrapGrid.GetColumnSpan(el);
                int rowSpan = Windows.UI.Xaml.Controls.VariableSizedWrapGrid.GetRowSpan(el);
                if (rowSpan > 1)
                {
                    // only set it if it has non-defaul value
                    element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.RowSpanProperty, rowSpan);
                }
                if (colSpan > 1)
                {
                    // only set it if it has non-defaul value
                    element.SetValue(Windows.UI.Xaml.Controls.VariableSizedWrapGrid.ColumnSpanProperty, colSpan);
                }
            }
            OnPreparingContainerForItem(element, item);
            base.PrepareContainerForItemOverride(element, item);
        }
        #endregion

        //----------------------------------------------------------------------
        #region ** protected
        /// <summary>
        /// Stores dragged items into DragEventArgs.Data.Properties["Items"] value.
        /// Override this method to set custom drag data if you need to.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDragStarting(DragItemsStartingEventArgs e)
        {
            e.Data.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            e.Data.Properties.Add("Items", e.Items);

            // set some custom drag data as below
            // e.Data.SetText(_lastIndex.ToString());
        }

        /// <summary>
        /// Handles drag&drop for cases when it is not supported by the Windows.UI.Xaml.Controls.GridView control (for example, for grouped GridView).
        /// </summary>
        /// <param name="e"></param>
        protected override async void OnDrop(DragEventArgs e)
        {
            IList<object> items = (IList<object>)e.Data.GetView().Properties["Items"];
            object item = (items != null && items.Count > 0) ? items[0] : Items[_lastIndex];

            // read custom drag data as below if they have been set in OnDragStarting
            // string text = await e.Data.GetView().GetTextAsync();

            int newIndex = GetDragOverIndex(e);
            if (newIndex >= 0)
            {
                ICollectionView view = this.ItemsSource as ICollectionView;
                if (view != null && view.CollectionGroups != null && view.CollectionGroups.Count > 0)
                {
                    // get new group index
                    FrameworkElement root = Window.Current.Content as FrameworkElement;

                    Point position = this.TransformToVisual(root).TransformPoint(e.GetPosition(this));
                    int newGroupIndex = _currentOverGroup;
                    bool requestNewGroupCreation = false;
                    int groupsCount = view.CollectionGroups.Count;
                    // check items directly under the pointer
                    foreach (var element in VisualTreeHelper.FindElementsInHostCoordinates(position, root))
                    {
                        if (element == _NewGroupPlaceholderFirst)
                        {
                            newGroupIndex = 0;
                            requestNewGroupCreation = true;
                            break;
                        }
                        if (element == _NewGroupPlaceholderLast)
                        {
                            newGroupIndex = groupsCount;
                            requestNewGroupCreation = true;
                            break;
                        }
                        else if (element is FrameworkElement && ((FrameworkElement)element).Name.ToLower() == "newgroupplaceholder")
                        {
                            newGroupIndex = _currentOverGroup + 1;
                            requestNewGroupCreation = true;
                            break;
                        }
                    }
                    if (!requestNewGroupCreation && _lastGroup == newGroupIndex && newIndex > _lastIndex)
                    {
                        // adjust newIndex if me move item forward
                        newIndex--;
                    }
                    BeforeDropItemsEventArgs args = new BeforeDropItemsEventArgs(item, _lastIndex, newIndex, _lastGroup, newGroupIndex, requestNewGroupCreation, e);
                    OnBeforeDrop(args);

                    if (!args.Cancel)
                    {
                        view = this.ItemsSource as ICollectionView;
                        if (groupsCount != view.CollectionGroups.Count && newGroupIndex == 0)
                        {
                            _lastGroup++;
                        }
                        ICollectionViewGroup oldGroup = (ICollectionViewGroup)view.CollectionGroups[_lastGroup];
                        if (newGroupIndex < view.CollectionGroups.Count)
                        {
                            ICollectionViewGroup newGroup = (ICollectionViewGroup)view.CollectionGroups[newGroupIndex];
                            if (newGroup != null)
                            {
                                // get index in the new group to insert
                                newIndex = newGroup.GroupItems.IndexOf(Items[newIndex]);

                                // todo: fire event, something like BeforeDrop? Cancellable, with information, 
                                // so that user can update item properties which depend on item group
                                if (oldGroup != null)
                                {
                                    oldGroup.GroupItems.Remove(item);
                                }
                                if (newIndex >= 0)
                                {
                                    newGroup.GroupItems.Insert(newIndex, item);
                                }
                                else
                                {
                                    // insert after the last item in the group
                                    newGroup.GroupItems.Add(item);
                                }
                            }
                        }
                    }
                }
                else if (newIndex != _lastIndex)
                {
                    if (newIndex > _lastIndex)
                    {
                        // adjust newIndex if me move item forward
                        newIndex--;
                    }
                    BeforeDropItemsEventArgs args = new BeforeDropItemsEventArgs(item, _lastIndex, newIndex, e);
                    OnBeforeDrop(args);
                    if (!args.Cancel)
                    {
                        System.Collections.IList source = this.ItemsSource as System.Collections.IList;
                        if (source != null)
                        {
                            source.RemoveAt(_lastIndex);
                            source.Insert(newIndex, item);
                        }
                        else
                        {
                            Items.RemoveAt(_lastIndex);
                            Items.Insert(newIndex, item);
                        }
                    }
                }
            }
            _lastIndex = -1;
            _currentOverIndex = -1;
            _lastGroup = -1;
            _currentOverGroup = -1;

            base.OnDrop(e);
        }

        /// <summary>
        /// Shows reoder hints while custom dragging.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDragOver(DragEventArgs e)
        {
            /* set ReorderHintStates for underlying items
                * possible ReorderHintStates:
                    - "NoReorderHint"
                    - "BottomReorderHint"
                    - "TopReorderHint"
                    - "RightReorderHint"
                    - "LeftReorderHint"
                */
            int newIndex = GetDragOverIndex(e);
            if (newIndex >= 0)
            {
                e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
                if (_currentOverIndex != newIndex)
                {
                    _currentOverIndex = newIndex;
                    if (_topReorderHintIndex != -1)
                    {
                        GoItemToState(_topReorderHintIndex, "NoReorderHint", true);
                        _topReorderHintIndex = -1;
                    }
                    if (_bottomReorderHintIndex != -1)
                    {
                        GoItemToState(_bottomReorderHintIndex, "NoReorderHint", true);
                        _bottomReorderHintIndex = -1;
                    }
                    if (newIndex > 0)
                    {
                        _topReorderHintIndex = newIndex - 1;
                    }
                    if (newIndex < Items.Count)
                    {
                        _bottomReorderHintIndex = newIndex;
                    }
                    if (IsGrouping && _currentOverGroup >= 0)
                    {
                        int topHintGroup = GetGroupForIndex(_topReorderHintIndex);
                        if (topHintGroup != _currentOverGroup)
                        {
                            _topReorderHintIndex = -1;
                        }
                        int bottomHintGroup = GetGroupForIndex(_bottomReorderHintIndex);
                        if (bottomHintGroup != _currentOverGroup)
                        {
                            _bottomReorderHintIndex = -1;
                        }
                    }
                    if (_topReorderHintIndex >= 0)
                    {
                        GoItemToState(_topReorderHintIndex, "TopReorderHint", true);
                    }
                    if (_bottomReorderHintIndex >= 0)
                    {
                        GoItemToState(_bottomReorderHintIndex, "BottomReorderHint", true);
                    }
                }
            }
            base.OnDragOver(e);
        }
        #endregion

        //----------------------------------------------------------------------
        #region ** private
        private void GridViewEx_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            _currentOverIndex = -1;
            _topReorderHintIndex = -1;
            _bottomReorderHintIndex = -1;
            _lastGroup = -1;
            _currentOverGroup = -1;
            object item = e.Items[0];
            _lastIndex = this.IndexFromContainer(this.ContainerFromItem(item));
            _lastGroup = this.GetItemGroup(item);
            OnDragStarting(e);
        }

        private int GetDragOverIndex(DragEventArgs e)
        {
            FrameworkElement root = Window.Current.Content as FrameworkElement;

            Point position = this.TransformToVisual(root).TransformPoint(e.GetPosition(this));

            int newIndex = -1;

            // check items directly under the pointer
            foreach (var element in VisualTreeHelper.FindElementsInHostCoordinates(position, root))
            {
                // assume horizontal orientation
                var container = element as ContentControl;
                if (container == null)
                {
                    continue;
                }

                int tempIndex = this.IndexFromContainer(container);
                if (tempIndex >= 0)
                {
                    _currentOverGroup = GetItemGroup(container.Content);
                    // we only need GridViewItems belonging to this GridView control
                    // if we found one - we done
                    newIndex = tempIndex;
                    // adjust index depending on pointer position
                    Point center = container.TransformToVisual(root).TransformPoint(new Point(container.ActualWidth / 2, container.ActualHeight / 2));
                    if (position.Y > center.Y)
                    {
                        newIndex++;
                    }
                    break;
                }
            }
            if (newIndex < 0)
            {
                // if we haven't found item under the pointer, check items in the rectangle to the left from the pointer position
                foreach (var element in GetIntersectingItems(position, root))
                {
                    // assume horizontal orientation
                    var container = element as ContentControl;
                    if (container == null)
                    {
                        continue;
                    }

                    int tempIndex = this.IndexFromContainer(container);
                    if (tempIndex < 0)
                    {
                        // we only need GridViewItems belonging to this GridView control
                        // so skip all elements which are not
                        continue;
                    }
                    Rect bounds = container.TransformToVisual(root).TransformBounds(new Rect(0, 0, container.ActualWidth, container.ActualHeight));

                    if (bounds.Left <= position.X && bounds.Top <= position.Y && tempIndex > newIndex)
                    {
                        _currentOverGroup = GetItemGroup(container.Content);
                        newIndex = tempIndex;
                        // adjust index depending on pointer position
                        if (position.Y > bounds.Top + container.ActualHeight / 2)
                        {
                            newIndex++;
                        }
                        if (bounds.Right > position.X && bounds.Bottom > position.Y)
                        {
                            break;
                        }
                    }
                }
            }
            if (newIndex < 0)
            {
                newIndex = 0;
            }
            if (newIndex >= Items.Count)
            {
                newIndex = Items.Count - 1;
            }
            return newIndex;
        }

        /// <summary>
        /// returns all items in the rectangle with x=0, y=0, width=intersectingPoint.X, height=root.ActualHeight.
        /// </summary>
        /// <param name="intersectingPoint"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        private static IEnumerable<UIElement> GetIntersectingItems(Point intersectingPoint, FrameworkElement root)
        {
            Rect rect = new Rect(0, 0, intersectingPoint.X, root.ActualHeight);
            return VisualTreeHelper.FindElementsInHostCoordinates(rect, root);
        }

        private void GoItemToState(int index, string state, bool useTransitions)
        {
            if (index >= 0)
            {
                Control control = this.ContainerFromIndex(index) as Control;
                if (control != null)
                {
                    VisualStateManager.GoToState(control, state, useTransitions);
                }
            }
        }

        private int GetGroupForIndex(int index)
        {
            if (index < 0)
            {
                return index;
            }
            return GetItemGroup(Items[index]);
        }
        private int GetItemGroup(object item)
        {
            ICollectionView view = this.ItemsSource as ICollectionView;
            if (view != null && view.CollectionGroups != null)
            {
                foreach (ICollectionViewGroup gr in view.CollectionGroups)
                {
                    if (gr.Group == item || gr.GroupItems.IndexOf(item) >= 0)
                    {
                        return view.CollectionGroups.IndexOf(gr);
                    }
                }
            }
            return -1;
        }
        #endregion

    }

    /// <summary>
    /// Provides data for the <see cref="GridViewEx.BeforeDrop"/> event.
    /// </summary>
    public class BeforeDropItemsEventArgs : System.ComponentModel.CancelEventArgs
    {
        internal BeforeDropItemsEventArgs(object item, int oldIndex, int newIndex, DragEventArgs dragEventArgs)
            : this(item, oldIndex, newIndex, -1, -1, false, dragEventArgs)
        {
        }
        internal BeforeDropItemsEventArgs(object item, int oldIndex, int newIndex,
            int oldGroupIndex, int newGroupIndex, bool requestCreateNewGroup, DragEventArgs dragEventArgs)
            : base()
        {
            RequestCreateNewGroup = requestCreateNewGroup;
            OldGroupIndex = oldGroupIndex;
            NewGroupIndex = newGroupIndex;
            OldIndex = oldIndex;
            NewIndex = newIndex;
            Item = item;
        }

        /// <summary>
        /// Gets the item which is beeing dragged.
        /// </summary>
        public object Item
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the current item index in the underlying data source.
        /// </summary>
        public int OldIndex
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the index in the underlying data source where the item will be insertet by the drop operation.
        /// </summary>
        public int NewIndex
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the <see cref="Boolean"/> value determining whether end-user actions requested creation of the new group in the underlying data source.
        /// This property only makes sense if GridViewEx.IsGrouping property is true.
        /// </summary>
        /// <remarks>
        /// If this property is true, create the new data group and insert it into the groups collection at the positions, specified by the 
        /// <see cref="BeforeDropItemsEventArgs.NewGroupIndex"/> property value. Then the <see cref="GridViewEx"/> will insert dragged item
        /// into the newly added group.
        /// </remarks>
        public bool RequestCreateNewGroup
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the current item data group index in the underlying data source.
        /// This property only makes sense if GridViewEx.IsGrouping property is true.
        /// </summary>
        public int OldGroupIndex
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the data group index in the underlying data source where the item will be insertet by the drop operation.
        /// This property only makes sense if GridViewEx.IsGrouping property is true.
        /// </summary>
        public int NewGroupIndex
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the original <see cref="DragEventArgs"/> data. 
        /// </summary>
        public DragEventArgs DragEventArgs
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Provides data for the <see cref="GridViewEx.PreparingContainerForItem"/> event.
    /// </summary>
    public sealed class PreparingContainerForItemEventArgs : EventArgs
    {
        internal PreparingContainerForItemEventArgs(Windows.UI.Xaml.DependencyObject element, object item)
            : base()
        {
            Element = element;
            Item = item;
        }

        /// <summary>
        /// Gets the item for which container is beeing prepared.
        /// </summary>
        public object Item
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the container prepared to show in UI.
        /// </summary>
        public Windows.UI.Xaml.DependencyObject Element
        {
            get;
            private set;
        }
    }

}
