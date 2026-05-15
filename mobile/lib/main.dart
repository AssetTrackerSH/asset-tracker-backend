import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'theme.dart';
import 'data/models.dart';
import 'screens/portfolio_screen.dart';
import 'screens/asset_detail_screen.dart';
import 'screens/goals_screen.dart';
import 'screens/news_screen.dart';
import 'screens/profile_screen.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  SystemChrome.setSystemUIOverlayStyle(const SystemUiOverlayStyle(
    statusBarColor: Colors.transparent,
    statusBarIconBrightness: Brightness.light,
    systemNavigationBarColor: AppTokens.bg,
    systemNavigationBarIconBrightness: Brightness.light,
  ));
  runApp(const ServetApp());
}

class ServetApp extends StatelessWidget {
  const ServetApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Servet',
      debugShowCheckedModeBanner: false,
      theme: AppTokens.theme(context),
      home: const HomeShell(),
    );
  }
}

class HomeShell extends StatefulWidget {
  const HomeShell({super.key});

  @override
  State<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends State<HomeShell> {
  int _tab = 0;
  Holding? _openAsset;
  bool _privacy = false;

  void _switchTab(int i) {
    setState(() { _tab = i; _openAsset = null; });
  }

  @override
  Widget build(BuildContext context) {
    Widget body;
    if (_openAsset != null && _tab == 0) {
      body = AssetDetailScreen(
        asset: _openAsset!,
        onBack: () => setState(() => _openAsset = null),
        privacy: _privacy,
      );
    } else {
      body = switch (_tab) {
        0 => PortfolioScreen(privacy: _privacy, onOpenAsset: (h) => setState(() => _openAsset = h)),
        1 => GoalsScreen(privacy: _privacy),
        2 => const NewsScreen(),
        _ => ProfileScreen(privacy: _privacy, onPrivacyChanged: (v) => setState(() => _privacy = v)),
      };
    }

    return Scaffold(
      backgroundColor: AppTokens.bg,
      body: Stack(children: [
        SafeArea(bottom: false, child: body),
        // Floating bottom tab bar
        Positioned(
          left: 18, right: 18, bottom: 22,
          child: _TabBar(
            tab: _tab,
            accent: AppTokens.accent,
            onChanged: _switchTab,
          ),
        ),
        // Sync indicator top right
        if (_tab == 0 && _openAsset == null)
          Positioned(
            top: MediaQuery.of(context).padding.top + 8,
            right: 18,
            child: Container(
              padding: const EdgeInsets.fromLTRB(8, 5, 10, 5),
              decoration: BoxDecoration(
                color: const Color(0x0FFFFFFF),
                borderRadius: BorderRadius.circular(999),
                border: Border.all(color: AppTokens.border),
              ),
              child: const Row(mainAxisSize: MainAxisSize.min, children: [
                _Dot(color: AppTokens.accent),
                SizedBox(width: 6),
                Text('Senkron', style: TextStyle(
                  color: AppTokens.textMute, fontSize: 11, fontWeight: FontWeight.w500,
                )),
              ]),
            ),
          ),
      ]),
    );
  }
}

class _Dot extends StatelessWidget {
  final Color color;
  const _Dot({required this.color});
  @override
  Widget build(BuildContext context) => Container(
    width: 6, height: 6,
    decoration: BoxDecoration(color: color, shape: BoxShape.circle),
  );
}

class _TabBar extends StatelessWidget {
  final int tab;
  final Color accent;
  final ValueChanged<int> onChanged;
  const _TabBar({required this.tab, required this.accent, required this.onChanged});

  @override
  Widget build(BuildContext context) {
    final items = const [
      (label: 'Portföy',  icon: Icons.bar_chart_rounded),
      (label: 'Hedefler', icon: Icons.adjust_rounded),
      (label: 'Haberler', icon: Icons.article_outlined),
      (label: 'Profil',   icon: Icons.person_outline_rounded),
    ];
    return Container(
      padding: const EdgeInsets.all(8),
      decoration: BoxDecoration(
        color: const Color(0xD9141414),  // 0.85 alpha
        borderRadius: BorderRadius.circular(28),
        border: Border.all(color: AppTokens.border),
        boxShadow: [BoxShadow(color: Colors.black.withOpacity(0.4), blurRadius: 32, offset: const Offset(0, 8))],
      ),
      child: Row(children: [
        for (var i = 0; i < items.length; i++)
          Expanded(child: GestureDetector(
            onTap: () => onChanged(i),
            behavior: HitTestBehavior.opaque,
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 180),
              padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 4),
              decoration: BoxDecoration(
                color: tab == i ? const Color(0x0FFFFFFF) : Colors.transparent,
                borderRadius: BorderRadius.circular(22),
              ),
              child: Column(mainAxisSize: MainAxisSize.min, children: [
                Icon(items[i].icon, size: 18,
                  color: tab == i ? accent : AppTokens.textDim),
                const SizedBox(height: 3),
                Text(items[i].label, style: TextStyle(
                  fontSize: 10, fontWeight: FontWeight.w500, letterSpacing: 0.2,
                  color: tab == i ? AppTokens.text : AppTokens.textDim,
                )),
              ]),
            ),
          )),
      ]),
    );
  }
}
