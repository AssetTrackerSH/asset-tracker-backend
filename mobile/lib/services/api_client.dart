import 'dart:convert';
import 'package:http/http.dart' as http;
import '../data/api_models.dart';

class ApiClient {
  static const baseUrl = 'http://localhost:5238';

  final http.Client _client;

  ApiClient({http.Client? client}) : _client = client ?? http.Client();

  Future<PriceResponse> getPrices() async {
    final uri = Uri.parse('$baseUrl/api/prices?baseCurrency=TRY');
    final response = await _client.get(uri);

    if (response.statusCode == 200) {
      final json = jsonDecode(response.body) as Map<String, dynamic>;
      return PriceResponse.fromJson(json);
    }

    throw Exception('Fiyatlar alınamadı. HTTP ${response.statusCode}');
  }
}
