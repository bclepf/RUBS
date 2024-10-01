namespace RUBS.Views;
using RUBS.Services;
public partial class MainPage : ContentPage
{
    private readonly ApiService _apiService;

    public MainPage()
    {
        InitializeComponent();
        _apiService = new ApiService();
    }

    // Quando o usuário clicar no botão, esse método será chamado
    private async void BuscarEstabelecimentosClicked(object sender, EventArgs e)
    {
        string codigoMunicipio = CodigoMunicipioEntry.Text;

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
            await DisplayAlert("Erro", "Por favor, insira o código do município", "OK");
        }
    }
}
