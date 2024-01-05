using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Shell.Interface.StartMenu
{
    /// <summary>
    /// The <see cref="NewGroupPlaceholder"/> control implements simple placeholder with DragOver visual state 
    /// to supply UI representation during drag and drop operations.
    /// </summary>
    [TemplateVisualState(Name = "DragOver", GroupName = "DragStates")]
    [TemplateVisualState(Name = "Normal", GroupName = "DragStates")]
    public class NewGroupPlaceholder : Control
    {
        public NewGroupPlaceholder()
        {
            AllowDrop = true;
            DefaultStyleKey = typeof(NewGroupPlaceholder);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            VisualStateManager.GoToState(this, "DragOver", true);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            VisualStateManager.GoToState(this, "Normal", true);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }
}
