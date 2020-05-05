using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace PrismActiveViewSample.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private IRegionManager regionManager;
        private IRegion region;
        private string selectedViewName;
        private DelegateCommand removeActiveViewCommand;
        private DelegateCommand<Type> navigateCommand;
        private DelegateCommand removeSelectedViewCommand;
        private DelegateCommand activateSelectedViewCommand;

        public MainWindowViewModel(IRegionManager regionManager)
        {
            if (regionManager is null)
                throw new ArgumentNullException(nameof(regionManager));

            this.regionManager = regionManager;

            Views = new ObservableCollection<string>();
            ActiveViews = new ObservableCollection<string>();
        }

        public ObservableCollection<string> Views { get; }
        public ObservableCollection<string> ActiveViews { get; }

        public string SelectedViewName
        {
            get { return selectedViewName; }
            set
            {
                if (SetProperty(ref selectedViewName, value))
                {
                    activateSelectedViewCommand.RaiseCanExecuteChanged();
                    removeSelectedViewCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand RemoveActiveViewCommand
        {
            get
            {
                if (removeActiveViewCommand == null)
                    removeActiveViewCommand = new DelegateCommand(ExecuteRemoveActiveViewCommand, CanExecuteRemoveActiveViewCommand);
                return removeActiveViewCommand;
            }
        }

        public DelegateCommand<Type> NavigateCommand
        {
            get
            {
                if (navigateCommand == null)
                    navigateCommand = new DelegateCommand<Type>(ExecuteNavigateCommand);
                return navigateCommand;
            }
        }

        public DelegateCommand RemoveSelectedViewCommand
        {
            get
            {
                if (removeSelectedViewCommand == null)
                    removeSelectedViewCommand = new DelegateCommand(ExecuteRemoveSelectedViewCommand, CanExecuteSelectedViewCommand);
                return removeSelectedViewCommand;
            }
        }

        public DelegateCommand ActivateSelectedViewCommand
        {
            get
            {
                if (activateSelectedViewCommand == null)
                    activateSelectedViewCommand = new DelegateCommand(ExecuteActivateSelectedViewCommand, CanExecuteSelectedViewCommand);
                return activateSelectedViewCommand;
            }
        }

        private bool CanExecuteSelectedViewCommand()
        {
            return selectedViewName != null;
        }

        private void ExecuteActivateSelectedViewCommand()
        {
            if (selectedViewName == null)
                return;

            var view = region.Views.FirstOrDefault(v => v.ToString() == selectedViewName);
            if (view != null)
            {
                // try activate
                if (!region.ActiveViews.Contains(view))
                {
                    region.Activate(view);
                }
            }
        }

        private void ExecuteRemoveSelectedViewCommand()
        {
            if (selectedViewName == null)
                return;

            var view = region.Views.FirstOrDefault(v => v.ToString() == selectedViewName);
            if (view != null)
            {
                // try deactivate
                var activeView = region.ActiveViews.FirstOrDefault(v => v == view);
                if (activeView != null)
                    region.Deactivate(view);

                region.Remove(view);

                // try activate last
                if (region.Views.Count() > 0)
                    region.Activate(region.Views.Last());
            }
        }

        private bool CanExecuteRemoveActiveViewCommand()
        {
            return region != null && region.ActiveViews.Count() > 0;
        }

        public void InitializeRegion()
        {
            region = regionManager.Regions["MainRegion"];
            region.Views.CollectionChanged += Views_CollectionChanged;
            region.ActiveViews.CollectionChanged += ActiveViews_CollectionChanged;
        }

        private void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Views.Add(e.NewItems[0].ToString());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Views.Remove(e.OldItems[0].ToString());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Views.Clear();
                    foreach (var view in region.Views)
                        Views.Add(view.ToString());
                    break;
                default:
                    break;
            }
        }

        private void ActiveViews_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ActiveViews.Add(e.NewItems[0].ToString());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ActiveViews.Remove(e.OldItems[0].ToString());
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ActiveViews.Clear();
                    foreach (var view in region.ActiveViews)
                        ActiveViews.Add(view.ToString());
                    break;
                default:
                    break;
            }

            RemoveActiveViewCommand.RaiseCanExecuteChanged();

            SelectedViewName = ActiveViews.FirstOrDefault();
        }

        private void ExecuteRemoveActiveViewCommand()
        {
            // 1
            //var activeView = region.ActiveViews.FirstOrDefault();
            //if (activeView != null)
            //{
            //    region.Deactivate(activeView);
            //    region.Remove(activeView);

            //    if (region.Views.Count() > 0)
            //    {
            //        var view = region.Views.LastOrDefault();
            //        region.Activate(view);
            //    }
            //}

            // 2 hack
            var itemMetadataCollectionProperty = region.GetType()
                 .GetProperty("ItemMetadataCollection", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var itemMetadataCollection = itemMetadataCollectionProperty.GetValue(region) as ObservableCollection<ItemMetadata>;

            //foreach (var itemMetadata in itemMetadataCollection)
            //{
            //    bool isActive = itemMetadata.IsActive;
            //    object item = itemMetadata.Item;
            //}

            var activeItemMetadata = itemMetadataCollection.FirstOrDefault(i => i.IsActive);
            if (activeItemMetadata != null)
            {
                // Debug.Assert(activeView.Equals(activeItemMetadata.Item));

                Debug.Assert(activeItemMetadata.IsActive);

                region.Deactivate(activeItemMetadata.Item);

                Debug.Assert(!activeItemMetadata.IsActive);

                region.Remove(activeItemMetadata.Item);

                if (region.Views.Count() > 0)
                {
                    var view = region.Views.LastOrDefault();
                    region.Activate(view);
                }
            }
        }

        private void ExecuteNavigateCommand(Type type)
        {
            regionManager.RequestNavigate("MainRegion", type.Name);
        }
    }
}
