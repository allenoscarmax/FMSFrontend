using System.Windows;

namespace FMSFrontend.Resources
{
    public partial class Strings_ja_JP : ResourceDictionary
    {
        public Strings_ja_JP()
        {
            InitializeComponent();
            CsvStringResourceLoader.Populate(this, "JP");
        }
    }
}