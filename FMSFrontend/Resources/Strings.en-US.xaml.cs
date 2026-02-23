using System.Windows;

namespace FMSFrontend.Resources
{
    public partial class Strings_en_US : ResourceDictionary
    {
        public Strings_en_US()
        {
            InitializeComponent();
            CsvStringResourceLoader.Populate(this, "EN");
        }
    }
}
