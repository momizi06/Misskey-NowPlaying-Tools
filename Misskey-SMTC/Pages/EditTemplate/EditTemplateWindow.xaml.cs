using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Misskey_SMTC.Pages.EditTemplate
{
    public sealed partial class EditTemplateWindow : Window
    {
        public EditTemplateWindow()
        {
            InitializeComponent();
            RootFrame.Navigate(typeof(EditTemplate));

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
            AppWindow.Resize(new Windows.Graphics.SizeInt32(1000, 800));
            AppWindow.SetIcon("Assets/StoreLogo.png");
        }
    }
}
