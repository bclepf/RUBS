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

            // Chama a fun��o que busca os estabelecimentos ao carregar a p�gina
            BuscarEstabelecimentos();
        }

        private async void BuscarEstabelecimentos()
        {
            // Obt�m o c�digo do munic�pio selecionado do servi�o
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
                await DisplayAlert("Erro", "C�digo do munic�pio n�o pode ser vazio.", "OK");
            }
        }
    }
}
