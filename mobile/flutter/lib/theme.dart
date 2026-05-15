import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

/// Servet design tokens — uzun vadeli finans takip uygulaması
class AppTokens {
  AppTokens._();

  // Surfaces
  static const Color bg          = Color(0xFF0A0A0A);
  static const Color surface     = Color(0xFF141414);
  static const Color surface2    = Color(0xFF1C1C1C);
  static const Color border      = Color(0x0FFFFFFF);     // rgba(255,255,255,0.06)
  static const Color borderStrong= Color(0x1FFFFFFF);     // rgba(255,255,255,0.12)

  // Text
  static const Color text        = Color(0xFFFAFAFA);
  static const Color textMute    = Color(0x8CFFFFFF);     // 0.55
  static const Color textDim     = Color(0x52FFFFFF);     // 0.32

  // Accent + states
  static const Color accent      = Color(0xFFC5F04A);     // lime
  static const Color accentSoft  = Color(0x1AC5F04A);     // 0.10
  static const Color danger      = Color(0xFFFF6B5E);

  // Class colors (allocation)
  static const Color cStocks = Color(0xFFC5F04A);
  static const Color cCrypto = Color(0xFFE8E8E8);
  static const Color cFx     = Color(0xFF8A8A8A);
  static const Color cGold   = Color(0xFF4A4A4A);
  static const Color cCash   = Color(0xFF2A2A2A);

  // Geometry
  static const double radiusCard = 20;
  static const double radiusPill = 999;

  // Typography — Inter Tight headings, Inter body, tabular numerals
  static TextTheme buildTextTheme(BuildContext context) {
    final base = Theme.of(context).textTheme;
    return GoogleFonts.interTightTextTheme(base).copyWith(
      // Display number (hero portfolio total)
      displayLarge: GoogleFonts.interTight(
        fontSize: 44, height: 1.0, letterSpacing: -1.5,
        fontWeight: FontWeight.w500, color: text,
        fontFeatures: const [FontFeature.tabularFigures()],
      ),
      // Screen title
      headlineMedium: GoogleFonts.interTight(
        fontSize: 28, height: 1.15, letterSpacing: -0.8,
        fontWeight: FontWeight.w500, color: text,
      ),
      titleLarge: GoogleFonts.interTight(
        fontSize: 17, fontWeight: FontWeight.w500, color: text,
      ),
      bodyMedium: GoogleFonts.inter(
        fontSize: 14, height: 1.45, color: text,
      ),
      bodySmall: GoogleFonts.inter(
        fontSize: 12, color: textMute,
      ),
      labelSmall: GoogleFonts.inter(
        fontSize: 11, letterSpacing: 1.4,
        fontWeight: FontWeight.w500, color: textDim,
      ),
    );
  }

  static ThemeData theme(BuildContext context) {
    final tt = buildTextTheme(context);
    return ThemeData(
      brightness: Brightness.dark,
      scaffoldBackgroundColor: bg,
      colorScheme: const ColorScheme.dark(
        primary: accent,
        secondary: accent,
        surface: surface,
        error: danger,
      ),
      textTheme: tt,
      splashFactory: InkRipple.splashFactory,
      visualDensity: VisualDensity.standard,
    );
  }
}
