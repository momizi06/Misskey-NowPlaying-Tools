using Microsoft.UI.Xaml.Controls;
using Misskey_SMTC.Media;
using Misskey_SMTC.Misskey;

namespace Misskey_SMTC.Pages.EditTemplate
{
    public sealed partial class EditTemplate : Page
    {
        public NowPlayingViewModel SongInfo => App.Media.NowPlaying;

        public string TemplateText
        {
            get => PostTemplate.Template;
            set => PostTemplate.Template = value;
        }

        public EditTemplate()
        {
            InitializeComponent();
        }
    }
}
