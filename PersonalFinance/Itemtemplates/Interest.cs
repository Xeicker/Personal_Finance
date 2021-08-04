using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Itemtemplates
{
    public class Interest : OnlyInsertItem
    {
        public DateTime? Start { get; set; }
        public DateTime? Stop { get; set; }
        public Decimal? Amount { get; set; }
        public int? AggregateId { get; set; }
        public int? InvId { get; set; }
        protected override string InsertCmd => updateable ?
            Queries.QueryManager["Update_Interest"] :
            Queries.QueryManager["Insert_Interest"];
        protected override bool insertable => Start != null && Stop != null && Amount != null && InvId != null;
        protected override void FillInsertCommand(SqlCommand command)
        {
            if (!updateable)
            {
                command.Parameters.AddWithValue("@start", Start);
                command.Parameters.AddWithValue("@stop", Stop);
                command.Parameters.AddWithValue("@Am", Amount);
                command.Parameters.AddWithValue("@AggId", AggregateId);
                command.Parameters.AddWithValue("@InvId", InvId);
            }
            else
            {

                command.Parameters.AddWithValue("@start", Start);
                command.Parameters.AddWithValue("@InvId", InvId);
                command.Parameters.AddWithValue("@Am", Amount);
            }
        }

    }
    public class InterestCollection : OnlyInsertItemCollection<Interest>
    {
        public override async Task<bool> InsertToDB()
        {
            if (this.Count > 0)
            {
                this[0].Start = (App.Current.Resources["MainInvestment"] as Investment).FromDate;
                this[0].InvId = (App.Current.Resources["MainInvestment"] as Investment).ID;
                this[0].AggregateId = (App.Current.Resources["MainInvestment"] as Investment).InvAggregate;
                if (this[0].Start != null && this[0].InvId != null)
                {
                    for (int i = 1; i < this.Count; i++)
                    {
                        this[i].Start = this[i - 1].Stop;
                        this[i].InvId = this[0].InvId;
                        this[i].AggregateId = this[0].AggregateId;
                    }
                }
                return await base.InsertToDB();
            }
            return false;
        }
    }
}
