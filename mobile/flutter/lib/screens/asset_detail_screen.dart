import 'package:flutter/material.dart';
import '../theme.dart';
import '../data/mock_data.dart';
import '../data/models.dart';
import '../widgets/primitives.dart';
import '../widgets/line_chart.dart';
import '../widgets/holding_row.dart';

class AssetDetailScreen extends StatefulWidget {
  final Holding asset;
  final VoidCallback onBack;
  final bool privacy;
  const AssetDetailScreen({super.key, required this.asset, required this.onBack, required this.privacy});

  @override
  State<AssetDetailScreen> createState() => _AssetDetailScreenState();
}

class _AssetDetailScreenState extends State<AssetDetailScreen> {
  String _range = '1Y';

  @override
  Widget build(BuildContext context) {
    final h = widget.asset;
    final series = seriesFor(h)[_range]!;
    final gain = h.gain;
    final gainPct = h.gainPct;
    final platformName = platforms.firstWhere((p) => p.id == h.platform, orElse: () => platforms.first).name;
    final valueStr = Fmt.tryStr(h.value);
    final gainStr = Fmt.tryStr(gain, sign: true);

    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(18, 8, 18, 110),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        // Back
        Align(
          alignment: Alignment.centerLeft,
          child: TextButton.icon(
            onPressed: widget.onBack,
            icon: const Text('‹', style: TextStyle(color: AppTokens.textMute, fontSize: 22, height: 1)),
            label: const Text('Portföy', style: TextStyle(color: AppTokens.textMute, fontSize: 14)),
            style: TextButton.styleFrom(padding: EdgeInsets.zero, minimumSize: const Size(0, 32)),
          ),
        ),
        const SizedBox(height: 14),

        // Identity
        Row(children: [
          SAssetIcon(symbol: h.symbol, klass: h.klass, size: 52),
          const SizedBox(width: 14),
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              Text(h.symbol, style: const TextStyle(
                color: AppTokens.text, fontSize: 22, fontWeight: FontWeight.w500, letterSpacing: -0.5,
              )),
              const SizedBox(width: 8),
              SPill(label: h.sub),
            ]),
            const SizedBox(height: 4),
            Text(h.name, style: const TextStyle(color: AppTokens.textMute, fontSize: 13)),
          ])),
        ]),
        const SizedBox(height: 22),

        // Value
        const Text('TOPLAM DEĞER', style: TextStyle(
          fontSize: 11, letterSpacing: 1.2, fontWeight: FontWeight.w500, color: AppTokens.textDim,
        )),
        const SizedBox(height: 6),
        Text(widget.privacy ? Fmt.mask(valueStr) : valueStr, style: const TextStyle(
          color: AppTokens.text, fontSize: 38, fontWeight: FontWeight.w500, letterSpacing: -1.2, height: 1,
          fontFeatures: [FontFeature.tabularFigures()],
        )),
        const SizedBox(height: 8),
        Row(children: [
          SDelta(value: gainPct, size: 13),
          const SizedBox(width: 10),
          Text(widget.privacy ? Fmt.mask(gainStr) : '$gainStr toplam', style: const TextStyle(
            color: AppTokens.textMute, fontSize: 13,
            fontFeatures: [FontFeature.tabularFigures()],
          )),
        ]),
        const SizedBox(height: 18),

        SLineChart(data: series, color: gainPct < 0 ? AppTokens.danger : AppTokens.accent),
        const SizedBox(height: 14),
        SRangeSelector(
          ranges: const ['1A', '1Y', '5Y', 'TÜM'],
          value: _range,
          onChange: (v) => setState(() => _range = v),
        ),
        const SizedBox(height: 28),

        const SSectionLabel(label: 'Pozisyon'),
        SCard(padding: EdgeInsets.zero, child: Column(children: [
          _DetailRow(label: 'Adet / Miktar',
            value: '${Fmt.num1(h.qty, h.qty < 10 ? 3 : 0)} ${_unit(h.klass, h.symbol)}',
          ),
          _DetailRow(label: 'Ortalama Maliyet',
            value: widget.privacy ? Fmt.mask(Fmt.tryStr(h.costAvg, decimals: 2)) : Fmt.tryStr(h.costAvg, decimals: 2),
          ),
          _DetailRow(label: 'Güncel Fiyat',
            value: widget.privacy ? Fmt.mask(Fmt.tryStr(h.price, decimals: 2)) : Fmt.tryStr(h.price, decimals: 2),
          ),
          _DetailRow(label: 'Kâr / Zarar',
            value: widget.privacy ? Fmt.mask(gainStr) : gainStr,
            color: gain >= 0 ? AppTokens.accent : AppTokens.danger,
          ),
          _DetailRow(label: 'Bağlı Platform', value: platformName, isLast: true),
        ])),

        const SizedBox(height: 28),
        const SSectionLabel(label: 'Notlarım'),
        SCard(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(_noteFor(h.klass), style: const TextStyle(
            color: AppTokens.textMute, fontSize: 13, height: 1.55, fontStyle: FontStyle.italic,
          )),
          const SizedBox(height: 12),
          Container(height: 1, color: AppTokens.border),
          const SizedBox(height: 12),
          const Text('Son güncelleme: 14 Mart 2026',
            style: TextStyle(color: AppTokens.textDim, fontSize: 11)),
        ])),

        const SizedBox(height: 28),
        const SSectionLabel(label: 'İşlem Geçmişi'),
        SCard(padding: EdgeInsets.zero, child: Column(children: [
          _TxRow(t: 'Alım', d: '14 Şub 2026', q: '4 adet',  p: widget.privacy ? '●●●●●●' : Fmt.tryStr(7820 * 4, compact: true)),
          _TxRow(t: 'Alım', d: '8 Oca 2026',  q: '6 adet',  p: widget.privacy ? '●●●●●●' : Fmt.tryStr(6940 * 6, compact: true)),
          _TxRow(t: 'Alım', d: '22 Kas 2025', q: '8 adet',  p: widget.privacy ? '●●●●●●' : Fmt.tryStr(6210 * 8, compact: true)),
          _TxRow(t: 'Alım', d: '15 Eyl 2025', q: '12 adet', p: widget.privacy ? '●●●●●●' : Fmt.tryStr(5840 * 12, compact: true), isLast: true),
        ])),
      ]),
    );
  }

  String _unit(String klass, String symbol) {
    if (klass == 'crypto' || klass == 'fx' || klass == 'cash') return symbol;
    return 'adet';
  }

  String _noteFor(String klass) => switch (klass) {
    'crypto' => '"En az 4 yıl tutma" planı. Halving sonrası kademeli ekleme. Aylık DCA: 5.000 ₺.',
    'gold'   => 'Portföy sigortası. %14 hedef ağırlık; rebalance yılda 1 kez.',
    'cash'   => 'Kasada saklanıyor. Acil fon yedeği — satılmayacak.',
    _        => 'Uzun vadeli temettü stratejisi. Hedef tutma süresi: 10+ yıl.',
  };
}

