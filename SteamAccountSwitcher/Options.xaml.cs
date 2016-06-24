using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SteamAccountSwitcher
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();

            Checkbox_HideOnStartup.IsChecked = Properties.Settings.Default.HideOnStartup;
        }

        private void Button_ClickSave(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.HideOnStartup = Checkbox_HideOnStartup.IsChecked.Value;
            Properties.Settings.Default.Save();

            DialogResult = true;
        }

        private void Button_ClickCancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
