using System;
using Prism;
using Prism.Mvvm;

namespace PrismActiveViewSample.ViewModels
{
    public class ViewModelBase : BindableBase, IActiveAware
    {
        private string isActiveState;
        private bool isActive;

        public string IsActiveState
        {
            get { return isActiveState; }
            set { SetProperty(ref isActiveState, value); }
        }

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                SetProperty(ref isActive, value);
                IsActiveState = isActive ? "Active" : "Inactive";
            }
        }

        public event EventHandler IsActiveChanged;
    }

    public class ViewAViewModel : ViewModelBase
    {

    }
    public class ViewBViewModel : ViewModelBase
    {

    }
    public class ViewCViewModel : ViewModelBase
    {

    }
}
