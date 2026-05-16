import 'dart:math' as math;
import 'models.dart';

class Portfolio {
  final double total, changeMonth, changeMonthPct, changeYear, changeYearPct,
      changeAllTime, changeAllTimePct, invested;
  final String firstDeposit;
  const Portfolio({
    required this.total, required this.changeMonth, required this.changeMonthPct,
    required this.changeYear, required this.changeYearPct,
    required this.changeAllTime, required this.changeAllTimePct,
    required this.invested, required this.firstDeposit,
  });
}

const portfolio = Portfolio(
  total: 2847394, changeMonth: 84612, changeMonthPct: 3.06,
  changeYear: 412840, changeYearPct: 16.97,
  changeAllTime: 1294810, changeAllTimePct: 83.4,
  invested: 1552584, firstDeposit: '2019-03',
);

const allocation = <Allocation>[
  Allocation('stocks', 'Hisse Senetleri', 1082010, 38.0),
  Allocation('crypto', 'Kripto',           626427, 22.0),
  Allocation('fx',     'Döviz',            512531, 18.0),
  Allocation('gold',   'Altın & Emtia',    398635, 14.0),
  Allocation('cash',   'Yastık Altı',      227791, 8.0),
];

const platforms = <Platform>[
  Platform('isyat',   'İş Yatırım',  'Aracı kurum',  8, '2 dk önce',  'ok'),
  Platform('garanti', 'Garanti BBVA','Banka',        3, '5 dk önce',  'ok'),
  Platform('binance', 'Binance',     'Kripto borsa', 6, '1 dk önce',  'ok'),
  Platform('paribu',  'Paribu',      'Kripto borsa', 2, '14 dk önce', 'ok'),
  Platform('midas',   'Midas',       'ABD hisseler', 5, '8 dk önce',  'ok'),
  Platform('manual',  'Yastık Altı', 'Manuel giriş', 4, 'Dün',        'manual'),
];

const holdings = <Holding>[
  Holding(id: 'THYAO', symbol: 'THYAO', name: 'Türk Hava Yolları', klass: 'stocks', sub: 'BIST',   qty: 850,  price: 312.40,   value: 265540, costAvg: 198.20, change1d: 1.84,  change1y: 32.4, platform: 'isyat'),
  Holding(id: 'ASELS', symbol: 'ASELS', name: 'Aselsan',           klass: 'stocks', sub: 'BIST',   qty: 1200, price: 89.65,    value: 107580, costAvg: 62.10,  change1d: -0.42, change1y: 28.1, platform: 'isyat'),
  Holding(id: 'SISE',  symbol: 'SISE',  name: 'Şişe Cam',          klass: 'stocks', sub: 'BIST',   qty: 2400, price: 48.92,    value: 117408, costAvg: 39.80,  change1d: 0.81,  change1y: 12.6, platform: 'isyat'),
  Holding(id: 'AAPL',  symbol: 'AAPL',  name: 'Apple Inc.',        klass: 'stocks', sub: 'NASDAQ', qty: 42,   price: 7892.10,  value: 331468, costAvg: 4210,   change1d: 0.22,  change1y: 22.4, platform: 'midas'),
  Holding(id: 'NVDA',  symbol: 'NVDA',  name: 'NVIDIA Corp.',      klass: 'stocks', sub: 'NASDAQ', qty: 18,   price: 14820,    value: 266760, costAvg: 6820,   change1d: 2.41,  change1y: 84.2, platform: 'midas'),
  Holding(id: 'BTC',   symbol: 'BTC',   name: 'Bitcoin',           klass: 'crypto', sub: 'Kripto', qty: 0.412,price: 1124800,  value: 463418, costAvg: 612000, change1d: -1.20, change1y: 48.6, platform: 'binance'),
  Holding(id: 'ETH',   symbol: 'ETH',   name: 'Ethereum',          klass: 'crypto', sub: 'Kripto', qty: 3.84, price: 32140,    value: 123418, costAvg: 24800,  change1d: -0.80, change1y: 21.4, platform: 'binance'),
  Holding(id: 'SOL',   symbol: 'SOL',   name: 'Solana',            klass: 'crypto', sub: 'Kripto', qty: 48,   price: 832.10,   value: 39941,  costAvg: 420,    change1d: 3.20,  change1y: 62.1, platform: 'paribu'),
  Holding(id: 'USD',   symbol: 'USD',   name: 'Amerikan Doları',   klass: 'fx',     sub: 'Döviz',  qty: 6800, price: 42.18,    value: 286824, costAvg: 28.40,  change1d: 0.18,  change1y: 28.4, platform: 'garanti'),
  Holding(id: 'EUR',   symbol: 'EUR',   name: 'Euro',              klass: 'fx',     sub: 'Döviz',  qty: 4200, price: 45.92,    value: 192864, costAvg: 32.10,  change1d: 0.32,  change1y: 26.8, platform: 'garanti'),
  Holding(id: 'GBP',   symbol: 'GBP',   name: 'İngiliz Sterlini',  klass: 'fx',     sub: 'Döviz',  qty: 580,  price: 56.62,    value: 32839,  costAvg: 42.80,  change1d: -0.12, change1y: 22.6, platform: 'garanti'),
  Holding(id: 'XAU',   symbol: 'GRAM',  name: 'Gram Altın',        klass: 'gold',   sub: 'Altın',  qty: 142,  price: 2418.40,  value: 343413, costAvg: 1620,   change1d: 0.42,  change1y: 38.1, platform: 'isyat'),
  Holding(id: 'XAU2',  symbol: 'ÇEYREK',name: 'Çeyrek Altın',      klass: 'gold',   sub: 'Altın',  qty: 28,   price: 1972.50,  value: 55230,  costAvg: 1340,   change1d: 0.38,  change1y: 37.6, platform: 'manual'),
  Holding(id: 'CASH_USD', symbol: 'USD', name: 'Fiziki Dolar',     klass: 'cash',   sub: 'Yastık Altı', qty: 3200, price: 42.18,  value: 134976, costAvg: 24.50,  change1d: 0.18, change1y: 28.4, platform: 'manual'),
  Holding(id: 'CASH_AU',  symbol: 'AU',  name: 'Fiziki Gram Altın',klass: 'cash',   sub: 'Yastık Altı', qty: 38.5, price: 2418.40,value: 93108,  costAvg: 1480,   change1d: 0.42, change1y: 38.1, platform: 'manual'),
];

