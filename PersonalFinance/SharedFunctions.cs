using PersonalFinance.Itemtemplates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance
{
    static class SharedFunctions
    {
        public static Date AppDate {
            get => App.Current.Resources["SelectedDate"] as Date;
            set => App.Current.Resources["SelectedDate"] = value; }
        public static DateTime? RevenueFromDate { get; set; }
        public static DateTime? RevenueToDate { get; set; }
        public static decimal? OverallRevenue { get; set; }
        public static async Task FillObsCollection<T>(string cmdstr, ItemCollection<T> Oc) where T : IItem, new()
        {

            string ConStr = ConfigurationManager.ConnectionStrings["FinanceDb"].ConnectionString;
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                SqlCommand cmd = new SqlCommand(cmdstr, con);
                Oc.FillSelectQuery(cmd);
                cmd.CommandTimeout = 120;

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("Summary");
                await Task.Run(() => sda.Fill(dt));
                Oc.Clear();
                foreach (DataRow DR in dt.Rows)
                {
                    Oc.Add(new T());
                    Oc.Last().LoadFromDtItem(DR);
                }
            }
        }
            public static async Task FillObsCollection<T>(string cmdstr, ObservableCollection<T> Oc) where T : IItem, new()
            {

                string ConStr = ConfigurationManager.ConnectionStrings["FinanceDb"].ConnectionString;
                using (SqlConnection con = new SqlConnection(ConStr))
                {
                    SqlCommand cmd = new SqlCommand(cmdstr, con);

                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable("Summary");
                    await Task.Run(() => sda.Fill(dt));
                    Oc.Clear();
                    foreach (DataRow DR in dt.Rows)
                    {
                        Oc.Add(new T());
                        Oc.Last().LoadFromDtItem(DR);
                    }
                }
            }
            public static async Task<DataRow> getSingleRow(string cmdstr,Dictionary<string,object> Parameters) 
        {

            string ConStr = ConfigurationManager.ConnectionStrings["FinanceDb"].ConnectionString;
            using (SqlConnection con = new SqlConnection(ConStr))
            {
                SqlCommand cmd = new SqlCommand(cmdstr, con);
                foreach(var param in Parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable("Result");
                await Task.Run(() => sda.Fill(dt));
                return dt.Rows.OfType<DataRow>().FirstOrDefault();
            }
        }
        public static T MinElement<T>(this IEnumerable<T> collection, Func<T,decimal> selector)
        {
            decimal min = selector(collection.First());
            T el = collection.First();
            foreach(T item in collection.Skip(1))
            {
                var val = selector(item);
                if (val< min)
                {
                    min = val;
                    el = item;
                }
            }
            return el;
        }
    }
}
