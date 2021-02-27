﻿using System;
using System.Drawing;
using System.Windows.Forms;
using MoneyMonitor.Windows.Controls;
using MoneyMonitor.Windows.Infrastructure;

namespace MoneyMonitor.Windows.Forms
{
    public partial class History : Form
    {
        public HistoryChart HistoryChart { get; set; }

        public bool IsTransient { get; private set; }

        public string Currency { get; set; }

        public Action<History, Point> FormMoved { set; private get; }

        private Point? _previousMouse;

        public History()
        {
            InitializeComponent();

            HistoryChart = new HistoryChart
                           {
                               Left = 1,
                               Top = 1,
                               Width = Width - 2,
                               Height = Height - 2,
                               BarWidth = Constants.BarWidth,
                               BarSpace = Constants.BarSpace,
                               FontName = "Lucida Console",
                               FontSize = 8
                           };

            Controls.Add(HistoryChart);
        }

        public void Show(bool transient)
        {
            Show();

            IsTransient = transient;

            if (transient)
            {
                Deactivate += OnDeactivate;
            }
            else
            {
                Cursor = Cursors.SizeAll;

                MouseDown += OnMouseDown;
                MouseMove += OnMouseMove;
                MouseUp += OnMouseUp;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _previousMouse = null;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (! _previousMouse.HasValue)
            {
                return;
            }

            Left += Cursor.Position.X - _previousMouse.Value.X;

            Top += Cursor.Position.Y - _previousMouse.Value.Y;

            _previousMouse = Cursor.Position;

            FormMoved?.Invoke(this, new Point(Left, Top));
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            _previousMouse = Cursor.Position;
        }

        private void OnDeactivate(object sender, EventArgs e)
        {
            if (! TopMost)
            {
                Close();
            }
        }
    }
}