class _DetailRow extends StatelessWidget {
  final String label, value;
  final Color? color;
  final bool isLast;
  const _DetailRow({required this.label, required this.value, this.color, this.isLast = false});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
      decoration: BoxDecoration(
        border: isLast ? null : const Border(bottom: BorderSide(color: AppTokens.border)),
      ),
      child: Row(children: [
        Text(label, style: const TextStyle(color: AppTokens.textMute, fontSize: 13)),
        const Spacer(),
        Text(value, style: TextStyle(
          color: color ?? AppTokens.text, fontSize: 14, fontWeight: FontWeight.w500,
          fontFeatures: const [FontFeature.tabularFigures()],
        )),
      ]),
    );
  }
}

class _TxRow extends StatelessWidget {
  final String t, d, q, p;
  final bool isLast;
  const _TxRow({required this.t, required this.d, required this.q, required this.p, this.isLast = false});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
      decoration: BoxDecoration(
        border: isLast ? null : const Border(bottom: BorderSide(color: AppTokens.border)),
      ),
      child: Row(children: [
        Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text(t, style: const TextStyle(color: AppTokens.text, fontSize: 14, fontWeight: FontWeight.w500)),
          const SizedBox(height: 2),
          Text('$d · $q', style: const TextStyle(color: AppTokens.textDim, fontSize: 11)),
        ])),
        Text(p, style: const TextStyle(color: AppTokens.text, fontSize: 13,
          fontFeatures: [FontFeature.tabularFigures()])),
      ]),
    );
  }
}
