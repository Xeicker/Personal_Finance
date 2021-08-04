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
    public class Date : ItemWInsert
    {
        public int? ID;
        public DateTime? DateData { get; set; }
        public string Datestr { get => (DateData ?? DateTime.MinValue).ToString("dd/MM/yyyy"); set => DateData = DateTime.Parse(value); }

        protected override bool insertable => DateData!=null && ID!=null;

        protected override string InsertCmd => Queries.QueryManager["Insert_Date"];

        protected override void FillInsertCommand(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@date", DateData);
            cmd.Parameters.AddWithValue("@id", ID);
        }

        public override void LoadFromDtItem(DataRow dtr)
        {
            ID = dtr["ID"] as int?;
            DateData = dtr["GivenDate"] as DateTime?;
        }

    }
    public class DateCollection : ItemWInsertCollection<Date>
    {
        protected override string SelectQuerycmd => Queries.QueryManager["Dates"];


        public override void FillSelectQuery(SqlCommand command)
        {
        }
    }
}
