using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Itemtemplates
{
    class Investment : ItemWInsert
    {
        public int? ID { get; set; }
        public string AggregateName { get; set; }
        public int? InvAggregate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Decimal? InvAmount { get; set; }
        protected override string InsertCmd => updateable ?
            Queries.QueryManager["Update_Invest"] :
            Queries.QueryManager["Insert_Invest"];
        protected override bool insertable => InvAmount != null && FromDate != null && InvAggregate != null;
        protected override void FillInsertCommand(SqlCommand command)
        {
            if (!updateable)
            {
                command.Parameters.AddWithValue("@invAgg", InvAggregate);
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
            int r = await base.InsertToDB();
            if (r > 0)
            {
                var Aux = await SharedFunctions.getSingleRow(@"SELECT TOP (1) [Id]
  FROM [Koala].[dbo].[Inv]
  ORDER BY ID DESC", new Dictionary<string, object>());
                ID = Aux["Id"] as int?;
            }
            updateable = false;

            return r;
        }

        public override void LoadFromDtItem(System.Data.DataRow dtr)
        {
            ID = dtr["Id"] as int?;
            AggregateName = dtr["AggregateName"].ToString();
            InvAggregate = dtr["InvAggregate"] as int?;
            FromDate = dtr["InvDate"] as DateTime?;
            EndDate = dtr["EndDate"] as DateTime?;
            InvAmount = dtr["InvAmount"] as decimal?;
        }
    }
    class InvestmentCollection : ItemWInsertCollection<Investment>
    {
        protected override string SelectQuerycmd => Queries.QueryManager["Investments"];

        public override void FillSelectQuery(SqlCommand command)
        {
        }
    }
}
