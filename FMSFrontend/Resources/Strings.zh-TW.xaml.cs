using System.Windows;

namespace FMSFrontend.Resources
{
    public partial class Strings_zh_TW : ResourceDictionary
    {
        public Strings_zh_TW()
        {
            InitializeComponent();
            CsvStringResourceLoader.Populate(this, "CN");
        }
    }
}
