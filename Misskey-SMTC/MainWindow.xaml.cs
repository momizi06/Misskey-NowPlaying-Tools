using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Misskey_SMTC
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;            
            SetTitleBar(titleBar); // Set the custom title bar


            OverlappedPresenter presenter = OverlappedPresenter.Create();
            presenter.PreferredMinimumWidth = 650;
            presenter.PreferredMinimumHeight = 600;
            presenter.IsMaximizable = true;
            presenter.IsMinimizable = true;
            presenter.SetBorderAndTitleBar(true, true);

            AppWindow.SetPresenter(presenter);

            navView.SelectedItem = navView.MenuItems.OfType<NavigationViewItem>().First();

        }

        private void TitleBar_PaneToggleRequested(Microsoft.UI.Xaml.Controls.TitleBar sender, object args)
        {
            navView.IsPaneOpen = !navView.IsPaneOpen;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            if (selectedItem != null)
            {
                string selectedItemTag = ((string)selectedItem.Tag);

                if (selectedItemTag == "Pages.Info")
                    ContentFrame.Navigate(typeof(Pages.Info));
                else if (selectedItemTag == "Pages.Misskey")
                    ContentFrame.Navigate(typeof(Pages.Misskey));
                else if (selectedItemTag == "Pages.Post")
                    ContentFrame.Navigate(typeof(Pages.Post));
            }
        }
    }
}
