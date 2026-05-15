import 'api_client.dart';
import '../data/api_models.dart';

class PriceService {
  final ApiClient _apiClient;

  PriceService({ApiClient? apiClient}) : _apiClient = apiClient ?? ApiClient();

  // Tüm varlık tiplerini tek bir symbol→fiyat map'ine düzleştirir.
  Future<Map<String, double>> fetchPrices() async {
    final PriceResponse response = await _apiClient.getPrices();
    final Map<String, double> prices = {};

    for (final c in response.currencies) {
      prices[c.symbol] = c.priceInTry;
    }

    for (final c in response.cryptos) {
      prices[c.symbol] = c.priceInTry;
    }

    // Backend enum adı döndürüyor ("Gold", "Silver"), mock holdings XAU/XAG kullanıyor.
    const metalSymbols = {
      'Gold':      'XAU',
      'Silver':    'XAG',
      'Platinum':  'XPT',
      'Palladium': 'XPD',
    };
    for (final m in response.preciousMetals) {
      final symbol = metalSymbols[m.metalType] ?? m.metalType;
      prices[symbol] = m.pricePerGram;
    }

    for (final s in response.stocks) {
      prices[s.symbol] = s.priceInTry;
    }

    return prices;
  }
}
