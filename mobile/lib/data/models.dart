import 'package:intl/intl.dart';

class Holding {
  final String id, symbol, name, klass, sub, platform;
  final double qty, price, value, costAvg, change1d, change1y;
  const Holding({
    required this.id, required this.symbol, required this.name,
    required this.klass, required this.sub, required this.platform,
    required this.qty, required this.price, required this.value,
    required this.costAvg, required this.change1d, required this.change1y,
  });
  double get gain => value - qty * costAvg;
  double get gainPct => (gain / (qty * costAvg)) * 100;
}

class Allocation {
  final String id, label;
  final double amount, pct;
  const Allocation(this.id, this.label, this.amount, this.pct);
}

class Platform {
  final String id, name, kind, synced, status;
  final int count;
  const Platform(this.id, this.name, this.kind, this.count, this.synced, this.status);
}

class Goal {
  final String id, label, note;
  final int year;
  final double target, current, monthly;
  const Goal(this.id, this.label, this.year, this.target, this.current, this.monthly, this.note);
}

class NewsItem {
  final String id, tag, source, time, title, summary;
  const NewsItem(this.id, this.tag, this.source, this.time, this.title, this.summary);
}

class Activity {
  final String id, type;
  final String? symbol, note, platform;
  final double? total;
  final String date;
  const Activity(this.id, this.type, this.symbol, this.total, this.date, this.platform, this.note);
}

class Fmt {
  static final _tr = NumberFormat.decimalPattern('tr_TR');
  static String tryStr(num n, {bool compact = false, bool sign = false, int decimals = 0}) {
    if (compact && n.abs() >= 1000000) {
      final f = NumberFormat('#,##0.##', 'tr_TR').format(n / 1000000);
      return '$f M ₺';
    }
    final f = NumberFormat.decimalPatternDigits(locale: 'tr_TR', decimalDigits: decimals).format(n.abs());
    final s = sign ? (n >= 0 ? '+' : '−') : (n < 0 ? '−' : '');
    return '$s$f ₺';
  }

  static String pct(double n) =>
      (n >= 0 ? '+' : '−') + n.abs().toStringAsFixed(2).replaceAll('.', ',') + '%';

  static String num1(num n, [int d = 0]) =>
      NumberFormat.decimalPatternDigits(locale: 'tr_TR', decimalDigits: d).format(n);

  static String mask(String s) => s.replaceAll(RegExp(r'[0-9]'), '●');
}
