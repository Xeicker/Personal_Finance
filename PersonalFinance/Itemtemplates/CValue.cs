using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance.Itemtemplates
{
    public class CValue: ItemWInsert
    {
        public int? ID { get; set; }
        public int? AggID { get; set; }
        public string Name { get; set; }
        public Decimal? Value { get; set; }
        public bool Invest { get; set; }
        protected override string InsertCmd => updateable ?
                Queries.QueryManager["Update_CValue"] :
                Queries.QueryManager["Insert_CValue"];
        protected override bool insertable => SharedFunctions.AppDate.ID != null && Value != null && AggID!=null;

        protected override void FillInsertCommand(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@dateid", SharedFunctions.AppDate.ID);
            cmd.Parameters.AddWithValue("@CValue", Value);
            cmd.Parameters.AddWithValue("@AggId", AggID);
        }

        public override void LoadFromDtItem(DataRow dtr)
        {
            ID = dtr["ID"] as int?;
            AggID = dtr["AggID"] as int?;
            Name = dtr["Name"].ToString();
            Value = (dtr["Value"] as Decimal?);
            Invest = (dtr["Invest"] as bool?)??false;
            updateable = Value != null;

        }
    }
    public class CValueCollection : ItemWInsertCollection<CValue>
    {
        protected override string SelectQuerycmd => Queries.QueryManager["Aggregates"];

        public override void FillSelectQuery(SqlCommand command)
        {
            command.Parameters.AddWithValue("@date", SharedFunctions.AppDate?.DateData ?? DateTime.Today);
        }
    }
}
