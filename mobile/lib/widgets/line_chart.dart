import 'package:flutter/material.dart';
import 'package:flutter/gestures.dart';
import '../theme.dart';
import '../data/models.dart';

/// Minimal line chart — smooth path, optional fill, hover/touch scrub
class SLineChart extends StatefulWidget {
  final List<double> data;
  final Color color;
  final double height;
  final bool fill;
  const SLineChart({
    super.key, required this.data, this.color = AppTokens.accent,
    this.height = 140, this.fill = true,
  });

  @override
  State<SLineChart> createState() => _SLineChartState();
}

class _SLineChartState extends State<SLineChart> {
  int? _hover;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: widget.height + 24,
      child: LayoutBuilder(builder: (ctx, c) {
        final w = c.maxWidth;
        return MouseRegion(
          onExit: (_) => setState(() => _hover = null),
          child: Listener(
            onPointerMove: (e) {
              final rb = ctx.findRenderObject() as RenderBox;
              final local = rb.globalToLocal(e.position);
              _updateHover(local.dx, w);
            },
            onPointerDown: (e) {
              final rb = ctx.findRenderObject() as RenderBox;
              final local = rb.globalToLocal(e.position);
              _updateHover(local.dx, w);
            },
            child: CustomPaint(
              size: Size(w, widget.height + 24),
              painter: _ChartPainter(
                data: widget.data,
                color: widget.color,
                fill: widget.fill,
                hover: _hover,
                chartHeight: widget.height,
              ),
            ),
          ),
        );
      }),
    );
  }

  void _updateHover(double x, double w) {
    final n = widget.data.length;
    final step = w / (n - 1);
    final idx = (x / step).round().clamp(0, n - 1);
    if (idx != _hover) setState(() => _hover = idx);
  }
}

class _ChartPainter extends CustomPainter {
  final List<double> data;
  final Color color;
  final bool fill;
  final int? hover;
  final double chartHeight;
  _ChartPainter({required this.data, required this.color, required this.fill, this.hover, required this.chartHeight});

  @override
  void paint(Canvas canvas, Size size) {
    if (data.isEmpty) return;
    final w = size.width;
    final h = chartHeight;
    final topPadding = 16.0;
    canvas.translate(0, topPadding);

    var minV = data.reduce((a, b) => a < b ? a : b);
    var maxV = data.reduce((a, b) => a > b ? a : b);
    final pad = (maxV - minV) * 0.1;
    minV -= pad == 0 ? 1 : pad;
    maxV += pad == 0 ? 1 : pad;

    final xStep = w / (data.length - 1);
    double y(double v) => h - ((v - minV) / (maxV - minV)) * h;

    final pts = <Offset>[for (var i = 0; i < data.length; i++) Offset(i * xStep, y(data[i]))];

    // smooth path
    final path = Path()..moveTo(pts.first.dx, pts.first.dy);
    for (var i = 1; i < pts.length; i++) {
      final prev = pts[i - 1];
      final cx = (prev.dx + pts[i].dx) / 2;
      path.cubicTo(cx, prev.dy, cx, pts[i].dy, pts[i].dx, pts[i].dy);
    }

    // fill
    if (fill) {
      final area = Path.from(path)
        ..lineTo(w, h)
        ..lineTo(0, h)
        ..close();
      final grad = LinearGradient(
        begin: Alignment.topCenter, end: Alignment.bottomCenter,
        colors: [color.withOpacity(0.22), color.withOpacity(0)],
      );
      final paint = Paint()..shader = grad.createShader(Rect.fromLTWH(0, 0, w, h));
      canvas.drawPath(area, paint);
    }

    // line
    final linePaint = Paint()
      ..color = color
      ..strokeWidth = 1.6
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round
      ..strokeJoin = StrokeJoin.round;
    canvas.drawPath(path, linePaint);

    // hover marker
    if (hover != null && hover! >= 0 && hover! < pts.length) {
      final p = pts[hover!];
      final dashed = Paint()
        ..color = AppTokens.borderStrong
        ..strokeWidth = 1;
      // dashed vertical
      var dy = 0.0;
      while (dy < h) {
        canvas.drawLine(Offset(p.dx, dy), Offset(p.dx, dy + 2), dashed);
        dy += 5;
      }
      final dot = Paint()..color = color;
      canvas.drawCircle(p, 3.5, dot);
      final ring = Paint()..color = AppTokens.bg..strokeWidth = 2..style = PaintingStyle.stroke;
      canvas.drawCircle(p, 3.5, ring);

      // label above
      final tp = TextPainter(
        text: TextSpan(text: Fmt.tryStr(data[hover!], compact: true), style: const TextStyle(
          fontSize: 11, color: AppTokens.textMute,
          fontFeatures: [FontFeature.tabularFigures()],
        )),
        textDirection: TextDirection.ltr,
      )..layout();
      tp.paint(canvas, Offset((w - tp.width) / 2, -topPadding + 2));
    } else {
      // last-point dot
      final dot = Paint()..color = color;
      canvas.drawCircle(pts.last, 3, dot);
      final ring = Paint()..color = AppTokens.bg..strokeWidth = 2..style = PaintingStyle.stroke;
      canvas.drawCircle(pts.last, 3, ring);
    }
  }

  @override
  bool shouldRepaint(covariant _ChartPainter old) =>
      old.data != data || old.color != color || old.hover != hover;
}
