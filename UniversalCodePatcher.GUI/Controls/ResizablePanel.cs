using System;
using System.Drawing;
using System.Windows.Forms;

namespace UniversalCodePatcher.Controls
{
    /// <summary>
    /// Panel that resizes based on percentage of parent width and supports mouse
    /// drag resizing. Implements simple minimum width constraint.
    /// </summary>
    public class ResizablePanel : Panel
    {
        private bool _dragging;
        private int _dragStartX;
        private double _widthPercent = 0.3;

        /// <summary>
        /// Percentage of parent width this panel should occupy.
        /// </summary>
        public double WidthPercentage
        {
            get => _widthPercent;
            set
            {
                _widthPercent = Math.Max(0, Math.Min(1, value));
                UpdateSize();
            }
        }

        /// <summary>
        /// Minimum width for the panel in pixels.
        /// </summary>
        public int MinimumWidth { get; set; } = 200;

        /// <summary>
        /// When false user cannot resize panel with the mouse.
        /// </summary>
        public bool AllowResize { get; set; } = true;

        public ResizablePanel()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Cursor = Cursors.SizeWE;
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateSize();
            if (Parent != null)
            {
                Parent.Resize -= Parent_Resize;
                Parent.Resize += Parent_Resize;
            }
        }

        private void Parent_Resize(object? sender, EventArgs e)
        {
            UpdateSize();
        }

        private void UpdateSize()
        {
            if (Parent == null) return;
            int newWidth = (int)(Parent.ClientSize.Width * WidthPercentage);
            newWidth = Math.Max(MinimumWidth, newWidth);
            this.Width = newWidth;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (AllowResize && e.Button == MouseButtons.Left)
            {
                _dragging = true;
                _dragStartX = e.X;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging && Parent != null)
            {
                int delta = e.X - _dragStartX;
                int newWidth = this.Width + delta;
                newWidth = Math.Max(MinimumWidth, newWidth);
                WidthPercentage = newWidth / (double)Parent.ClientSize.Width;
                _dragStartX = e.X;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }
    }
}
