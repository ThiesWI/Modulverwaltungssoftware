using System.Windows;
using ModuleData = ModuleDataRepository.ModuleData;

namespace Modulverwaltungssoftware
{
    public partial class CommentView : Window
    {
        private ModuleData _moduleData;
        private string _version;

        public CommentView(ModuleData moduleData, string version) : this()
        {
            _moduleData = moduleData;
            _version = version;
            LoadModuleData();
        }

        private void LoadModuleData()
        {
            TitelTextBox.Text = _moduleData.Titel ?? string.Empty;
            EctsTextBox.Text = _moduleData.Ects > 0 ? _moduleData.Ects.ToString() : string.Empty;
        }
    }
}