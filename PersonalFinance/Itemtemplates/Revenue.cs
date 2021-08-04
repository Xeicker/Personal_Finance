using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Itemtemplates
{
    class Revenue:IItem
    {
        public string Name { get; set; }
        public decimal Value { get; set; }

        public void LoadFromDtItem(DataRow dtr)
        {
        }
    }
    class RevAuxiliar : IItem
    {
        public string Name { get; set; }
        public DateTime? InvDate { get; set; }
        public decimal? Amount { get; set; }
        public void LoadFromDtItem(DataRow dtr)
        {
            Name = dtr["Name"].ToString();
            InvDate = dtr["Date"] as DateTime?;
            Amount = dtr["Amount"]as decimal?;
        }
    }
    class RevAuxiliarCollection : ItemCollection<RevAuxiliar>
    {
        private string selectedQuerycmd;
        public RevAuxiliarCollection(string queryName) : base(){
            selectedQuerycmd = Queries.QueryManager[queryName];
        }
        protected override string SelectQuerycmd => selectedQuerycmd;

        public override void FillSelectQuery(SqlCommand command)
        {
            command.Parameters.AddWithValue("@fromd", SharedFunctions.RevenueFromDate);
            command.Parameters.AddWithValue("@tod", SharedFunctions.RevenueToDate);
        }
    }
    class RevenueCollection : ObservableCollection<Revenue>
    {
        private decimal? overallRevenue;
        public decimal? OverallRevenue
        {
            get => overallRevenue;
            set
            {
                if (overallRevenue != value)
                {
                    overallRevenue = value;
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("OverallRevenue"));
                }
            }
        }
        RevAuxiliarCollection InvsA, InvsB, InvsCValue, OverallA, OverallB, OverallCValue;
        IEnumerable<RevAuxiliar> itemsInvs => InvsA.Concat(InvsB).Concat(InvsCValue);
        IEnumerable<RevAuxiliar> itemsOverAll => OverallA.Concat(OverallB).Concat(OverallCValue);
        public RevenueCollection()
        {
            InvsA = new RevAuxiliarCollection("InvsA");
            InvsB = new RevAuxiliarCollection("InvsB");
            InvsCValue = new RevAuxiliarCollection("InvsCValue");
            OverallA = new RevAuxiliarCollection("OverallA");
            OverallB = new RevAuxiliarCollection("OverallB");
            OverallCValue = new RevAuxiliarCollection("OverallCValue");
        }
        public async Task Update()
        {
            this.Clear();
            await InvsA.UpdateFromDB();
            await InvsB.UpdateFromDB();
            await InvsCValue.UpdateFromDB();
            await foreach(var item in Calculate(itemsInvs))
            {
                Add(item);
            }
            await OverallA.UpdateFromDB();
            await OverallB.UpdateFromDB();
            await OverallCValue.UpdateFromDB(); 
            await foreach (var item in Calculate(itemsOverAll))
            {
               OverallRevenue = item.Value;
            }
        }
        public async Task Initialize()
        {
            if (Count>0)
            {
                await Update();
            }
        }
        private async IAsyncEnumerable<Revenue> Calculate(IEnumerable<RevAuxiliar> collection)
        {
            var names = collection.Select(x => x.Name).Distinct();
            Polynomial P;
            foreach(var name in names)
            {
                P = new Polynomial(collection.Where(x=>x.Name==name).Select(x=>(x.Amount??0,(int)(SharedFunctions.RevenueToDate-x.InvDate).Value.TotalDays)));
                yield return new Revenue { Name = name, Value = (decimal)Math.Pow((double)await P.FindRoot(),365)-1 };
            }
        }
        public void AddAsync()
        {
        }
    }
}
