using SQLite;
using PillTime.Models;

namespace PillTime.Services
{
    public class PillTimeDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public PillTimeDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Medicine>().Wait();
        }

        public Task<List<Medicine>> GetMedicinesAsync()
        {
            return _database.Table<Medicine>().ToListAsync();
        }

        public async Task SaveMedicineAsync(Medicine medicine)
        {
            if (medicine.Id != 0)
            {
                await _database.UpdateAsync(medicine);
            }
            else
            {
                await _database.InsertAsync(medicine);
            }
        }


        public Task<int> DeleteMedicineAsync(Medicine medicine)
        {
            return _database.DeleteAsync(medicine);
        }
    }
}
