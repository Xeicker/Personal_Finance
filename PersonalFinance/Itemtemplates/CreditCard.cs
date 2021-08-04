using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace PersonalFinance.Itemtemplates
{
    class CreditCard : IItem
    {
        public int ID { get; set; }
        public string Bank { get; set; }
        public void LoadFromDtItem(DataRow dtr)
        {
            ID = (int)dtr["Id"];
            Bank = dtr["Bank"].ToString();
        }
    }
    class CreditCardCollection : ItemCollection<CreditCard>
    {
        protected override string SelectQuerycmd => Queries.QueryManager["CreditCards"];

        public override void FillSelectQuery(SqlCommand command)
        {
        }
    }
}
