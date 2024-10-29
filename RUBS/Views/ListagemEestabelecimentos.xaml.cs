using RUBS.Services;
using Microsoft.Maui.Controls;

namespace RUBS.Views
{
    public partial class ListagemEestabelecimentos : ContentPage
    {
        private readonly ApiService _apiService;

        public ListagemEestabelecimentos()
        {
            InitializeComponent();
            _apiService = new ApiService();

            // Chama a função que busca os estabelecimentos ao carregar a página
            BuscarEstabelecimentos();
        }

        private async void BuscarEstabelecimentos()
        {
            // Obtém o código do município selecionado do serviço
            var codigoMunicipio = MunicipioService.Instance.CodigoMunicipioSelecionado;

            if (!string.IsNullOrEmpty(codigoMunicipio))
            {
                try
                {
                    // Realiza a busca paginada
                    await _apiService.GetEstabelecimentosPaginadosAsync(codigoMunicipio);

                    // Carregar estabelecimentos salvos no banco e exibir na lista
                    var estabelecimentos = await _apiService._databaseService.GetEstabelecimentosAsync();

                    if (estabelecimentos != null)
                    {
                        ListaEstabelecimentos.ItemsSource = estabelecimentos;
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Erro", "Código do município não pode ser vazio.", "OK");
            }
        }
    }
}
