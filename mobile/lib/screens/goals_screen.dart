import 'package:flutter/material.dart';
import '../theme.dart';
import '../data/mock_data.dart';
import '../data/models.dart';
import '../widgets/primitives.dart';

class GoalsScreen extends StatelessWidget {
  final bool privacy;
  const GoalsScreen({super.key, required this.privacy});

  @override
  Widget build(BuildContext context) {
    final overallCurrent = 4700594.0;
    final overallTarget  = 36500000.0;
    final overallPct = (overallCurrent / overallTarget) * 100;

    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(18, 8, 18, 110),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Padding(
          padding: const EdgeInsets.only(top: 4, bottom: 24),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            const Text('HEDEFLER & ANALİZ', style: TextStyle(
              fontSize: 12, letterSpacing: 1.2, fontWeight: FontWeight.w500, color: AppTokens.textDim,
            )),
            const SizedBox(height: 8),
            Text('Uzun vadeli planını\ntek bir yerden izle.',
              style: Theme.of(context).textTheme.headlineMedium),
          ]),
        ),

        // Overall
        SCard(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              const Text('TOPLAM İLERLEME', style: TextStyle(
                fontSize: 11, letterSpacing: 1.2, color: AppTokens.textDim,
              )),
              const SizedBox(height: 6),
              Text('%${overallPct.toStringAsFixed(1).replaceAll('.', ',')}',
                style: const TextStyle(
                  color: AppTokens.text, fontSize: 28, fontWeight: FontWeight.w500,
                  letterSpacing: -0.5,
                  fontFeatures: [FontFeature.tabularFigures()],
                )),
            ])),
            const SPill(label: '3 hedef aktif', tone: 'accent'),
          ]),
          const SizedBox(height: 14),
          _Bar(pct: overallPct, total: 100),
          const SizedBox(height: 12),
          Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            Text(privacy ? '●●●●●●●' : '${Fmt.tryStr(overallCurrent, compact: true)} / ${Fmt.tryStr(overallTarget, compact: true)}',
              style: const TextStyle(color: AppTokens.textMute, fontSize: 12,
                fontFeatures: [FontFeature.tabularFigures()])),
            const Text('· kümülatif', style: TextStyle(color: AppTokens.textMute, fontSize: 12)),
          ]),
        ])),

        const SizedBox(height: 24),
        ...goals.map((g) => Padding(
          padding: const EdgeInsets.only(bottom: 14),
          child: _GoalCard(goal: g, privacy: privacy),
        )),

        const SizedBox(height: 14),
        const SSectionLabel(label: 'Yıllık Yatırım Temposu'),
        SCard(child: Column(children: [
          SizedBox(height: 110, child: Row(crossAxisAlignment: CrossAxisAlignment.end, children: [
            for (final b in const [(y:'21', v:35.0, partial:false), (y:'22', v:48.0, partial:false),
                                   (y:'23', v:62.0, partial:false), (y:'24', v:70.0, partial:false),
                                   (y:'25', v:86.0, partial:false), (y:'26', v:28.0, partial:true)])
              Expanded(child: Padding(
                padding: const EdgeInsets.symmetric(horizontal: 3),
                child: Column(mainAxisAlignment: MainAxisAlignment.end, children: [
                  Container(
                    width: double.infinity,
                    height: 90 * (b.v / 100),
                    decoration: BoxDecoration(
                      color: b.partial ? AppTokens.accentSoft : AppTokens.accent,
                      borderRadius: BorderRadius.circular(4),
                      border: b.partial ? Border.all(color: AppTokens.borderStrong) : null,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text("'${b.y}", style: const TextStyle(color: AppTokens.textDim, fontSize: 10,
                    fontFeatures: [FontFeature.tabularFigures()])),
                ]),
              )),
          ])),
          const SizedBox(height: 14),
          Container(height: 1, color: AppTokens.border),
          const SizedBox(height: 14),
          Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            const Text('2026 tahmini katkı', style: TextStyle(color: AppTokens.textMute, fontSize: 12)),
            Text(privacy ? '●●●●●●' : '612.000 ₺', style: const TextStyle(
              color: AppTokens.text, fontSize: 14, fontWeight: FontWeight.w500,
              fontFeatures: [FontFeature.tabularFigures()])),
          ]),
        ])),
      ]),
    );
  }
}

class _GoalCard extends StatelessWidget {
  final Goal goal;
  final bool privacy;
  const _GoalCard({required this.goal, required this.privacy});

  @override
  Widget build(BuildContext context) {
    final pct = (goal.current / goal.target) * 100;
    final yearsLeft = goal.year - 2026;

    return SCard(padding: EdgeInsets.zero, child: Column(children: [
      Padding(
        padding: const EdgeInsets.fromLTRB(18, 18, 18, 14),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(goal.label, style: const TextStyle(color: AppTokens.text, fontSize: 17, fontWeight: FontWeight.w500)),
              const SizedBox(height: 3),
              Text('Hedef yıl ${goal.year} · $yearsLeft yıl kaldı',
                style: const TextStyle(color: AppTokens.textMute, fontSize: 12)),
            ])),
            Text('%${pct.toStringAsFixed(1).replaceAll('.', ',')}',
              style: const TextStyle(color: AppTokens.text, fontSize: 18, fontWeight: FontWeight.w500,
                letterSpacing: -0.3, fontFeatures: [FontFeature.tabularFigures()])),
          ]),
          const SizedBox(height: 14),
          _Bar(pct: pct, total: 100, height: 5),
          const SizedBox(height: 10),
          Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            Text(privacy ? Fmt.mask(Fmt.tryStr(goal.current, compact: true)) : Fmt.tryStr(goal.current, compact: true),
              style: const TextStyle(color: AppTokens.textMute, fontSize: 12,
                fontFeatures: [FontFeature.tabularFigures()])),
            Text(privacy ? Fmt.mask(Fmt.tryStr(goal.target, compact: true)) : Fmt.tryStr(goal.target, compact: true),
              style: const TextStyle(color: AppTokens.textMute, fontSize: 12,
                fontFeatures: [FontFeature.tabularFigures()])),
          ]),
        ]),
      ),
      Container(height: 1, color: AppTokens.border),
      Container(
        padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 12),
        child: Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
          const Text('Aylık katkı', style: TextStyle(color: AppTokens.textMute, fontSize: 12)),
          Text(privacy ? Fmt.mask(Fmt.tryStr(goal.monthly)) : Fmt.tryStr(goal.monthly),
            style: const TextStyle(color: AppTokens.text, fontSize: 13, fontWeight: FontWeight.w500,
              fontFeatures: [FontFeature.tabularFigures()])),
        ]),
      ),
      Container(height: 1, color: AppTokens.border),
      Padding(
        padding: const EdgeInsets.fromLTRB(18, 14, 18, 14),
        child: Text(goal.note, style: const TextStyle(
          color: AppTokens.textMute, fontSize: 12, height: 1.5,
        )),
      ),
    ]));
  }
}

class _Bar extends StatelessWidget {
  final double pct, total, height;
  const _Bar({required this.pct, required this.total, this.height = 6});

  @override
  Widget build(BuildContext context) {
    return Container(
      height: height,
      decoration: BoxDecoration(
        color: AppTokens.surface2,
        borderRadius: BorderRadius.circular(999),
      ),
      child: FractionallySizedBox(
        widthFactor: (pct / total).clamp(0, 1),
        alignment: Alignment.centerLeft,
        child: Container(
          decoration: BoxDecoration(
            color: AppTokens.accent,
            borderRadius: BorderRadius.circular(999),
          ),
        ),
      ),
    );
  }
}
