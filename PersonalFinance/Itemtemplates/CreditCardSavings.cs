using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Itemtemplates
{
    class CreditCardSaving
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
    }
    class  CreditCardSavingsCollection: ObservableCollection<CreditCardSaving>
    {

        public async Task Update()
        {
            this.Clear();
            var CCMoves = App.Current.Resources["CCMoves"] as CreditCardMoveCollection;
            await CCMoves.Initialize();
            foreach(var item in Calculate(CCMoves
                .Where(x=>DateTime.Compare(x.MoveDate ?? DateTime.MinValue, SharedFunctions.RevenueFromDate??DateTime.MinValue)>0
                && DateTime.Compare(x.MoveDate ?? DateTime.MinValue, SharedFunctions.RevenueToDate ?? DateTime.MinValue) < 0)))
            {
                Add(item);
            }
        }
        public async Task Initialize()
        {
            if (Count == 0)
            {
                await Update();
            }
        }
        private IEnumerable<CreditCardSaving> Calculate(IEnumerable<CreditCardMove> collection)
        {
            decimal overallRevenue = (App.Current.Resources["Revenues"] as RevenueCollection).OverallRevenue??0;
            var Rewards = App.Current.Resources["Rewards"] as CreditCardRewardCollection;
            var names = collection.Select(x => x.CardName).Distinct();
            Polynomial P;
            decimal reward;
            foreach (var name in names)
            {
                reward = Rewards?.Where(x => x.CardName == name)?.Sum(x => x.Amount)??0;
                P = new Polynomial(collection.Where(x => x.CardName == name).Select(x => (x.Amount ?? 0, (int)(SharedFunctions.RevenueToDate - x.MoveDate).Value.TotalDays)));
                yield return new CreditCardSaving { Name = name, Value = P.Evaluate((decimal)Math.Pow((double)overallRevenue + 1,1.0/365))-P.Evaluate(1) + reward};
            }
        }
    }
}
