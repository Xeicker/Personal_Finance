using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Itemtemplates
{
    public class Income : ItemWInsert
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public Decimal? Value { get; set; }
        public int? AggId { get; set; }

        protected override string InsertCmd => updateable?
                Queries.QueryManager["Update_Income"] : 
                Queries.QueryManager["Insert_Income"];

        protected override bool insertable => SharedFunctions.AppDate.ID!=null && Value != null;

        protected override void FillInsertCommand(SqlCommand command)
        {
            command.Parameters.AddWithValue("@dateid",SharedFunctions.AppDate.ID);
            command.Parameters.AddWithValue("@incomeid", ID);
            command.Parameters.AddWithValue("@amount", Value);
        }

        public override void LoadFromDtItem(DataRow dtr)
        {
            ID = (int)dtr["ID"];
            Name = (string)dtr["Income"];
            AggId = (int?)dtr["AggregateID"];
            Value = dtr["IncomeAmount"] as Decimal?;
            updateable = Value != null;            
        }
    }
    public class IncomeCollection : ItemWInsertCollection<Income>
    {
        protected override string SelectQuerycmd => Queries.QueryManager["Incomes"];

        public override void FillSelectQuery(SqlCommand command)
        {
            command.Parameters.AddWithValue("@date", SharedFunctions.AppDate.DateData??DateTime.Today);
        }
    }
}
