import 'package:flutter/material.dart';
import '../theme.dart';
import '../data/mock_data.dart';
import '../data/models.dart';
import '../widgets/primitives.dart';
import '../widgets/line_chart.dart';
import '../widgets/holding_row.dart';
import '../services/price_service.dart';

class PortfolioScreen extends StatefulWidget {
  final void Function(Holding) onOpenAsset;
  final bool privacy;
  const PortfolioScreen({super.key, required this.onOpenAsset, required this.privacy});

  @override
  State<PortfolioScreen> createState() => _PortfolioScreenState();
}

class _PortfolioScreenState extends State<PortfolioScreen> {
  String _range = '1Y';
  bool _showAll = false;
  final _priceService = PriceService();
  late Future<Map<String, double>> _pricesFuture;

  @override
  void initState() {
    super.initState();
    _pricesFuture = _priceService.fetchPrices();
  }

  // Mock holdings listesindeki her varlığa gerçek API fiyatını uygular.
  // Fiyat bulunamazsa (API'de olmayan varlık) mock fiyat korunur.
  List<Holding> _applyPrices(Map<String, double> prices) {
    return holdings.map((h) {
      final realPrice = prices[h.id] ?? h.price;
      return Holding(
        id: h.id,
        symbol: h.symbol,
        name: h.name,
        klass: h.klass,
        sub: h.sub,
        platform: h.platform,
        qty: h.qty,
        price: realPrice,
        value: h.qty * realPrice,
        costAvg: h.costAvg,
        change1d: h.change1d,
        change1y: h.change1y,
      );
    }).toList();
  }

