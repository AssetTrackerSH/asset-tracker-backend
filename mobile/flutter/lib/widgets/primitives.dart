import 'package:flutter/material.dart';
import '../theme.dart';

/// Kart — yumuşak yüzey, ince border, 20px radius
class SCard extends StatelessWidget {
  final Widget child;
  final EdgeInsetsGeometry padding;
  final VoidCallback? onTap;
  const SCard({super.key, required this.child, this.padding = const EdgeInsets.all(18), this.onTap});

  @override
  Widget build(BuildContext context) {
    return Material(
      color: AppTokens.surface,
      borderRadius: BorderRadius.circular(AppTokens.radiusCard),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(AppTokens.radiusCard),
        child: Container(
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(AppTokens.radiusCard),
            border: Border.all(color: AppTokens.border, width: 1),
          ),
          padding: padding,
          child: child,
        ),
      ),
    );
  }
}

/// Pill etiket — tone bazlı renk
class SPill extends StatelessWidget {
  final String label;
  final String tone; // 'mute' | 'accent' | 'danger' | 'solid'
  const SPill({super.key, required this.label, this.tone = 'mute'});

  @override
  Widget build(BuildContext context) {
    final Color bg, fg, bd;
    switch (tone) {
      case 'accent':
        bg = AppTokens.accentSoft; fg = AppTokens.accent; bd = const Color(0x2EC5F04A); break;
      case 'danger':
        bg = const Color(0x1AFF6B5E); fg = AppTokens.danger; bd = const Color(0x33FF6B5E); break;
      case 'solid':
        bg = AppTokens.accent; fg = AppTokens.bg; bd = AppTokens.accent; break;
      default:
        bg = const Color(0x0FFFFFFF); fg = AppTokens.textMute; bd = AppTokens.border;
    }
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 9, vertical: 4),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: bd, width: 1),
      ),
      child: Text(label, style: TextStyle(
        fontSize: 11, fontWeight: FontWeight.w500, color: fg, letterSpacing: 0.2, height: 1,
      )),
    );
  }
}

/// Delta — artış/azalış göstergesi
class SDelta extends StatelessWidget {
  final double value;
  final String suffix;
  final double size;
  final bool plain;
  const SDelta({super.key, required this.value, this.suffix = '%', this.size = 13, this.plain = false});

  @override
  Widget build(BuildContext context) {
    final pos = value >= 0;
    final color = pos ? AppTokens.accent : AppTokens.danger;
    final txt = (pos ? '+' : '−') + value.abs().toStringAsFixed(2).replaceAll('.', ',') + suffix;
    return Row(mainAxisSize: MainAxisSize.min, children: [
      if (!plain) ...[
        Text(pos ? '▲' : '▼', style: TextStyle(fontSize: size * 0.7, color: color)),
        const SizedBox(width: 3),
      ],
      Text(txt, style: TextStyle(
        fontSize: size, color: color, fontWeight: FontWeight.w500,
        fontFeatures: const [FontFeature.tabularFigures()],
      )),
    ]);
  }
}

/// Section başlığı — ALL CAPS, dim
class SSectionLabel extends StatelessWidget {
  final String label;
  final String? right;
  const SSectionLabel({super.key, required this.label, this.right});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.fromLTRB(4, 0, 4, 10),
      child: Row(children: [
        Text(label.toUpperCase(), style: const TextStyle(
          fontSize: 11, letterSpacing: 1.4, fontWeight: FontWeight.w500, color: AppTokens.textDim,
        )),
        const Spacer(),
        if (right != null) Text(right!, style: const TextStyle(fontSize: 12, color: AppTokens.textMute)),
      ]),
    );
  }
}

/// Range selector — 1A / 1Y / 5Y / TÜM (segmented pill)
class SRangeSelector extends StatelessWidget {
  final List<String> ranges;
  final String value;
  final ValueChanged<String> onChange;
  const SRangeSelector({super.key, required this.ranges, required this.value, required this.onChange});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(3),
      decoration: BoxDecoration(
        color: AppTokens.surface2,
        borderRadius: BorderRadius.circular(999),
        border: Border.all(color: AppTokens.border),
      ),
      child: Row(children: ranges.map((r) {
        final active = r == value;
        return Expanded(child: GestureDetector(
          onTap: () => onChange(r),
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 150),
            padding: const EdgeInsets.symmetric(vertical: 7),
            alignment: Alignment.center,
            decoration: BoxDecoration(
              color: active ? AppTokens.text : Colors.transparent,
              borderRadius: BorderRadius.circular(999),
            ),
            child: Text(r, style: TextStyle(
              fontSize: 12, fontWeight: FontWeight.w500,
              color: active ? AppTokens.bg : AppTokens.textMute,
            )),
          ),
        ));
      }).toList()),
    );
  }
}

/// Stacked horizontal bar (allocation)
class SStackedBar extends StatelessWidget {
  final List<({String id, double pct, Color color})> items;
  final double height;
  const SStackedBar({super.key, required this.items, this.height = 8});

  @override
  Widget build(BuildContext context) {
    return Container(
      height: height,
      decoration: BoxDecoration(
        color: AppTokens.surface2,
        borderRadius: BorderRadius.circular(999),
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(999),
        child: Row(children: [
          for (var i = 0; i < items.length; i++) ...[
            Expanded(flex: (items[i].pct * 10).round(), child: Container(color: items[i].color)),
            if (i < items.length - 1) const SizedBox(width: 2),
          ]
        ]),
      ),
    );
  }
}
