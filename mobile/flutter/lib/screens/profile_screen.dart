import 'package:flutter/material.dart';
import '../theme.dart';
import '../data/mock_data.dart';
import '../widgets/primitives.dart';

class ProfileScreen extends StatelessWidget {
  final bool privacy;
  final ValueChanged<bool> onPrivacyChanged;
  const ProfileScreen({super.key, required this.privacy, required this.onPrivacyChanged});

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: const EdgeInsets.fromLTRB(18, 8, 18, 110),
      child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
        const Padding(
          padding: EdgeInsets.only(top: 4, bottom: 24),
          child: Text('PROFİL', style: TextStyle(
            fontSize: 12, letterSpacing: 1.2, fontWeight: FontWeight.w500, color: AppTokens.textDim,
          )),
        ),

        // Identity
        SCard(child: Row(children: [
          Container(
            width: 52, height: 52,
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(26),
              gradient: const LinearGradient(
                begin: Alignment.topLeft, end: Alignment.bottomRight,
                colors: [Color(0xFF2A2A2A), Color(0xFF1A1A1A)],
              ),
              border: Border.all(color: AppTokens.borderStrong),
            ),
            alignment: Alignment.center,
            child: const Text('MK', style: TextStyle(color: AppTokens.accent, fontSize: 18, fontWeight: FontWeight.w500)),
          ),
          const SizedBox(width: 14),
          const Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text('Mert K.', style: TextStyle(color: AppTokens.text, fontSize: 16, fontWeight: FontWeight.w500)),
            SizedBox(height: 2),
            Text("2019'dan beri yatırımcı · 7 yıl", style: TextStyle(color: AppTokens.textMute, fontSize: 13)),
          ])),
        ])),

        const SizedBox(height: 24),
        SSectionLabel(label: 'Bağlı Platformlar', right: '${platforms.length} bağlı'),
        SCard(padding: EdgeInsets.zero, child: Column(children: [
          for (var i = 0; i < platforms.length; i++)
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
              decoration: BoxDecoration(
                border: i < platforms.length - 1
                  ? const Border(bottom: BorderSide(color: AppTokens.border)) : null,
              ),
              child: Row(children: [
                Container(
                  width: 36, height: 36,
                  decoration: BoxDecoration(
                    color: AppTokens.surface2,
                    borderRadius: BorderRadius.circular(10),
                    border: Border.all(color: AppTokens.border),
                  ),
                  alignment: Alignment.center,
                  child: Text(platforms[i].name[0],
                    style: const TextStyle(color: AppTokens.text, fontSize: 13, fontWeight: FontWeight.w600)),
                ),
                const SizedBox(width: 12),
                Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(platforms[i].name, style: const TextStyle(color: AppTokens.text, fontSize: 14, fontWeight: FontWeight.w500)),
                  const SizedBox(height: 2),
                  Text('${platforms[i].kind} · ${platforms[i].count} varlık · ${platforms[i].synced}',
                    style: const TextStyle(color: AppTokens.textDim, fontSize: 11)),
                ])),
                Container(
                  width: 6, height: 6,
                  decoration: BoxDecoration(
                    color: platforms[i].status == 'ok' ? AppTokens.accent : AppTokens.textDim,
                    shape: BoxShape.circle,
                  ),
                ),
              ]),
            ),
          Container(height: 1, color: AppTokens.border),
          InkWell(
            onTap: () {},
            child: const Padding(
              padding: EdgeInsets.symmetric(horizontal: 18, vertical: 14),
              child: Row(children: [
                Text('+', style: TextStyle(color: AppTokens.accent, fontSize: 16)),
                SizedBox(width: 8),
                Text('Platform ekle', style: TextStyle(color: AppTokens.accent, fontSize: 14, fontWeight: FontWeight.w500)),
              ]),
            ),
          ),
        ])),

        const SizedBox(height: 24),
        const SSectionLabel(label: 'Ayarlar'),
        SCard(padding: EdgeInsets.zero, child: Column(children: [
          _SettingRow(
            label: 'Gizlilik modu', sub: 'Tutarları gizle',
            toggle: privacy, onToggle: () => onPrivacyChanged(!privacy),
          ),
          const _SettingRow(label: 'Bildirimler', sub: 'Haftalık özet · pazartesi'),
          const _SettingRow(label: 'Para birimi', sub: 'TRY (Türk Lirası)'),
          const _SettingRow(label: 'Vergi raporu', sub: '2025 hazır'),
          const _SettingRow(label: 'Veri dışa aktar', sub: 'CSV, PDF', isLast: true),
        ])),

        const SizedBox(height: 24),
        const Center(child: Padding(
          padding: EdgeInsets.symmetric(vertical: 20),
          child: Text('Servet · v1.0',
            style: TextStyle(color: AppTokens.textDim, fontSize: 11, letterSpacing: 0.5)),
        )),
      ]),
    );
  }
}

class _SettingRow extends StatelessWidget {
  final String label, sub;
  final bool? toggle;
  final VoidCallback? onToggle;
  final bool isLast;
  const _SettingRow({required this.label, required this.sub, this.toggle, this.onToggle, this.isLast = false});

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onToggle,
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 14),
        decoration: BoxDecoration(
          border: isLast ? null : const Border(bottom: BorderSide(color: AppTokens.border)),
        ),
        child: Row(children: [
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text(label, style: const TextStyle(color: AppTokens.text, fontSize: 14, fontWeight: FontWeight.w500)),
            const SizedBox(height: 2),
            Text(sub, style: const TextStyle(color: AppTokens.textDim, fontSize: 11)),
          ])),
          if (toggle != null)
            _ToggleSwitch(value: toggle!, onChanged: (_) => onToggle?.call())
          else
            const Text('›', style: TextStyle(color: AppTokens.textDim, fontSize: 18, height: 1)),
        ]),
      ),
    );
  }
}

class _ToggleSwitch extends StatelessWidget {
  final bool value;
  final ValueChanged<bool> onChanged;
  const _ToggleSwitch({required this.value, required this.onChanged});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: () => onChanged(!value),
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 200),
        width: 42, height: 24,
        padding: const EdgeInsets.all(2),
        decoration: BoxDecoration(
          color: value ? AppTokens.accent : AppTokens.surface2,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: AppTokens.border),
        ),
        child: AnimatedAlign(
          duration: const Duration(milliseconds: 200),
          alignment: value ? Alignment.centerRight : Alignment.centerLeft,
          child: Container(
            width: 18, height: 18,
            decoration: BoxDecoration(
              color: value ? AppTokens.bg : const Color(0xFF777777),
              shape: BoxShape.circle,
            ),
          ),
        ),
      ),
    );
  }
}
