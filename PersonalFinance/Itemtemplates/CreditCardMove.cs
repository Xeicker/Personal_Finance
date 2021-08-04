using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace PersonalFinance.Itemtemplates
{
    public class CreditCardMove : ItemWInsert
    {
        public int? ID { get; set; }
        public int? CardID { get; set; }
        public string CardName { get; set; }
        public DateTime? MoveDate { get; set; }
        public decimal? Amount { get; set; }
        protected override string InsertCmd => updateable?
            Queries.QueryManager["Update_CCMove"] :
            Queries.QueryManager["Insert_CCMove"];
        protected override bool insertable => CardID!=null && MoveDate!=null && Amount!=null;
        public override void LoadFromDtItem(DataRow dtr)
        {
            ID = dtr["Id"] as int?;
            CardID = dtr["CardId"] as int?;
            CardName = (App.Current.Resources["CreditCards"] as CreditCardCollection)?.FirstOrDefault(x => x.ID == this.CardID)?.Bank;
            MoveDate = dtr["MovDate"] as DateTime?;
            Amount = dtr["Movement"] as decimal?;
        }
        protected override void FillInsertCommand(SqlCommand command)
        {
            if (updateable)
            {
                command.Parameters.AddWithValue("@id",ID);
                command.Parameters.AddWithValue("@amnt", Amount);
            }
            else
            {
                command.Parameters.AddWithValue("@cardId", CardID);
                command.Parameters.AddWithValue("@mDate", MoveDate);
                command.Parameters.AddWithValue("@amnt", Amount);
            }
        }
    }
    class CreditCardReward : CreditCardMove
    {
        protected override string InsertCmd => updateable ?
            Queries.QueryManager["Update_CCReward"] :
            Queries.QueryManager["Insert_CCReward"];
        public override void LoadFromDtItem(DataRow dtr)
        {
            ID = dtr["Id"] as int?;
            CardID = dtr["CardId"] as int?;
            CardName = (App.Current.Resources["CreditCards"] as CreditCardCollection)?.FirstOrDefault(x => x.ID == this.CardID)?.Bank;
            MoveDate = dtr["Dateapp"] as DateTime?;
            Amount = dtr["Amount"] as decimal?;
        }
    }
    class CreditCardMoveCollection : ItemWInsertCollection<CreditCardMove>
    {
        protected override string SelectQuerycmd => Queries.QueryManager["CCMove"];

        public override void FillSelectQuery(SqlCommand command)
        {
        }
    }
    class CreditCardRewardCollection : ItemWInsertCollection<CreditCardReward>
    {
        protected override string SelectQuerycmd => Queries.QueryManager["Rewards"];

        public override void FillSelectQuery(SqlCommand command)
        {
        }
    }
}