  // Güncel holding değerlerinden varlık sınıfı bazında allocation hesaplar.
  List<Allocation> _calcAllocations(List<Holding> liveHoldings) {
    final total = liveHoldings.fold(0.0, (s, h) => s + h.value);
    if (total == 0) return allocation;

    final Map<String, double> byClass = {};
    for (final h in liveHoldings) {
      byClass[h.klass] = (byClass[h.klass] ?? 0) + h.value;
    }

    const labels = {
      'stocks': 'Hisse Senetleri',
      'crypto': 'Kripto',
      'fx':     'Döviz',
      'gold':   'Altın & Emtia',
      'cash':   'Yastık Altı',
    };

    return byClass.entries
        .where((e) => labels.containsKey(e.key))
        .map((e) => Allocation(e.key, labels[e.key]!, e.value, e.value / total * 100))
        .toList()
      ..sort((a, b) => b.amount.compareTo(a.amount));
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<Map<String, double>>(
      future: _pricesFuture,
      builder: (context, snapshot) {
        final liveHoldings = snapshot.hasData
            ? _applyPrices(snapshot.data!)
            : List<Holding>.from(holdings);

        final total = liveHoldings.fold(0.0, (s, h) => s + h.value);
        final liveAllocations = snapshot.hasData
            ? _calcAllocations(liveHoldings)
            : List<Allocation>.from(allocation);

        final series = portfolioSeries()[_range]!;
        final rPct = {'1A': portfolio.changeMonthPct, '1Y': portfolio.changeYearPct, '5Y': 64.2, 'TÜM': portfolio.changeAllTimePct}[_range]!;
        final rAmt = {'1A': portfolio.changeMonth, '1Y': portfolio.changeYear, '5Y': 1112000.0, 'TÜM': portfolio.changeAllTime}[_range]!;

        final totalStr = Fmt.tryStr(total);
        final amtStr = Fmt.tryStr(rAmt, sign: true);

        return SingleChildScrollView(
          padding: const EdgeInsets.fromLTRB(18, 8, 18, 110),
          child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            // Hero
            Padding(
              padding: const EdgeInsets.only(top: 4, bottom: 22),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text('TOPLAM SERVET', style: TextStyle(
                  fontSize: 12, letterSpacing: 1.2, fontWeight: FontWeight.w500,
                  color: AppTokens.textDim,
                )),
                const SizedBox(height: 8),
                Text(widget.privacy ? Fmt.mask(totalStr) : totalStr,
                  style: Theme.of(context).textTheme.displayLarge),
                const SizedBox(height: 12),
                Row(children: [
                  SDelta(value: rPct, size: 14),
                  const SizedBox(width: 10),
                  Text(widget.privacy ? Fmt.mask(amtStr) : amtStr,
                    style: const TextStyle(color: AppTokens.textMute, fontSize: 13,
                      fontFeatures: [FontFeature.tabularFigures()])),
                  const SizedBox(width: 8),
                  Text('· $_range', style: const TextStyle(color: AppTokens.textDim, fontSize: 13)),
                  if (snapshot.connectionState == ConnectionState.waiting) ...[
                    const SizedBox(width: 8),
                    const SizedBox(
                      width: 10, height: 10,
                      child: CircularProgressIndicator(strokeWidth: 1.5, color: AppTokens.textDim),
                    ),
                  ],
                  if (snapshot.hasError) ...[
                    const SizedBox(width: 8),
                    Tooltip(
                      message: 'Fiyatlar alınamadı, mock veri gösteriliyor',
                      child: const Icon(Icons.warning_amber_rounded, size: 14, color: AppTokens.danger),
                    ),
                  ],
                ]),
              ]),
            ),
            SLineChart(data: series, color: AppTokens.accent),
            const SizedBox(height: 16),
            SRangeSelector(
              ranges: const ['1A', '1Y', '5Y', 'TÜM'],
              value: _range,
              onChange: (v) => setState(() => _range = v),
            ),
            const SizedBox(height: 28),

            _TrioStats(privacy: widget.privacy),

            const SizedBox(height: 28),
            const SSectionLabel(label: 'Varlık Dağılımı', right: '5 sınıf'),
            SCard(child: _AllocationContent(privacy: widget.privacy, allocations: liveAllocations)),

            const SizedBox(height: 28),
            SSectionLabel(label: 'Varlıklarım', right: '${liveHoldings.length} varlık'),
            SCard(
              padding: const EdgeInsets.symmetric(horizontal: 18),
              child: Column(children: [
                for (final h in _showAll ? liveHoldings : liveHoldings.take(8))
                  SHoldingRow(h: h, privacy: widget.privacy, onTap: () => widget.onOpenAsset(h)),
                GestureDetector(
                  onTap: () => setState(() => _showAll = !_showAll),
                  child: Padding(
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    child: Center(child: Text(
                      _showAll ? 'Daha az göster' : 'Tümünü gör (${liveHoldings.length})',
                      style: const TextStyle(color: AppTokens.accent, fontSize: 13, fontWeight: FontWeight.w500),
                    )),
                  ),
                ),
              ]),
            ),

            const SizedBox(height: 28),
            const SSectionLabel(label: 'Son İşlemler', right: 'Son 2 hafta'),
            SCard(padding: EdgeInsets.zero, child: _ActivityList(privacy: widget.privacy)),
          ]),
        );
      },
    );
  }
}

class _TrioStats extends StatelessWidget {
  final bool privacy;
  const _TrioStats({required this.privacy});

  @override
  Widget build(BuildContext context) {
    final items = [
      ('YATIRILAN',  Fmt.tryStr(portfolio.invested, compact: true), false),
      ('NET KAZANÇ', Fmt.tryStr(portfolio.changeAllTime, compact: true), true),
      ('İLK YATIRIM','Mar 2019', false),
    ];
    return Container(
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(18),
        border: Border.all(color: AppTokens.border),
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(18),
        child: Row(children: [
          for (var i = 0; i < items.length; i++) ...[
            Expanded(child: Container(
              color: AppTokens.surface,
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(items[i].$1, style: const TextStyle(
                  fontSize: 10, letterSpacing: 1, fontWeight: FontWeight.w500,
                  color: AppTokens.textDim,
                )),
                const SizedBox(height: 6),
                Text(privacy ? Fmt.mask(items[i].$2) : items[i].$2,
                  style: TextStyle(
                    fontSize: 16, fontWeight: FontWeight.w500,
                    color: items[i].$3 ? AppTokens.accent : AppTokens.text,
                    letterSpacing: -0.3,
                    fontFeatures: const [FontFeature.tabularFigures()],
                  )),
              ]),
            )),
            if (i < items.length - 1) Container(width: 1, height: 56, color: AppTokens.border),
          ],
        ]),
      ),
    );
  }
}

