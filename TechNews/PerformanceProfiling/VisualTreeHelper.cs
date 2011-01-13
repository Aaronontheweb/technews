using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls.Primitives;

namespace TechNews.PerformanceProfiling
{
    public static class TreeHelper
    {
        private static int _counter;

        public static void Dump()
        {
            Debug.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                "ID",
                "layer",
                "cached",
                "hasPixels",
                "area",
                "element");

            Dump((FrameworkElement)Application.Current.RootVisual, 0, 0);
        }

        private static bool DoesBrushHavePixels(Brush b)
        {
            // assume gradients have pixels
            SolidColorBrush scb = b as SolidColorBrush;

            return b != null && (scb == null || scb.Color != Colors.Transparent);
        }

        private static Brush GetBackgroundBrush(FrameworkElement fe)
        {
            PropertyInfo pi = fe.GetType().GetProperty("Background");
            object brush = pi == null ? null : pi.GetGetMethod().Invoke(fe, new object[] { });
            return brush as Brush;
        }

        private static bool IsBorderWithBorder(FrameworkElement fe)
        {
            Border b = fe as Border;
            if (b == null)
            {
                return false;
            }

            if (DoesBrushHavePixels(b.BorderBrush))
            {
                return true; // b.BorderThickness.Left > 0 || b.BorderThickness.Right > 0 || b.BorderThickness.Top > 0 || b.BorderThickness.Bottom > 0;
            }

            return false;
        }

        private static void Dump(FrameworkElement fe, int treeDepth, int layerDepth)
        {
            if (fe.Visibility == Visibility.Collapsed)
                return;

            double area = 0;

            Rect r = LayoutInformation.GetLayoutSlot(fe);
            GeneralTransform xform = fe.TransformToVisual(null);
            Point p = xform.Transform(new Point(0, 0));
            Rect bounds = xform.TransformBounds(r);
            Rect screen = new Rect(0, 0, 480, 800);
            screen.Intersect(bounds);

            if (!screen.IsEmpty)
            {
                area = screen.Width * screen.Height;
            }
            else
            {
                area = -1;
            }

            //if (area != -1)
            {
                string name = fe.Name ?? "";

                bool hasPixels = !(fe is Control) && DoesBrushHavePixels(GetBackgroundBrush(fe));

                hasPixels |= IsBorderWithBorder(fe);
                hasPixels |= (fe is Shape) || (fe is TextBlock) || (fe is TextBox);

                if (hasPixels)
                {
                    ++layerDepth;
                }
                else
                {
                    area = 0;
                }
                bool hasCache = fe.CacheMode != null;

                System.Diagnostics.Debug.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t\"{5}\"",
                    _counter,
                    layerDepth,
                    hasCache ? "Cached" : "",
                    hasPixels,
                    area,
                    new String(' ', treeDepth * 2) + fe.ToString() + " (" + name + ")");
            }

            ++_counter;

            int count = VisualTreeHelper.GetChildrenCount(fe);
            for (int index = 0; index < count; ++index)
            {
                Dump((FrameworkElement)VisualTreeHelper.GetChild(fe, index), treeDepth + 1, layerDepth);
            }
        }
    }
}


