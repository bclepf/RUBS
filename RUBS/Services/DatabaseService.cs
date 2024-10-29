using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RUBS.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseService()
        {
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RUBS.db3");
            _database = new SQLiteAsyncConnection(databasePath);
            _database.CreateTableAsync<estabelecimentos>().Wait();
        }

        // Inserir ou atualizar os estabelecimentos
        public Task<int> SaveEstabelecimentoAsync(estabelecimentos estabelecimento)
        {
            return _database.InsertOrReplaceAsync(estabelecimento);
        }

        // Retorna todos os estabelecimentos
        public Task<List<estabelecimentos>> GetEstabelecimentosAsync()
        {
            return _database.Table<estabelecimentos>().ToListAsync();
        }

        public async Task RemoverEstabelecimetnosAsync()
        {
            await _database.DeleteAllAsync<estabelecimentos>();
        }
    }
}
