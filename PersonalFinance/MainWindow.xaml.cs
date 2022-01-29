using PersonalFinance.Itemtemplates;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

namespace PersonalFinance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Calculating = false;
        public decimal InvAmount { get; set; }
        public DateTime? RevenueFromDate
        {
            get => SharedFunctions.RevenueFromDate;
            set => SharedFunctions.RevenueFromDate = value;
        }
        public DateTime? RevenueToDate {
            get => SharedFunctions.RevenueToDate;
            set => SharedFunctions.RevenueToDate = value;
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task FillDataGrid(string cmdstr, DataGrid dtg)
        {
            string ConStr = ConfigurationManager.ConnectionStrings["FinanceDb"].ConnectionString;
            using SqlConnection con = new SqlConnection(ConStr);
            SqlCommand cmd = new SqlCommand(cmdstr, con);
            cmd.CommandTimeout = 120;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            dt.Clear();
            await Task.Run(() => sda.Fill(dt));
            dtg.ItemsSource = dt.DefaultView;
        }
        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Label).Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF7, 0xD7, 0x5F));
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Label).Background = SystemColors.ControlBrush;
        }

        private void dtgMain_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd/MM/yyyy";
            else if (e.PropertyType == typeof(Decimal))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "{0:$#,#.00;-$#,#.00}";
        }

        private async void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string tag = (sender as Label).Tag.ToString();
            await FillDataGrid(Queries.QueryManager[TagReader.GetParameter(tag, "query")], dtgMain);
        }

        private async void tabCUpdate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.Source is TabControl)
            {
                if(e.AddedItems.Count>0 && e.AddedItems[0] is TabItem)
                {
                    var tab = e.AddedItems[0] as TabItem;
                    switch (tab.Name)
                    {
                        case nameof(tabCValue):
                        case nameof(tabIncome):
                            await (((e.AddedItems[0] as TabItem)?.Content as UpdateDataWIthSingleDate)?.Initialize() ?? Task.CompletedTask);
                            break;
                        case nameof(tabInvest):
                            await (App.Current.Resources["Investments"] as InvestmentCollection).Initialize();
                            break;
                    }
                }
            }
        }


        private async void btnUpdateInv_Click(object sender, RoutedEventArgs e)
        {
            var MainInvestment = (App.Current.Resources["MainInvestment"] as Investment);
            var Interests = (App.Current.Resources["Interests"] as InterestCollection);
            MainInvestment.InvAggregate = (cmbInvest.SelectedItem as CValue).AggID;
            MainInvestment.FromDate = dpkFromDate.SelectedDate;
            MainInvestment.EndDate = (chbxEndDate.IsChecked ?? false) ? dpkToDate.SelectedDate : null;
            MainInvestment.InvAmount = InvAmount;
            int r = await MainInvestment.InsertToDB();
            await (dtg_Investmets.ItemsSource as InvestmentCollection).UpdateFromDB();
            if (r > 0)
            {
                bool ins = await Interests.InsertToDB();
                if (ins || Interests.Count == 0)
                {
                    Interests.Clear();
                    MessageBox.Show("Done", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                return;
            }
            MessageBox.Show("Something went wrong", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void tbx_Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            var collection = CollectionViewSource.GetDefaultView(dtg_Investmets.ItemsSource);
            collection.Filter = x => (x as Investment).AggregateName?.Contains(tbx_Search.Text,StringComparison.OrdinalIgnoreCase)??false;
            collection.Refresh();
        }

        private async void btnCalculateRev_Click(object sender, RoutedEventArgs e)
        {
            if (Calculating)
                return;
            Calculating = true;

            var Dates = App.Current.Resources["Dates"] as DateCollection;
            var Revenues = App.Current.Resources["Revenues"] as RevenueCollection;
            var Rewards = App.Current.Resources["Rewards"] as CreditCardRewardCollection;
            await(App.Current.Resources["CreditCards"] as CreditCardCollection).Initialize();
            await Rewards.Initialize();
            await Dates.Initialize();
            RevenueFromDate = Dates.MinElement(x => (decimal)Math.Abs((x.DateData - RevenueFromDate).Value.TotalDays)).DateData;
            RevenueToDate = Dates.MinElement(x => (decimal)Math.Abs((x.DateData - RevenueToDate).Value.TotalDays)).DateData;
            await Revenues.Update();
            await(App.Current.Resources["Savings"] as CreditCardSavingsCollection).Update();
            Calculating = false;
        }

        private void cmbInvest_DropDownOpened(object sender, EventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(this.cmbInvest.ItemsSource);
            view.Filter = (x) => (x as CValue).Invest;
            view.Refresh();
        }
    }
    static class TagReader
    {
        public static string GetParameter(string tag, string key)
        {
            return tag.Split(';').Select(x =>
            {
                var a = x.Split('=');
                return (key: a[0], value: a[1]);
            })
            .FirstOrDefault(x => x.key == key).value;
        }
    }
}
