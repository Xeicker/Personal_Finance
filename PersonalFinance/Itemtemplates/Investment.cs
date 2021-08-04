using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Itemtemplates
{
    class Investment : OnlyInsertItem
    {
        public int? ID { get; set; }
        public int? InvAggregate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Decimal? InvAmount { get; set; }
        protected override string InsertCmd => updateable?
            Queries.QueryManager["Update_Invest"] :
            Queries.QueryManager["Insert_Invest"];
        protected override bool insertable => InvAmount!=null && FromDate!=null && InvAggregate!=null;
        protected override void FillInsertCommand(SqlCommand command)
        {
            if (!updateable)
            {
                command.Parameters.AddWithValue("@invAgg",InvAggregate);
                command.Parameters.AddWithValue("@invD", FromDate);
                command.Parameters.AddWithValue("@invAm", InvAmount);
                if (EndDate == null)
                    command.Parameters.AddWithValue("@endD", DBNull.Value);
                else
                    command.Parameters.AddWithValue("@endD", EndDate);
            }
            else
            {
                command.Parameters.AddWithValue("@invAm", InvAmount);
                command.Parameters.AddWithValue("@Id", ID);
            }
        }

        public override async Task<int> InsertToDB()
        {
            int r =  await base.InsertToDB();
            if (r > 0)
            {
                var Aux = await SharedFunctions.getSingleRow(@"SELECT TOP (1) [Id]
  FROM [Koala].[dbo].[Inv]
  ORDER BY ID DESC", new Dictionary<string, object>());
                ID = Aux["Id"] as int?;
            }
            return r;
        }
    }
}