class _AllocationContent extends StatelessWidget {
  final bool privacy;
  final List<Allocation> allocations;
  const _AllocationContent({required this.privacy, required this.allocations});

  @override
  Widget build(BuildContext context) {
    Color colorFor(String id) => switch (id) {
      'stocks' => AppTokens.cStocks,
      'crypto' => AppTokens.cCrypto,
      'fx'     => AppTokens.cFx,
      'gold'   => AppTokens.cGold,
      _        => AppTokens.cCash,
    };
    return Column(children: [
      SStackedBar(items: [
        for (final a in allocations) (id: a.id, pct: a.pct, color: colorFor(a.id))
      ]),
      const SizedBox(height: 18),
      for (final a in allocations) Padding(
        padding: const EdgeInsets.only(bottom: 12),
        child: Row(children: [
          Container(width: 8, height: 8, decoration: BoxDecoration(
            color: colorFor(a.id), borderRadius: BorderRadius.circular(2),
          )),
          const SizedBox(width: 12),
          Expanded(child: Text(a.label, style: const TextStyle(color: AppTokens.text, fontSize: 13))),
          SizedBox(width: 44, child: Text(
            '%${a.pct.toStringAsFixed(1).replaceAll('.', ',')}',
            textAlign: TextAlign.right,
            style: const TextStyle(color: AppTokens.textMute, fontSize: 12,
              fontFeatures: [FontFeature.tabularFigures()]),
          )),
          SizedBox(width: 80, child: Text(
            privacy ? Fmt.mask(Fmt.tryStr(a.amount, compact: true)) : Fmt.tryStr(a.amount, compact: true),
            textAlign: TextAlign.right,
            style: const TextStyle(color: AppTokens.text, fontSize: 13,
              fontFeatures: [FontFeature.tabularFigures()]),
          )),
        ]),
      ),
    ]);
  }
}

class _ActivityList extends StatelessWidget {
  final bool privacy;
  const _ActivityList({required this.privacy});

  @override
  Widget build(BuildContext context) {
    return Column(children: [
      for (var i = 0; i < activity.take(5).length; i++) ...[
        () {
          final a = activity[i];
          final isBuy = a.type == 'buy', isSell = a.type == 'sell';
          final iconBg = isBuy ? AppTokens.accentSoft : isSell ? const Color(0x1AFF6B5E) : const Color(0x0DFFFFFF);
          final iconFg = isBuy ? AppTokens.accent : isSell ? AppTokens.danger : AppTokens.textMute;
          final glyph = isBuy ? '↓' : isSell ? '↑' : a.type == 'div' ? '◆' : '↻';
          final platformName = platforms.firstWhere((p) => p.id == a.platform, orElse: () => platforms.first).name;
          return Container(
            padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
            decoration: BoxDecoration(
              border: i < 4 ? const Border(bottom: BorderSide(color: AppTokens.border, width: 1)) : null,
            ),
            child: Row(children: [
              Container(
                width: 32, height: 32, decoration: BoxDecoration(color: iconBg, shape: BoxShape.circle),
                alignment: Alignment.center,
                child: Text(glyph, style: TextStyle(color: iconFg, fontWeight: FontWeight.w600, fontSize: 13)),
              ),
              const SizedBox(width: 12),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                RichText(text: TextSpan(
                  style: const TextStyle(color: AppTokens.text, fontSize: 14, fontWeight: FontWeight.w500),
                  children: [
                    TextSpan(text: isBuy ? 'Alım' : isSell ? 'Satım' : (a.note ?? '')),
                    if (a.symbol != null)
                      TextSpan(text: ' · ${a.symbol}', style: const TextStyle(color: AppTokens.textMute, fontWeight: FontWeight.w400)),
                  ],
                )),
                const SizedBox(height: 2),
                Text('${a.date} · $platformName', style: const TextStyle(color: AppTokens.textDim, fontSize: 11)),
              ])),
              if (a.total != null)
                Text(privacy ? Fmt.mask(Fmt.tryStr(a.total!)) : Fmt.tryStr(a.total!),
                  style: const TextStyle(color: AppTokens.text, fontSize: 13,
                    fontFeatures: [FontFeature.tabularFigures()])),
            ]),
          );
        }(),
      ],
    ]);
  }
}
