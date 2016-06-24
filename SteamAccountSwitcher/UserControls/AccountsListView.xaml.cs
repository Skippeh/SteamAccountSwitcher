using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace SteamAccountSwitcher.UserControls
{
    /// <summary>
    /// Interaction logic for AccountsListView.xaml
    /// </summary>
    public partial class AccountsListView : UserControl
    {
        public object SelectedItem { get { return listBoxAccounts.SelectedItem; } }
        public int SelectedIndex { get { return listBoxAccounts.SelectedIndex; } }

        public static AccountList AccountList { get; set; }
        public static Steam Steam { get; set; }
        private static bool steamInitialized;

        public bool ShowDeleteButton { get; set; }

        private readonly string settingsSave;

        public AccountsListView()
        {
            ShowDeleteButton = true; // Default to true

            InitializeComponent();

            if (!steamInitialized)
            {
                AccountList = new AccountList();

                //Get directory of Executable
                settingsSave = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).TrimStart(@"file:\\".ToCharArray());

#if DEBUG
                if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
#endif

                    try
                    {
                        ReadAccountsFromFile();
                    }
                    catch
                    {
                        //Maybe create file?
                    }

                    if (AccountList.InstallDir == "" || (AccountList.InstallDir == null))
                    {
                        AccountList.InstallDir = SelectSteamFile(@"C:\Program Files (x86)\Steam");
                        if (AccountList.InstallDir == null)
                        {
                            MessageBox.Show("You cannot use SteamAccountSwitcher without selecting your Steam.exe. Program will close now.", "Steam missing", MessageBoxButton.OK, MessageBoxImage.Error);
                            //Close();
                        }
                    }

#if DEBUG
                }
#endif

                Steam = new Steam(AccountList.InstallDir);
                steamInitialized = true;
            }

            listBoxAccounts.ItemsSource = AccountList.Accounts;
            listBoxAccounts.Items.Refresh();

            MainWindow.AccountListViews.Add(listBoxAccounts);
        }

        ~AccountsListView()
        {
            if (listBoxAccounts == null)
                return;

            if (MainWindow.AccountListViews.Contains(listBoxAccounts))
                MainWindow.AccountListViews.Remove(listBoxAccounts);
        }

        public void RefreshList()
        {
            MainWindow.RefreshLists();
        }

        private string SelectSteamFile(string initialDirectory)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter =
               "Steam |steam.exe";
            dialog.InitialDirectory = initialDirectory;
            dialog.Title = "Select your Steam Installation";
            return (dialog.ShowDialog() == true)
               ? dialog.FileName : null;
        }

        public void WriteAccountsToFile()
        {
            string xmlAccounts = this.ToXML<AccountList>(AccountList);
            StreamWriter file = new System.IO.StreamWriter(settingsSave + "\\accounts.ini");
            file.Write(Crypto.Encrypt(xmlAccounts));
            file.Close();
        }

        public void ReadAccountsFromFile()
        {
            string text = System.IO.File.ReadAllText(settingsSave + "\\accounts.ini");
            AccountList = FromXML<AccountList>(Crypto.Decrypt(text));
        }

        private static T FromXML<T>(string xml)
        {
            using (StringReader stringReader = new StringReader(xml))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
        }

        private string ToXML<T>(T obj)
        {
            using (StringWriter stringWriter = new StringWriter(new StringBuilder()))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Image itemClicked = (Image)e.Source;

            SteamAccount selectedAcc = (SteamAccount)itemClicked.DataContext;
            MessageBoxResult dialogResult = MessageBox.Show("Are you sure you want to delete the '" + selectedAcc.Name + "' account?", "Delete Account", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                AccountList.Accounts.Remove((SteamAccount)listBoxAccounts.SelectedItem);
                RefreshList();
            }
        }

        private void listBoxAccounts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SteamAccount selectedAcc = (SteamAccount)listBoxAccounts.SelectedItem;
            Steam.StartSteamAccount(selectedAcc);
        }

        private void ContextMenu_ClickEdit(object sender, RoutedEventArgs e)
        {
            if (listBoxAccounts.SelectedItem != null)
            {
                var steamAccount = (SteamAccount) listBoxAccounts.SelectedItem;
                var dialog = new AddAccount(steamAccount);
                dialog.Owner = Window.GetWindow(this);
                dialog.ShowDialog();

                if (dialog.IsInputValid())
                {
                    AccountList.Accounts[listBoxAccounts.SelectedIndex] = dialog.Account;
                    RefreshList();
                }
            }
        }

        private void ContextMenu_ClickAdd(object sender, RoutedEventArgs e)
        {
            var dialog = new AddAccount();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();

            if (dialog.IsInputValid())
            {
                AccountList.Accounts.Add(dialog.Account);
                RefreshList();
            }
        }
    }
}