const goals = <Goal>[
  Goal('emeklilik', 'Emeklilik',         2045, 25000000, 2847394, 18500, 'Yıllık %12 reel getiri varsayımı ile hedefe 19 yıl kaldı.'),
  Goal('ev',        'Ev Peşinatı',       2028,  3500000, 1240800, 24000, '2,3 yıl içinde hedefte. Mevcut tempoyla 4 ay erken tamamlanır.'),
  Goal('cocuk',     'Çocuk Eğitim Fonu', 2038,  8000000,  612400,  8500, '14 yıl vadeli. Aylık katkı %15 artırılırsa hedefe 2 yıl önce ulaşılır.'),
];

const news = <NewsItem>[
  NewsItem('n1', 'Hisse',    'Reuters',   '2 saat', 'BIST 100 endeksi son 6 ayın en yüksek seviyesinde kapattı', 'Endeks; bankacılık ve havacılık liderliğinde yükselişini sürdürüyor. Yabancı girişi 3 ay üst üste pozitif.'),
  NewsItem('n2', 'Kripto',   'Bloomberg', '4 saat', 'ETF girişleri Bitcoin\'i tarihi seviyelere yaklaştırdı', 'Spot Bitcoin ETF\'lerine son 30 günde 8,4 milyar dolar net giriş gerçekleşti. Uzun vadeli sahiplik oranı artıyor.'),
  NewsItem('n3', 'Makro',    'TCMB',      '1 gün',  'Merkez Bankası faiz kararını açıkladı', 'Politika faizi 250 baz puan indirildi. Karar piyasa beklentilerinin üzerinde sayıldı; tahvil getirileri geriledi.'),
  NewsItem('n4', 'Emtia',    'FT',        '1 gün',  'Altın yıllık bazda dolar karşısında %18 değerlendi', 'Merkez bankalarının alımları rekor seviyede. Stratejistler portföyde %10-15 altın ağırlığı öneriyor.'),
  NewsItem('n5', 'Strateji', 'Anadolu',   '2 gün',  '"Dolar maliyet ortalaması" stratejisi 2025\'te öne çıkıyor', 'Düzenli ve eşit aralıklı alım yapan yatırımcılar, zamanlama yapanlara göre son 5 yılda ortalama %14 daha iyi sonuç aldı.'),
];

const activity = <Activity>[
  Activity('t1', 'buy',  'NVDA', 59280, 'Bugün',   'midas',   null),
  Activity('t2', 'div',  'AAPL', 1284,  'Dün',     'midas',   'Temettü'),
  Activity('t3', 'buy',  'GRAM', 19347, '3 gün',   'isyat',   null),
  Activity('t4', 'sync', null,   null,  '4 gün',   'binance', 'Otomatik senkron'),
  Activity('t5', 'sell', 'SISE', 9700,  '1 hafta', 'isyat',   null),
  Activity('t6', 'buy',  'BTC',  28120, '2 hafta', 'binance', null),
];

/// Deterministik psödo-rastgele zaman serisi üretir.
List<double> generateSeries(int n, double start, double end, double volatility, int seed) {
  final out = <double>[];
  var v = start;
  final drift = math.pow(end / start, 1 / (n - 1)).toDouble();
  var s = seed;
  double rand() {
    s = (s * 9301 + 49297) % 233280;
    return s / 233280;
  }
  for (var i = 0; i < n; i++) {
    v = v * drift * (1 + (rand() - 0.5) * volatility);
    out.add(v);
  }
  out[out.length - 1] = end;
  return out;
}

Map<String, List<double>> portfolioSeries() => {
  '1A':  generateSeries(30, portfolio.total - portfolio.changeMonth,   portfolio.total, 0.012, 11),
  '1Y':  generateSeries(52, portfolio.total - portfolio.changeYear,    portfolio.total, 0.025, 17),
  '5Y':  generateSeries(60, portfolio.total / 2.4,                     portfolio.total, 0.045, 23),
  'TÜM': generateSeries(72, portfolio.invested,                        portfolio.total, 0.055, 29),
};

Map<String, List<double>> seriesFor(Holding h) => {
  '1A':  generateSeries(30, h.value * 0.94,         h.value, 0.018, 31),
  '1Y':  generateSeries(52, h.value * 0.78,         h.value, 0.04,  37),
  '5Y':  generateSeries(60, h.costAvg * h.qty,      h.value, 0.06,  43),
  'TÜM': generateSeries(72, h.costAvg * h.qty * 0.6,h.value, 0.07,  47),
};
