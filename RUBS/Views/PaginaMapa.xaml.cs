using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using SQLite;
using Microsoft.Extensions.Logging;

namespace RUBS.Views;

public partial class PaginaMapa : ContentPage
{
    private SQLiteConnection _db;
    private ILogger<PaginaMapa> _logger; // Alterar para ser inicializado depois

    // Construtor sem par�metros
    public PaginaMapa()
    {
        InitializeComponent();
        InitializeDatabase();
    }

    // M�todo para inicializar o logger ap�s a cria��o
    public void SetLogger(ILogger<PaginaMapa> logger)
    {
        _logger = logger;
    }

    private void InitializeDatabase()
    {
        // Conectar ao banco de dados (ajuste o caminho conforme necess�rio)
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RUBS.db3");
        _db = new SQLiteConnection(dbPath);
        _db.CreateTable<EstabelecimentosDB>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var geolocationRequest = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
        var location = await Geolocation.GetLastKnownLocationAsync();

        if (location != null)
        {
            _logger?.LogInformation($"Movendo mapa para a localiza��o do usu�rio: Latitude {location.Latitude}, Longitude {location.Longitude}");
            map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(500)));
        }

        // Carregar os estabelecimentos do banco de dados
        var estabelecimentosList = _db.Table<EstabelecimentosDB>().ToList();

        if(map.Pins.Count != estabelecimentosList.Count) { 
            map.Pins.Clear();

            foreach (var estabelecimento in estabelecimentosList)
            {
                if (estabelecimento.latitude_estabelecimento_decimo_grau.HasValue && estabelecimento.longitude_estabelecimento_decimo_grau.HasValue)
                {
                    var pin = new Pin
                    {
                        Label = estabelecimento.nome_fantasia,
                        Address = $"{estabelecimento.endereco_estabelecimento}, {estabelecimento.numero_estabelecimento}, {estabelecimento.bairro_estabelecimento}",
                        Location = new Location(estabelecimento.latitude_estabelecimento_decimo_grau.Value, estabelecimento.longitude_estabelecimento_decimo_grau.Value)
                    };

                    map.Pins.Add(pin);
                    _logger?.LogInformation($"PIN criado: {pin.Label} - Latitude: {estabelecimento.latitude_estabelecimento_decimo_grau}, Longitude: {estabelecimento.longitude_estabelecimento_decimo_grau}");
                }
                else
                {
                    _logger?.LogWarning($"Estabelecimento {estabelecimento.nome_fantasia} n�o possui coordenadas v�lidas.");
                }
            }
        }
        else
        {
            _logger?.LogInformation("Os PINs j� est�o carregados no mapa");
        }

        _logger?.LogInformation($"Total de PINs adicionados: {map.Pins.Count}");
    }

    private async void Pin_MarkerClicked(object sender, PinClickedEventArgs e)
    {
        var pinInfo = (Pin)sender;
        await DisplayAlert("Detalhes do Estabelecimento", pinInfo.Address, "Ok");
    }
}
