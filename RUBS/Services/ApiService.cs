using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using RUBS.Services;

namespace RUBS.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseService _databaseService;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _databaseService = new DatabaseService();
        }

        // Novo método para fazer a busca com código de município e controle de paginação
        public async Task GetEstabelecimentosPaginadosAsync(string codigoMunicipio)
        {
            int offset = 1;
            int limit = 1;
            bool continuar = true;

            while (continuar)
            {
                string url = $"https://apidadosabertos.saude.gov.br/cnes/estabelecimentos?codigo_municipio={codigoMunicipio}&limit={limit}&offset={offset}";
                var response = await _httpClient.GetStringAsync(url);
                var result = JsonConvert.DeserializeObject<List<estabelecimentos>>(response);

                // Se a resposta vier vazia, interrompe o loop
                if (result == null || result.Count == 0)
                {
                    continuar = false;
                }
                else
                {
                    // Salva os estabelecimentos no banco
                    foreach (var estabelecimento in result)
                    {
                        await _databaseService.SaveEstabelecimentoAsync(estabelecimento);
                    }

                    // Incrementa o offset para a próxima página
                    offset += limit;
                }
            }
        }
    }
}
