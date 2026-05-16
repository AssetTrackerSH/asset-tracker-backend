import 'package:flutter/material.dart';
import '../theme.dart';
import '../data/models.dart';

/// Asset icon — symbol initials with class-tinted background
class SAssetIcon extends StatelessWidget {
  final String symbol;
  final String klass;
  final double size;
  const SAssetIcon({super.key, required this.symbol, required this.klass, this.size = 36});

  @override
  Widget build(BuildContext context) {
    Color bg, fg;
    switch (klass) {
      case 'stocks': bg = const Color(0x1FC5F04A); fg = AppTokens.cStocks; break;
      case 'crypto': bg = const Color(0x1FF4A259); fg = const Color(0xFFF4A259); break;
      case 'fx':     bg = const Color(0x1F8ECAE6); fg = const Color(0xFF8ECAE6); break;
      case 'gold':   bg = const Color(0x1FE6C75A); fg = const Color(0xFFE6C75A); break;
      default:       bg = const Color(0x1FFFFFFF); fg = AppTokens.textMute;
    }
    final letters = symbol.substring(0, symbol.length < 3 ? symbol.length : 3);
    return Container(
      width: size, height: size,
      decoration: BoxDecoration(
        color: bg, shape: BoxShape.circle,
        border: Border.all(color: AppTokens.border),
      ),
      alignment: Alignment.center,
      child: Text(letters, style: TextStyle(
        color: fg, fontSize: size * 0.32, fontWeight: FontWeight.w600, letterSpacing: 0.2,
      )),
    );
  }
}

/// Holding satırı — varlık listesinde tek bir satır
class SHoldingRow extends StatelessWidget {
  final Holding h;
  final bool privacy;
  final VoidCallback? onTap;
  const SHoldingRow({super.key, required this.h, this.privacy = false, this.onTap});

  @override
  Widget build(BuildContext context) {
    final gainPct = h.gainPct;
    final pos = gainPct >= 0;
    final valueStr = Fmt.tryStr(h.value);
    return InkWell(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.fromLTRB(4, 14, 4, 14),
        decoration: const BoxDecoration(
          border: Border(bottom: BorderSide(color: AppTokens.border, width: 1)),
        ),
        child: Row(children: [
          SAssetIcon(symbol: h.symbol, klass: h.klass),
          const SizedBox(width: 12),
          Expanded(child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(children: [
                Text(h.symbol, style: const TextStyle(
                  color: AppTokens.text, fontWeight: FontWeight.w500, fontSize: 15,
                )),
                const SizedBox(width: 8),
                Text(h.sub, style: const TextStyle(color: AppTokens.textDim, fontSize: 11)),
              ]),
              const SizedBox(height: 2),
              Text(h.name, maxLines: 1, overflow: TextOverflow.ellipsis,
                style: const TextStyle(color: AppTokens.textMute, fontSize: 12)),
            ],
          )),
          Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
            Text(privacy ? Fmt.mask(valueStr) : valueStr, style: const TextStyle(
              color: AppTokens.text, fontWeight: FontWeight.w500, fontSize: 14,
              fontFeatures: [FontFeature.tabularFigures()],
            )),
            const SizedBox(height: 2),
            Text(
              (pos ? '+' : '−') + gainPct.abs().toStringAsFixed(2).replaceAll('.', ',') + '%',
              style: TextStyle(
                fontSize: 11, fontWeight: FontWeight.w500,
                color: pos ? AppTokens.accent : AppTokens.danger,
                fontFeatures: const [FontFeature.tabularFigures()],
              ),
            ),
          ]),
        ]),
      ),
    );
  }
}
