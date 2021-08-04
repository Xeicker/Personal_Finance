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
    public interface IItem
    {
        void LoadFromDtItem(DataRow dtr);
    }
    public abstract class OnlyInsertItem
    {
        protected bool updateable;
        protected abstract string InsertCmd { get; }
        private readonly static string ConnectionString = ConfigurationManager.ConnectionStrings["FinanceDb"].ConnectionString;
        protected abstract bool insertable { get; }
        protected abstract void FillInsertCommand(SqlCommand command);
        public virtual async Task<int> InsertToDB()
        {
            if (insertable)
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(InsertCmd, connection))
                    {
                        int r=-1;
                        FillInsertCommand(command);
                        connection.Open();
                        if ((r = await Task.Run(() => command.ExecuteNonQuery())) >= 0)
                        {
                            updateable = true;
                            return r;
                        }
                        else
                            return r;
                    }
                }
            else
                return 0;
        }
    }
    public abstract class ItemWInsert : OnlyInsertItem, IItem
    {
        public abstract void LoadFromDtItem(DataRow dtr);
    }
    public interface IItemCollection
    {
        IEnumerable<object> GetGenericCollection();
        Task UpdateFromDB();
        Task Initialize();
    }
    public interface IOnlyInsertItemCollection
    {
        Task<bool> InsertToDB();
    }
    public interface IItemWInsertCollection:IItemCollection,IOnlyInsertItemCollection
    {}
    public abstract class ItemCollection<T> : ObservableCollection<T>,IItemCollection  where T : IItem, new()
    {   
        protected abstract string SelectQuerycmd { get; }
        public abstract void FillSelectQuery(SqlCommand command);
        public virtual async Task UpdateFromDB()
        {
            await SharedFunctions.FillObsCollection(SelectQuerycmd, this);
        }
        public async Task Initialize()
        {
            if (this.Count == 0)
                await UpdateFromDB();
        }

        public IEnumerable<object> GetGenericCollection()
        {
            foreach(var item in this)
            {
                yield return item;
            }
        }
    }
    public abstract class OnlyInsertItemCollection<T> : ObservableCollection<T>, IOnlyInsertItemCollection where T: OnlyInsertItem
    {
        public virtual async Task<bool> InsertToDB()
        {
            bool success = true;
            foreach (T item in this)
            {
                success &= await item.InsertToDB() >= 0;
            }
            return success;
        }
    }
    public abstract class ItemWInsertCollection<T> : ItemCollection<T>, IItemWInsertCollection where T : ItemWInsert, new()
    {
        public async Task<bool> InsertToDB()
        {
            bool success = true;
            foreach (T item in this)
            {
                success &= await item.InsertToDB()>=0;
            }
            await this.UpdateFromDB();
            return success;
        }
    }
}
