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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using PersonalFinance.Itemtemplates;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PersonalFinance
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool Calculating= false;
        public DateTime? RevenueFromDate
        {
            get => SharedFunctions.RevenueFromDate;
            set
            {
                if (SharedFunctions.RevenueFromDate != value)
                {
                    SharedFunctions.RevenueFromDate = value;
                    OnPropertyChanged();
                }
            }
        }
        public DateTime? RevenueToDate
        {
            get => SharedFunctions.RevenueToDate;
            set
            {
                if (SharedFunctions.RevenueToDate != value)
                {
                    SharedFunctions.RevenueToDate = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal? OverallRevenue
        {
            get => SharedFunctions.OverallRevenue;
            set
            {
                if (value != SharedFunctions.OverallRevenue)
                {
                    SharedFunctions.OverallRevenue = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal InvAmount { get; set; }
       
        public MainWindow()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task FillDataGrid(string cmdstr, DataGrid dtg)
        {
            string ConStr = ConfigurationManager.ConnectionStrings["FinanceDb"].ConnectionString;
            using(SqlConnection con = new SqlConnection(ConStr))
            {
                SqlCommand cmd = new SqlCommand(cmdstr,con);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("Summary");
                await Task.Run(()=>sda.Fill(dt));
                dtg.ItemsSource = dt.DefaultView;
            }
        }
        private void dtgMain_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd/MM/yyyy";
            else if (e.PropertyType == typeof(Decimal))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "C";
        }
        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Label).Background = new SolidColorBrush(Color.FromArgb(0xFF,0xF7,0xD7,0x5F));
            
        }
        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Label).Background = SystemColors.ControlBrush;
        }
        private async void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string tag = (sender as Label).Tag.ToString();
            await FillDataGrid(Queries.QueryManager[TagReader.GetParameter(tag, "query")],dtgMain);
        }
        private void cmbInvest_DropDownOpened(object sender, EventArgs e)
        {
            cmbInvest.ItemsSource = (App.Current.Resources["CValues"] as CValueCollection).Where(x=>x.Invest);
        }
        private async void btnUpdateInv_Click(object sender, RoutedEventArgs e)
        {
            var MainInvestment = (App.Current.Resources["MainInvestment"] as Investment);
            var Interests = (App.Current.Resources["Interests"] as InterestCollection);
            MainInvestment.InvAggregate = (cmbInvest.SelectedItem as CValue).AggID;
            MainInvestment.FromDate = dpkFromDate.SelectedDate;
            MainInvestment.EndDate = (chbxEndDate.IsChecked??false)? dpkToDate.SelectedDate:null;
            MainInvestment.InvAmount = InvAmount;
            int r = await MainInvestment.InsertToDB();
            if (r > 0) {
                bool ins = await Interests.InsertToDB();
                if (ins) {
                    Interests.Clear();
                    MessageBox.Show("Done", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                 }
            }
            MessageBox.Show("Something went wrong", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        private async void btnCalculateRev_Click(object sender, RoutedEventArgs e)
        {
            if (Calculating)
                return;
            Calculating = true;
            var Dates = App.Current.Resources["Dates"] as DateCollection;
            var Revenues = App.Current.Resources["Revenues"] as RevenueCollection;
            var Rewards = App.Current.Resources["Rewards"] as CreditCardRewardCollection;
            await (App.Current.Resources["CreditCards"] as CreditCardCollection).Initialize();
            await Rewards.Initialize();
            await Dates.Initialize();
            RevenueFromDate = Dates.MinElement(x => (decimal)Math.Abs((x.DateData - RevenueFromDate).Value.TotalDays)).DateData;
            RevenueToDate = Dates.MinElement(x => (decimal)Math.Abs((x.DateData - RevenueToDate).Value.TotalDays)).DateData;
            await Revenues.Update();
            await (App.Current.Resources["Savings"] as CreditCardSavingsCollection).Update();
            Calculating = false;
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
