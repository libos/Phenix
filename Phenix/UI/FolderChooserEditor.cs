using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;

using System.IO;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;

namespace Phenix.UI
{
    public class FolderChooserEditor : Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor
    {
        TextBox tb;

        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            DockPanel dp = new DockPanel();
            dp.LastChildFill = true;
            Button bt = new Button();
            bt.Content = "...";
            bt.Click += new RoutedEventHandler(bt_Click);
            DockPanel.SetDock(bt, Dock.Right);
            dp.Children.Add(bt);
            tb = new TextBox();
            tb.Text = "xyz";
            dp.Children.Add(tb);

            //create the binding from the bound property item to the editor
            var _binding = new Binding("Value"); //bind to the Value property of the PropertyItem
            _binding.Source = propertyItem;
            _binding.ValidatesOnExceptions = true;
            _binding.ValidatesOnDataErrors = true;
            _binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(tb, TextBox.TextProperty, _binding);
            return dp;
        }

        void bt_Click(Object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog openFile = new System.Windows.Forms.FolderBrowserDialog();
            //  Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
            //  openFile.Filter = "All Files (*.*)|*.*";

            openFile.Description = @"选择数据文件所在的根目录：";
            openFile.ShowNewFolderButton = true;
            if (openFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb.Text = openFile.SelectedPath;
                BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
        }    

    }
    public class FileChooserEditor : Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor
    {
        TextBox tb;

        public FrameworkElement ResolveEditor(Xceed.Wpf.Toolkit.PropertyGrid.PropertyItem propertyItem)
        {
            DockPanel dp = new DockPanel();
            dp.LastChildFill = true;
            Button bt = new Button();
            bt.Content = " ... ";
            bt.Click += new RoutedEventHandler(bt_Click);
            DockPanel.SetDock(bt, Dock.Right);
            dp.Children.Add(bt);
            tb = new TextBox();
            tb.Text = "xyz";
            dp.Children.Add(tb);

            //create the binding from the bound property item to the editor
            var _binding = new Binding("Value"); //bind to the Value property of the PropertyItem
            _binding.Source = propertyItem;
            _binding.ValidatesOnExceptions = true;
            _binding.ValidatesOnDataErrors = true;
            _binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(tb, TextBox.TextProperty, _binding);
            return dp;
        }

        void bt_Click(Object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
            openFile.Filter = "All Files (*.*)|*.*";

            if (openFile.ShowDialog() == true)
            {
                tb.Text = openFile.FileName;
                BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
        }
    }
}
