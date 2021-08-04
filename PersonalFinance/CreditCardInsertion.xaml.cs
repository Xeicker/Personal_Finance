using PersonalFinance.Itemtemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Lógica de interacción para CreditCardInsertion.xaml
    /// </summary>
    public partial class CreditCardInsertion : UserControl
    {
        public int CardID { get; set; }
        public DateTime dateMove { get; set; }
        public decimal amountMove { get; set; }

        private string Collection => TagReader.GetParameter(this.Tag.ToString(), "Collection");
        private string Type => TagReader.GetParameter(this.Tag.ToString(), "Type");
        public CreditCardInsertion()
        {
            InitializeComponent();
            cmbCreditCard.DataContext = this;
            dpkMainDate.DataContext = this;
            tbxAmount.DataContext = this;
        }
        private async void cmbCreditCard_DropDownOpened(object sender, EventArgs e)
        {
            await (App.Current.Resources["CreditCards"] as CreditCardCollection).Initialize();
        }
        private async void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            var CCollection = App.Current.Resources[Collection] as IItemCollection;
            if (CCollection.GetGenericCollection().Select(x=>x as CreditCardMove).Any(x=> x.MoveDate == this.dateMove && x.Amount == this.amountMove && x.CardID == this.CardID))
                return;
            switch (Type)
            {
                case "Move":
                    await new CreditCardMove
                    {
                        CardID = this.CardID,
                        MoveDate = this.dateMove,
                        Amount = this.amountMove
                    }.InsertToDB();
                    break;
                case "Reward":
                    await new CreditCardReward
                    {
                        CardID = this.CardID,
                        MoveDate = this.dateMove,
                        Amount = this.amountMove
                    }.InsertToDB();
                    break;
            }
            
            await CCollection.UpdateFromDB();
            dtg.ItemsSource = (App.Current.Resources[Collection] as IItemCollection).GetGenericCollection();
        }

        private async void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            if (btnInitialize.Content.ToString() == "Initialize table")
            {
                await (App.Current.Resources["CreditCards"] as CreditCardCollection).Initialize();
                await (App.Current.Resources[Collection] as IItemCollection).Initialize();
                btnInitialize.Content = "Update table";
            }
            else
                await (App.Current.Resources[Collection] as IItemCollection).UpdateFromDB();
            dtg.ItemsSource = (App.Current.Resources[Collection] as IItemCollection).GetGenericCollection();
        }
    }
}
