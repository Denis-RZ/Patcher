using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UniversalCodePatcher.Controls
{
    /// <summary>
    /// Layout manager that arranges controls by DockStyle using width percentages
    /// for left/right docked panels without using splitters.
    /// </summary>
    public class DockLayoutManager
    {
        private readonly List<DockableControl> _controls = new();

        /// <summary>
        /// Arrange the given controls inside the parent.
        /// </summary>
        public void ArrangeControls(Control parent, List<DockableControl> controls)
        {
            _controls.Clear();
            _controls.AddRange(controls);
            foreach (var dc in _controls)
            {
                if (!parent.Controls.Contains(dc.Control))
                {
                    parent.Controls.Add(dc.Control);
                }
                dc.Control.Dock = dc.Dock;
            }
            UpdateLayout(parent);
        }

        /// <summary>
        /// Update layout positions based on current parent size.
        /// </summary>
        public void UpdateLayout(Control parent)
        {
            int remainingWidth = parent.ClientSize.Width;
            int remainingHeight = parent.ClientSize.Height;

            int offsetLeft = 0;
            foreach (var dc in _controls.Where(c => c.Dock == DockStyle.Left))
            {
                int width = (int)(parent.ClientSize.Width * dc.WidthPercentage);
                width = Math.Max(dc.MinimumWidth, width);
                dc.Control.Bounds = new Rectangle(offsetLeft, 0, width, remainingHeight);
                offsetLeft += width;
                remainingWidth -= width;
            }

            int offsetRight = parent.ClientSize.Width;
            foreach (var dc in _controls.Where(c => c.Dock == DockStyle.Right))
            {
                int width = (int)(parent.ClientSize.Width * dc.WidthPercentage);
                width = Math.Max(dc.MinimumWidth, width);
                offsetRight -= width;
                dc.Control.Bounds = new Rectangle(offsetRight, 0, width, remainingHeight);
                remainingWidth -= width;
            }

            foreach (var dc in _controls.Where(c => c.Dock == DockStyle.Fill))
            {
                dc.Control.Bounds = new Rectangle(offsetLeft, 0, remainingWidth, remainingHeight);
            }
        }

        public void SaveLayout(string filePath)
        {
            var lines = _controls.Select(c => $"{c.Control.Name}:{c.WidthPercentage}");
            System.IO.File.WriteAllLines(filePath, lines);
        }

        public void LoadLayout(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) return;
            var lines = System.IO.File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                var ctrl = _controls.FirstOrDefault(c => c.Control.Name == parts[0]);
                if (ctrl != null && parts.Length == 2 && double.TryParse(parts[1], out double perc))
                {
                    ctrl.WidthPercentage = perc;
                }
            }
        }
    }

    public class DockableControl
    {
        public Control Control { get; set; } = default!;
        public DockStyle Dock { get; set; }
        public double WidthPercentage { get; set; } = 0.3;
        public int MinimumWidth { get; set; } = 200;
    }
}
