﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MoneyMonitor.Windows.Infrastructure;

namespace MoneyMonitor.Windows.Controls
{
    public sealed partial class HistoryChart : Control
    {
        private List<int> _dataPoints;

        public string Title { set; private get; }

        public string CurrencySymbol { set; private get; }

        public HistoryChart()
        {
            InitializeComponent();

            DoubleBuffered = true;

            SetStyle(ControlStyles.UserPaint, true);
        }

        public void UpdateData(List<int> dataPoints)
        {
            _dataPoints = dataPoints;

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            var graphics = paintEventArgs.Graphics;

            graphics.Clear(Color.Black);

            var font = new Font("Lucida Console", 8);

            var textBrush = new SolidBrush(Color.White);
            
            graphics.DrawString(Title, font, textBrush, 2, 2);

            string text;

            SizeF size;

            if (_dataPoints == null || _dataPoints.Count == 0)
            {
                text = "No Data";

                size = graphics.MeasureString(text, font);

                graphics.DrawString(text, font, textBrush, (Width - size.Width) / 2, (Height - size.Height) / 2);

                return;
            }

            var min = _dataPoints.Min();

            var max = _dataPoints.Max();

            var delta = max - min;

            if (delta == 0)
            {
                text = "Invalid Data";

                size = graphics.MeasureString(text, font);

                graphics.DrawString(text, font, textBrush, (Width - size.Width) / 2, (Height - size.Height) / 2);

                return;
            }

            var yScale = (float) (Height - Constants.TextHeight * 2) / delta;

            var barBrush = new SolidBrush(Color.DarkSlateBlue);

            var backgroundBrush = new SolidBrush(Color.FromArgb(30, 30, 30));

            var index = _dataPoints.Count - 1;

            float? currentY = null;

            for (var x = Width - 2; x > -Constants.BarWidth; x -= Constants.BarWidth + Constants.BarSpace)
            {
                graphics.FillRectangle(backgroundBrush, x - Constants.BarWidth, Constants.TextHeight, Constants.BarWidth, Height - Constants.TextHeight * 2);

                if (index < 0)
                {
                    continue;
                }

                var barHeight = (_dataPoints[index] - min) * yScale;

                if (barHeight < 2)
                {
                    barHeight = 2;
                }

                graphics.FillRectangle(barBrush, x - Constants.BarWidth, Constants.TextHeight + (Height - Constants.TextHeight * 2 - barHeight), Constants.BarWidth, barHeight);

                currentY ??= Constants.TextHeight + (Height - Constants.TextHeight * 2 - barHeight);

                index--;
            }

            var title = $"{CurrencySymbol}{max / 100m:N2}";

            size = graphics.MeasureString(title, font);

            graphics.DrawString(title, font, textBrush, Width / 2f - size.Width / 2, 2);

            title = $"{CurrencySymbol}{min / 100m:N2}";

            size = graphics.MeasureString(title, font);

            graphics.DrawString(title, font, textBrush, Width / 2f - size.Width / 2, Height - size.Height);

            title = $"{CurrencySymbol}{_dataPoints.Last() / 100m:N2}";

            size = graphics.MeasureString(title, font);

            // TODO: Sort magic constant +2
            // ReSharper disable once PossibleInvalidOperationException
            graphics.DrawString(title, font, textBrush, Width - size.Width, (float) currentY - size.Height / 2f + 2);

            var pen = new Pen(Color.DimGray, 1);

            graphics.DrawLine(pen, 1, (float) currentY, Width - size.Width, (float) currentY);

            if (_dataPoints.Count > 1)
            {
                var diff = _dataPoints.Last() - _dataPoints[^2];

                title = $"{(diff > 0 ? "+" : "-")}{CurrencySymbol}{Math.Abs(diff / 100m):N2}";

                size = graphics.MeasureString(title, font);

                if (diff > 0)
                {
                    graphics.DrawString(title, font, textBrush, Width - size.Width, 2);
                }
                else if (diff < 0)
                {
                    graphics.DrawString(title, font, textBrush, Width - size.Width, Height - size.Height);
                }
            }
        }
    }
}
