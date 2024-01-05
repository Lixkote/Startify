using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace Shell.Interface.StartMenu
{
    public class GridViewTiled : GridView
    {
        // set ColumnSpan according to the business logic (maybe some GridViewSamples.Samples.Item or group properties)
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
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
