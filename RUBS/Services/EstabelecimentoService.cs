﻿using RUBS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RUBS.Services
{
    public class EstabelecimentoService
    {
        private readonly DatabaseService _databaseService;

        public EstabelecimentoService()
        {
            _databaseService = new DatabaseService();
        }

        public Task<List<EstabelecimentosDB>> ObterEstabelecimentosSalvosAsync()
        {
            return _databaseService.GetEstabelecimentosAsync();
        }

        public async Task SalvarEstabelecimentosAsync(List<EstabelecimentosDB> estabelecimentos)
        {
            foreach (var estabelecimento in estabelecimentos)
            {
                await _databaseService.SaveEstabelecimentoAsync(estabelecimento);
            }
        }
    }
}
