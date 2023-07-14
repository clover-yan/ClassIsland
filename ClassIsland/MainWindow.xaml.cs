﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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
using System.Windows.Threading;
using ClassIsland.Models;
using ClassIsland.ViewModels;
using ClassIsland.Views;

namespace ClassIsland;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainViewModel ViewModel
    {
        get;
        set;
    } = new();

    public ProfileSettingsWindow? ProfileSettingsWindow
    {
        get;
        set;
    }

    public DispatcherTimer UpdateTimer
    {
        get;
    } = new()
    {
        Interval = TimeSpan.FromMilliseconds(25)
    };

    public MainWindow()
    {
        InitializeComponent();
        UpdateTimer.Tick += UpdateTimerOnTick;
        DataContext = this;
        UpdateTimer.Start();
    }

    private void UpdateTimerOnTick(object? sender, EventArgs e)
    {
        LoadCurrentClassPlan();

        if (ViewModel.CurrentClassPlan is null)
        {
            return;
        }

        foreach (var i in ViewModel.CurrentClassPlan.TimeLayout.Layouts)
        {
            if (i.StartSecond.TimeOfDay <= DateTime.Now.TimeOfDay && i.EndSecond.TimeOfDay >= DateTime.Now.TimeOfDay)
            {
                ViewModel.CurrentSelectedIndex = ViewModel.CurrentClassPlan.TimeLayout.Layouts.IndexOf(i);
                break;
            }
        }
    }

    public void LoadProfile()
    {
        var json = File.ReadAllText("./Profile.json");
        var r = JsonSerializer.Deserialize<Profile>(json);
        if (r != null)
        {
            ViewModel.Profile = r;
        }
    }

    public void SaveProfile()
    {
        File.WriteAllText("./Profile.json", JsonSerializer.Serialize<Profile>(ViewModel.Profile));
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        LoadProfile();
        ViewModel.Profile.PropertyChanged += (sender, args) => SaveProfile();
    }

    private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
    {
        ProfileSettingsWindow = new ProfileSettingsWindow
        {
            MainViewModel = ViewModel,
            Owner = this
        };
        ProfileSettingsWindow.Closed += (o, args) => SaveProfile();
        ProfileSettingsWindow.Show();
    }

    public bool CheckClassPlan(ClassPlan plan)
    {
        if (plan.TimeRule.WeekDay != (int)DateTime.Now.DayOfWeek)
        {
            return false;
        }
        // TODO: 完成单双周判定
        return true;
    }

    public void LoadCurrentClassPlan()
    {
        var a = (from p in ViewModel.Profile.ClassPlans
            where CheckClassPlan(p.Value)
            select p.Value).ToList();
        ViewModel.CurrentClassPlan = a.Count < 1 ? null : a[0]!;
    }

    private void ListView_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        e.Handled = true;
    }
}
