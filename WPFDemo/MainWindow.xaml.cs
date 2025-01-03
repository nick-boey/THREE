using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using OpenTK.Windowing.Common;
using THREEExample;
using THREEExample.ThreeImGui;
using WPFDemo.Controls;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace WPFDemo;

/// <summary>
///     MainWindow.xaml에 대한 상호 작용 논리
/// </summary>
public partial class MainWindow : Window
{
    private readonly List<GLRenderWindow> sceneList = new();
    private Example currentExample;

    private GLRenderWindow currentWindow;
    public int timeInterval = 10;
    public Timer timer;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoadExampleFromAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException("assembly");

        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            var attributes = type.GetCustomAttributes(false);
            //foreach (var example in attributes.OfType<DemoAttribute>())
            foreach (var exampleType in attributes)
                if (exampleType is ExampleAttribute)
                {
                    var example = exampleType as ExampleAttribute;
                    //var index = -1;
                    TreeViewItem rootItem = null;
                    foreach (var item in treeViewSample.Items)
                    {
                        var header = example.Category + " " + string.Format("{0}", example.Subcategory);
                        if ((item as TreeViewItem).Header.Equals(header))
                        {
                            rootItem = item as TreeViewItem;
                            break;
                        }
                    }


                    if (rootItem == null)
                    {
                        rootItem = new TreeViewItem();
                        rootItem.Header = example.Category + " " + string.Format("{0}", example.Subcategory);
                        treeViewSample.Items.Add(rootItem);
                    }


                    var treeItem = new TreeViewItem();
                    treeItem.Header = example.Title;

                    //int u = (int)(example.LevelComplete * 5);
                    //treeItem.Foreground = new SolidColorBrush(loC[u]);
                    treeItem.Tag = new ExampleInfo(type, example);

                    rootItem.Items.Add(treeItem);
                }
        }
    }

    private void TimerOnTick(object sender, EventArgs e)
    {
        if (currentWindow != null)
            currentWindow.Render();
    }

    private void treeViewSample_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var item = e.NewValue as TreeViewItem;
        if (item.Tag != null) ActivateNode(item);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        if (sceneList.Count > 0) MessageBox.Show("About", "About");
    }

    private void ActivateNode(TreeViewItem item)
    {
        if (item == null) return;

        if (item.Tag == null)
        {
            if (item.IsExpanded)
                item.IsExpanded = false;
            else
                item.IsExpanded = true;
        }
        else
        {
            RunExample((ExampleInfo)item.Tag);
        }
    }

    private void RunExample(ExampleInfo info)
    {
        if (null == currentWindow)
        {
            MainGrid.Children.Remove(Host);
        }
        else
        {
            MainGrid.Children.Remove(currentWindow);
            currentWindow.Dispose();
            currentWindow = null;
        }

        if (timer != null)
        {
            timer.Stop();
            timer.Dispose();
        }

        currentExample = (Example)Activator.CreateInstance(info.Example);
        var glControl = new GLRenderControl();
#if NET6_0_OR_GREATER
        glControl.Profile = ContextProfile.Compatability;
#endif
        currentExample.Load(glControl);
        currentExample.imGuiManager = new ImGuiManager(glControl);
        currentWindow = new GLRenderWindow(currentExample);


        timer = new Timer();
        timer.Interval = timeInterval;
        timer.Tick += TimerOnTick;
        timer.Start();

        MainGrid.Children.Add(currentWindow);
        Grid.SetColumn(currentWindow, 2);

        currentWindow.Render();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (timer != null)
            timer.Stop();
        var index = 0;
        foreach (var example in sceneList)
            try
            {
                example.Dispose();
                //Trace.TraceInformation((listBox.Items[index] as string));
                index++;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        Application.Current.Shutdown();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var t = typeof(Example);
        LoadExampleFromAssembly(Assembly.GetAssembly(t));
    }
}