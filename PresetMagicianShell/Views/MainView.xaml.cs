﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Catel.IoC;
using PresetMagicianShell.Services.Interfaces;
using Xceed.Wpf.AvalonDock;

namespace PresetMagicianShell.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
            
            var serviceLocator = ServiceLocator.Default;

            serviceLocator.RegisterInstance<DockingManager>(DockingManager);
        }
    }
}