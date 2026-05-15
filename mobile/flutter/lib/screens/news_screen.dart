import 'package:flutter/material.dart';
import '../theme.dart';
import '../data/mock_data.dart';
import '../widgets/primitives.dart';

class NewsScreen extends StatefulWidget {
  const NewsScreen({super.key});

  @override
  State<NewsScreen> createState() => _NewsScreenState();
}

class _NewsScreenState extends State<NewsScreen> {
  String _filter = 'Tümü';
  final _filters = const ['Tümü', 'Hisse', 'Kripto', 'Makro', 'Emtia', 'Strateji'];

  @override
  Widget build(BuildContext context) {
    final items = _filter == 'Tümü' ? news : news.where((n) => n.tag == _filter).toList();

    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(18, 8, 18, 110),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        Padding(
          padding: const EdgeInsets.only(top: 4, bottom: 22),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            const Text('HABERLER', style: TextStyle(
              fontSize: 12, letterSpacing: 1.2, fontWeight: FontWeight.w500, color: AppTokens.textDim,
            )),
            const SizedBox(height: 8),
            Text('Portföyüne dair.',
              style: Theme.of(context).textTheme.headlineMedium),
          ]),
        ),

        // Filters — horizontal scroll
        SizedBox(height: 36, child: ListView.separated(
          scrollDirection: Axis.horizontal,
          itemCount: _filters.length,
          separatorBuilder: (_, __) => const SizedBox(width: 8),
          itemBuilder: (_, i) {
            final f = _filters[i];
            final active = f == _filter;
            return GestureDetector(
              onTap: () => setState(() => _filter = f),
              child: AnimatedContainer(
                duration: const Duration(milliseconds: 150),
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                decoration: BoxDecoration(
                  color: active ? AppTokens.text : AppTokens.surface,
                  borderRadius: BorderRadius.circular(999),
                  border: Border.all(color: active ? Colors.transparent : AppTokens.border),
                ),
                child: Text(f, style: TextStyle(
                  color: active ? AppTokens.bg : AppTokens.textMute,
                  fontSize: 13, fontWeight: FontWeight.w500,
                )),
              ),
            );
          },
        )),
        const SizedBox(height: 22),

        for (var i = 0; i < items.length; i++) Padding(
          padding: const EdgeInsets.only(bottom: 14),
          child: SCard(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              SPill(label: items[i].tag, tone: i == 0 ? 'accent' : 'mute'),
              const SizedBox(width: 8),
              Text(items[i].source, style: const TextStyle(color: AppTokens.textDim, fontSize: 11)),
              const SizedBox(width: 8),
              Text('· ${items[i].time} önce', style: const TextStyle(color: AppTokens.textDim, fontSize: 11)),
            ]),
            const SizedBox(height: 12),
            Text(items[i].title, style: const TextStyle(
              color: AppTokens.text, fontSize: 16, fontWeight: FontWeight.w500,
              height: 1.35, letterSpacing: -0.2,
            )),
            const SizedBox(height: 8),
            Text(items[i].summary, style: const TextStyle(
              color: AppTokens.textMute, fontSize: 13, height: 1.55,
            )),
          ])),
        ),
      ]),
    );
  }
}
