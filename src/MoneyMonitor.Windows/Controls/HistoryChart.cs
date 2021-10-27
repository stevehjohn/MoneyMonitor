using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MoneyMonitor.Windows.Controls
{
    public sealed partial class HistoryChart : Control
    {
        private List<int> _dataPoints;

        private decimal? _exchangeRate;

        private DateTime? _dataTime;

        private decimal? _holding;

        private decimal? _holdingPercent;

        private int? _high;

        private int? _low;

        public string Title { set; private get; }

        public string CurrencySymbol { set; private get; }

        public Color BarColour { get; set; }

        public int BarWidth { get; set; }

        public int BarSpace { get; set; }

        public string FontName { get; set; }

        public int FontSize { get; set; }

        public HistoryChart()
        {
            InitializeComponent();

            DoubleBuffered = true;

            SetStyle(ControlStyles.UserPaint, true);

            BarColour = Color.DodgerBlue;

            Enabled = false;
        }

        public void UpdateData(List<int> dataPoints, DateTime? dataTime, decimal? exchangeRate, decimal? holding, decimal? holdingPercent, int? high, int? low)
        {
            _dataPoints = dataPoints;

            _dataTime = dataTime;

            _exchangeRate = exchangeRate;

            _holding = holding;

            _holdingPercent = holdingPercent;

            _high = high;

            _low = low;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            var graphics = paintEventArgs.Graphics;

            graphics.Clear(Color.Black);

            var font = new Font(FontName, FontSize);

            var textBrush = new SolidBrush(Color.White);
            
            graphics.DrawString($"{Title} @ {_dataTime:HH:mm.ss}", font, textBrush, 2, 2);

            SizeF size;

            if (_dataPoints == null || _dataPoints.Count == 0)
            {
                var text = "No Data";

                size = graphics.MeasureString(text, font);

                graphics.DrawString(text, font, textBrush, (Width - size.Width) / 2, (Height - size.Height) / 2);

                return;
            }

            var min = _dataPoints.Min();

            var max = _dataPoints.Max();

            var delta = max - min;

            if (delta == 0)
            {
                delta = max;
            }

            var yScale = (float) (Height - FontSize * 4) / delta;

            var barBrush = new LinearGradientBrush(new Point(0, Height), new Point(0, 0), BarColour, Color.FromArgb(BarColour.A,
                                                                                                                    BarColour.R > 150 ? BarColour.R - 150 : 0,
                                                                                                                    BarColour.G > 150 ? BarColour.G - 150 : 0,
                                                                                                                    BarColour.B > 150 ? BarColour.B - 150 : 0));

            var backgroundBrush = new SolidBrush(Color.FromArgb(30, 30, 30));

            var index = _dataPoints.Count - 1;

            float? currentY = null;

            for (var x = Width - 1; x > -BarWidth; x -= BarWidth + BarSpace)
            {
                graphics.FillRectangle(backgroundBrush, x - BarWidth, FontSize * 2, BarWidth, Height - FontSize * 4);

                if (index < 0)
                {
                    continue;
                }

                var barHeight = (_dataPoints[index] - min) * yScale;

                if (barHeight < 2)
                {
                    barHeight = 2;
                }
                else
                {
                    barHeight = (int) barHeight / (BarWidth + BarSpace) * (BarWidth + BarSpace);
                }

                graphics.FillRectangle(barBrush, x - BarWidth, FontSize * 2 + (Height - FontSize * 4 - barHeight), BarWidth, barHeight);

                currentY ??= Height - barHeight - FontSize * 2;

                index--;
            }

            var gridPen = new Pen(Color.Black);

            for (var y = FontSize * 2; y < Height - FontSize * 2; y += BarWidth + BarSpace)
            {
                graphics.DrawLine(gridPen, 0, y, Width, y);
            }

            var title = $"{CurrencySymbol}{max / 100m:N2}";

            size = graphics.MeasureString(title, font);

            graphics.DrawString(title, font, textBrush, Width / 2f - size.Width / 2, 2);
            
            var dimTextBrush = new SolidBrush(Color.Gray);

            if (_high.HasValue)
            {
                title = $"{CurrencySymbol}{_high / 100m:N2}";

                size = graphics.MeasureString(title, font);

                graphics.DrawString(title, font, dimTextBrush, Width * .68f - size.Width / 2, 2);
            }

            if (_low.HasValue)
            {
                title = $"{CurrencySymbol}{_low / 100m:N2}";

                size = graphics.MeasureString(title, font);

                graphics.DrawString(title, font, dimTextBrush, Width * .32f - size.Width / 2, 2);
            }

            title = $"{CurrencySymbol}{min / 100m:N2}";

            size = graphics.MeasureString(title, font);

            graphics.DrawString(title, font, textBrush, Width / 2f - size.Width / 2, Height - size.Height);

            title = $"{CurrencySymbol}{_dataPoints.Last() / 100m:N2}";

            if (_holdingPercent.HasValue)
            {
                title = $"{title} [{_holdingPercent:N2}%]";
            }

            size = graphics.MeasureString(title, font);

            var pen = new Pen(Color.DimGray, 1);

            var textBackgroundBrush = new SolidBrush(Color.Black);

            // ReSharper disable once PossibleInvalidOperationException
            graphics.DrawLine(pen, 1, (float) currentY, Width, (float) currentY);

            graphics.FillRectangle(textBackgroundBrush, Width - size.Width - BarSpace * 20, (float) currentY - size.Height / 2f, size.Width, size.Height);

            // TODO: Sort magic constant +2
            graphics.DrawString(title, font, textBrush, Width - size.Width - BarSpace * 20, (float) currentY - size.Height / 2f + 2);

            graphics.DrawRectangle(pen, Width - size.Width - BarSpace * 20, (float) currentY - size.Height / 2f, size.Width, size.Height);

            if (_holding.HasValue)
            {
                title = $"{_holding:#,###,###,##0.#########}";

                size = graphics.MeasureString(title, font);

                graphics.FillRectangle(textBackgroundBrush, BarSpace * 20, (float) currentY - size.Height / 2f, size.Width, size.Height);

                // TODO: Sort magic constant +2
                graphics.DrawString(title, font, textBrush, BarSpace * 20, (float) currentY - size.Height / 2f + 2);

                graphics.DrawRectangle(pen, BarSpace * 20, (float) currentY - size.Height / 2f, size.Width, size.Height);
            }

            if (_dataPoints.Count > 1)
            {
                var diff = _dataPoints.Last() - _dataPoints[^2];

                var percent = (decimal) diff / _dataPoints.Last() * 100;

                title = $"{(diff > 0 ? "+" : "-")}{CurrencySymbol}{Math.Abs(diff / 100m):N2} [{Math.Abs(percent):N2}%]";

                size = graphics.MeasureString(title, font);

                switch (diff)
                {
                    case > 0:
                        graphics.DrawString(title, font, textBrush, Width - size.Width, 2);
                        break;
                    case < 0:
                        graphics.DrawString(title, font, textBrush, Width - size.Width, Height - size.Height);
                        break;
                }
            }

            if (_exchangeRate.HasValue)
            {
                var exchangeRate = 1 / _exchangeRate;

                title = $"1 {Title} : {CurrencySymbol}{exchangeRate:N6}";

                size = graphics.MeasureString(title, font);

                graphics.DrawString(title, font, dimTextBrush, 2, Height - size.Height);

                graphics.DrawString(title.Substring(0, title.Length - 4), font, textBrush, 2, Height - size.Height);
            }
        }
    }
}
