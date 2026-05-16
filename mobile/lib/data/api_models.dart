class PriceResponse {
  final String baseCurrency;
  final DateTime timestamp;
  final List<CurrencyPrice> currencies;
  final List<CryptoPrice> cryptos;
  final List<PreciousMetalPrice> preciousMetals;
  final List<StockPrice> stocks;

  const PriceResponse({
    required this.baseCurrency,
    required this.timestamp,
    required this.currencies,
    required this.cryptos,
    required this.preciousMetals,
    required this.stocks,
  });

  factory PriceResponse.fromJson(Map<String, dynamic> json) => PriceResponse(
        baseCurrency: json['baseCurrency'] as String,
        timestamp: DateTime.parse(json['timestamp'] as String),
        currencies: (json['currencies'] as List)
            .map((e) => CurrencyPrice.fromJson(e as Map<String, dynamic>))
            .toList(),
        cryptos: (json['cryptos'] as List)
            .map((e) => CryptoPrice.fromJson(e as Map<String, dynamic>))
            .toList(),
        preciousMetals: (json['preciousMetals'] as List)
            .map((e) => PreciousMetalPrice.fromJson(e as Map<String, dynamic>))
            .toList(),
        stocks: (json['stocks'] as List)
            .map((e) => StockPrice.fromJson(e as Map<String, dynamic>))
            .toList(),
      );
}

class CurrencyPrice {
  final String symbol;
  final double buyingPrice;
  final double sellingPrice;
  final int unit;

  const CurrencyPrice({
    required this.symbol,
    required this.buyingPrice,
    required this.sellingPrice,
    required this.unit,
  });

  // TCMB bazı dövizleri 100'lük birimle verir (JPY gibi).
  // Birim başına gerçek TL fiyatı için ikiye bölüp ortalamasını alıyoruz.
  double get priceInTry => (buyingPrice + sellingPrice) / 2 / unit;

  factory CurrencyPrice.fromJson(Map<String, dynamic> json) => CurrencyPrice(
        symbol: json['currencyCode'] as String,
        buyingPrice: (json['buyingPrice'] as num).toDouble(),
        sellingPrice: (json['sellingPrice'] as num).toDouble(),
        unit: (json['unit'] as num).toInt(),
      );
}

class CryptoPrice {
  final String symbol;
  final double priceInUsd;
  final double priceInTry;

  const CryptoPrice({
    required this.symbol,
    required this.priceInUsd,
    required this.priceInTry,
  });

  factory CryptoPrice.fromJson(Map<String, dynamic> json) => CryptoPrice(
        symbol: json['symbol'] as String,
        priceInUsd: (json['priceInUsd'] as num).toDouble(),
        priceInTry: (json['priceInTry'] as num).toDouble(),
      );
}

class PreciousMetalPrice {
  final String metalType;
  final double pricePerGram;
  final double pricePerOunce;

  const PreciousMetalPrice({
    required this.metalType,
    required this.pricePerGram,
    required this.pricePerOunce,
  });

  factory PreciousMetalPrice.fromJson(Map<String, dynamic> json) => PreciousMetalPrice(
        metalType: json['metalType'] as String,
        pricePerGram: (json['pricePerGram'] as num).toDouble(),
        pricePerOunce: (json['pricePerOunce'] as num).toDouble(),
      );
}

class StockPrice {
  final String symbol;
  final String name;
  final String exchange;
  final double priceInUsd;
  final double priceInTry;

  const StockPrice({
    required this.symbol,
    required this.name,
    required this.exchange,
    required this.priceInUsd,
    required this.priceInTry,
  });

  factory StockPrice.fromJson(Map<String, dynamic> json) => StockPrice(
        symbol: json['symbol'] as String,
        name: json['name'] as String,
        exchange: json['exchange'] as String,
        priceInUsd: (json['priceInUsd'] as num).toDouble(),
        priceInTry: (json['priceInTry'] as num).toDouble(),
      );
}
