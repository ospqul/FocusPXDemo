## 1 Setup Environment

#### 1.1 Create a new WPF project

**Windows Presentation Foundation (WPF)** is a powerful tool to build a GUI application.

Open Visual Studio -> New Project -> Visual c# -> Windows Desktop -> WPF App (.NET Framework) -> Set Solution Name to "FPXDemo" -> OK

#### 1.2 MVVM Pattern

The WPF is built to take full advantage of the **Model-View-ViewModel (MVVM)** pattern. The main goal of MVVM pattern is to provide a clear separation between domain logic and presentation layer, so that you can write **maintainable**, **testable**, and **extensible** code. 

- **Model** − It simply holds the data and has nothing to do with any of the business logic.

- **ViewModel** − It acts as the link/connection between the Model and View and makes stuff look pretty.

- **View** − It simply holds the formatted data and essentially delegates everything to the Model.

  

**Caliburn.Micro** package is a small framework that supports MVVM pattern and enables you to build solution quickly. So let's install Caliburn.Micro in our new project.

Right click project -> Manage NuGet Packages -> Browse -> Search "Caliburn" -> Select "Caliburn.Micro" -> Install latest version

#### 1.3 Setup MVVM

Remove `MainWindow.xaml` from project, and remove `StartupUri="MainWindow.xaml"` from `App.xaml` file, because we will start this application from our own view.

Add a new class file `Bootstrapper.cs` under project. This code tells WPF to start from `ShellViewModel`, and we will create a `ShellView` window file and `ShellViewModel` class file later.

```c#
# Bootstrapper.cs    
using Caliburn.Micro;
using FPXDemo.ViewModels;
using System.Windows;

namespace FPXDemo
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
```

Modify and add Bootstrapper to `App.xaml`.

```xaml
<Application x:Class="FPXDemo.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:FPXDemo">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary>
                    <local:Bootstrapper x:Key="Bootstrapper"/>
                </ResourceDictionary>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

Add 3 new folders under project: `Views`, `ViewModels`, and `Models`.

Add a new window file `ShellView.xaml` under `Views` folder.

Add a new class file `ShellViewModel.cs` under `ViewModels` folder.

Setup is completed by now, and you should be able to rebuild and start this project.

Project folder structure will be similar to this.

```mathematica
FPXDemo
   |--> Models
   |--> ViewModels
   |         |--> ShellViewModel.cs
   |--> Views
   |      |--> ShellView.xaml
   |--> App.config
   |--> App.xaml
   |--> Bootstrapper.cs
```

#### 1.4 Source Code

Run `git checkout 1_Set_Environment` to get source code for this section.
