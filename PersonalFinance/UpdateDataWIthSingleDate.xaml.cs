using PersonalFinance.Itemtemplates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Lógica de interacción para UpdateDataWIthSingleDate.xaml
    /// </summary>
    public partial class UpdateDataWIthSingleDate : UserControl
    {
        public Date SelectedDate
        {
            get => SharedFunctions.AppDate;
            set => SharedFunctions.AppDate = value;
        }
        DateCollection Dates;
        IItemWInsertCollection inputManager;
        public ObservableCollection<IItemCommon_CValue_Income> dataCollection { get; set; }
        public UpdateDataWIthSingleDate()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var headers = TagReader.GetParameter(this.Tag.ToString(), "headers").Split(',');
            dtg.Columns[0].Header = headers[0];
            dtg.Columns[1].Header = headers[1];
            string its = TagReader.GetParameter(this.Tag.ToString(), "ItemSource");
            dtg.ItemsSource = App.Current.Resources[its] as System.Collections.IEnumerable;
            Dates = App.Current.Resources["Dates"] as DateCollection;
            inputManager = dtg.ItemsSource as IItemWInsertCollection;
            cmbDates.DataContext = this;
        }

        private void cmbDates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            inputManager.UpdateFromDB();
            UpdateFilter();
        }

        private void cmbDates_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!cmbDates.IsEnabled)
                cmbDates.SelectedItem = null;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Date newDate;
            int lastId = (int)Dates.Where(x => x.ID != null).Max(x => x.ID);
            if (chbUseTodayDate.IsChecked == true && !Dates.Any(x => x.DateData == DateTime.Today))
            {
                newDate = new Date
                {
                    ID = lastId + 1,
                    DateData = DateTime.Today
                };
                lastId++;
                await newDate.InsertToDB();
                await Dates.UpdateFromDB();
                cmbDates.SelectedIndex = cmbDates.Items.Count - 1;
            }
            else if (chbUseTodayDate.IsChecked == true)
                newDate = Dates.First(x => x.DateData == DateTime.Today);
            else
                newDate = cmbDates.SelectedItem as Date;
            App.Current.Resources["SelectedDate"] = newDate;
            if (await inputManager.InsertToDB())
            {
                MessageBox.Show("Done");
            }
            UpdateFilter();
        }

        public async Task Initialize()
        {
            if (!cmbDates.IsEnabled)
                cmbDates.SelectedItem = null;
            await Dates.Initialize();
            await inputManager.UpdateFromDB();
            UpdateFilter();
        }

        private void UpdateFilter()
        {
            var view = CollectionViewSource.GetDefaultView(dtg.ItemsSource);
            Predicate<object> predicate = x => true;
            if (inputManager is CValueCollection)
                predicate = x => !(x as CValue).Calculable;
            view.Filter = predicate;
            view.Refresh();
        }
    }
}
