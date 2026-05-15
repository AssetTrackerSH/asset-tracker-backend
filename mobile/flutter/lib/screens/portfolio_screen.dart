import 'package:flutter/material.dart';
import '../theme.dart';
import '../data/mock_data.dart';
import '../data/models.dart';
import '../widgets/primitives.dart';
import '../widgets/line_chart.dart';
import '../widgets/holding_row.dart';

class PortfolioScreen extends StatefulWidget {
  final void Function(Holding) onOpenAsset;
  final bool privacy;
  const PortfolioScreen({super.key, required this.onOpenAsset, required this.privacy});

  @override
  State<PortfolioScreen> createState() => _PortfolioScreenState();
}

class _PortfolioScreenState extends State<PortfolioScreen> {
  String _range = '1Y';

  @override
  Widget build(BuildContext context) {
    final series = portfolioSeries()[_range]!;
    final rPct = {'1A': portfolio.changeMonthPct, '1Y': portfolio.changeYearPct, '5Y': 64.2, 'TÜM': portfolio.changeAllTimePct}[_range]!;
    final rAmt = {'1A': portfolio.changeMonth, '1Y': portfolio.changeYear, '5Y': 1112000.0, 'TÜM': portfolio.changeAllTime}[_range]!;

    final totalStr = Fmt.tryStr(portfolio.total);
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

        // Trio
        _TrioStats(privacy: widget.privacy),

        const SizedBox(height: 28),
        const SSectionLabel(label: 'Varlık Dağılımı', right: '5 sınıf'),
        SCard(child: _AllocationContent(privacy: widget.privacy)),

        const SizedBox(height: 28),
        SSectionLabel(label: 'Varlıklarım', right: '${holdings.length} varlık'),
        SCard(
          padding: const EdgeInsets.symmetric(horizontal: 18),
          child: Column(children: [
            for (final h in holdings.take(8))
              SHoldingRow(h: h, privacy: widget.privacy, onTap: () => widget.onOpenAsset(h)),
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 14),
              child: Center(child: Text('Tümünü gör (${holdings.length})',
                style: const TextStyle(color: AppTokens.textMute, fontSize: 13, fontWeight: FontWeight.w500))),
            ),
          ]),
        ),

        const SizedBox(height: 28),
        const SSectionLabel(label: 'Son İşlemler', right: 'Son 2 hafta'),
        SCard(padding: EdgeInsets.zero, child: _ActivityList(privacy: widget.privacy)),
      ]),
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
  const _AllocationContent({required this.privacy});

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
        for (final a in allocation) (id: a.id, pct: a.pct, color: colorFor(a.id))
      ]),
      const SizedBox(height: 18),
      for (final a in allocation) Padding(
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
