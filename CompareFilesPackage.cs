using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE80;
using System.Collections.Generic;
using EnvDTE;
using System.IO;
using System.Windows.Forms;

namespace ARDG.CompareFiles
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [Guid(GuidList.guidCompareFilesPkgString)]
    public sealed class CompareFilesPackage : Package
    {
        private string compareToolPath = @"%PROGRAMFILES%\Beyond Compare 4\BCompare.exe";
        private const string settingsFilePath = @"%USERPROFILE%\AppData\Local\CompareFilesAddIn\CompareFiles.conf";

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public CompareFilesPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                {
                    // Create the command for the context menu item.
                    CommandID menuCommandID = new CommandID(GuidList.guidCompareFilesCmdSet, (int)PkgCmdIDList.cmdidCompareFiles);
                    MenuCommand menuItem = new MenuCommand(CompareFilesCallback, menuCommandID);
                    mcs.AddCommand(menuItem);
                }
                {
                    // Create the command for the web context menu item.
                    CommandID webMenuCommandID = new CommandID(GuidList.guidCompareFilesWebCmdSet, (int)PkgCmdIDList.cmdidCompareFilesWeb);
                    MenuCommand webMenuItem = new MenuCommand(CompareFilesCallback, webMenuCommandID);
                    mcs.AddCommand(webMenuItem);
                }
                {
                    // Create the command for the multiple cross project context menu items.
                    CommandID multiMenuCommandID = new CommandID(GuidList.guidCompareFilesMultiCmdSet, (int)PkgCmdIDList.cmdidCompareFilesMulti);
                    MenuCommand multiMenuItem = new MenuCommand(CompareFilesCallback, multiMenuCommandID);
                    mcs.AddCommand(multiMenuItem);
                }
                {
                    // Create the command for the configuration window
                    CommandID toolwndCommandID = new CommandID(GuidList.guidConfigureCompareFilesCmdSet, (int)PkgCmdIDList.cmdidConfigureCompareFiles);
                    MenuCommand menuToolWin = new MenuCommand(ShowCompareFilesConfigurationWindow, toolwndCommandID);
                    mcs.AddCommand(menuToolWin);
                }
            }
        }
        #endregion

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void CompareFilesCallback(object sender, EventArgs e)
        {
            LoadCompareToolPath();

            DTE2 _applicationObject = (DTE2)GetService(typeof(DTE));

            var items = _applicationObject.SelectedItems;

            var compareToolPathExpanded = Environment.ExpandEnvironmentVariables(compareToolPath);

            string arguments;
            switch (items.Count)
            {
                case 1:
                    SelectedItem item = items.Item(1);
                    for (short i = 1; i <= item.ProjectItem.FileCount; i++)
                    {
                        arguments = "\"" + item.ProjectItem.FileNames[i] + "\"";
                        System.Diagnostics.Process.Start(compareToolPathExpanded, arguments);
                    }

                    var subProjectItems = item.ProjectItem.ProjectItems;
                    if (subProjectItems != null)
                    {
                        for (short i = 1; i <= subProjectItems.Count; i++)
                        {
                            ProjectItem subItem = subProjectItems.Item(i);
                            for (short j = 1; j <= subItem.FileCount; j++)
                            {
                                arguments = "\"" + subItem.FileNames[j] + "\"";
                                System.Diagnostics.Process.Start(compareToolPathExpanded, arguments);
                            }
                        }
                    }
                    break;
                case 2:
                    SelectedItem item1 = items.Item(1);
                    SelectedItem item2 = items.Item(2);
                    for (short i = 1; i <= Math.Min(item1.ProjectItem.FileCount, item2.ProjectItem.FileCount); i++)
                    {
                        arguments = "\"" + item1.ProjectItem.FileNames[i] + "\" \"" + item2.ProjectItem.FileNames[i] + "\"";
                        System.Diagnostics.Process.Start(compareToolPathExpanded, arguments);
                    }

                    var subProjectItems1 = item1.ProjectItem.ProjectItems;
                    var subProjectItems2 = item2.ProjectItem.ProjectItems;
                    if (subProjectItems1 != null && subProjectItems2 != null)
                    {
                        for (short i = 1; i <= Math.Min(subProjectItems1.Count, subProjectItems2.Count); i++)
                        {
                            ProjectItem subItem1 = subProjectItems1.Item(i);
                            ProjectItem subItem2 = subProjectItems2.Item(i);
                            for (short j = 1; j <= Math.Min(subItem1.FileCount, subItem1.FileCount); j++)
                            {
                                arguments = "\"" + subItem1.FileNames[i] + "\" \"" + subItem2.FileNames[i] + "\"";
                                System.Diagnostics.Process.Start(compareToolPathExpanded, arguments);
                            }
                        }
                    }
                    break;
                default:
                    MessageBox.Show("Select 1 or 2 files.", "Compare Files");
                    return;
            }
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowCompareFilesConfigurationWindow(object sender, EventArgs e)
        {
            LoadCompareToolPath();

            using (ConfigureDialog dialog = new ConfigureDialog(compareToolPath))
            {
                DialogResult result = dialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    StoreCompareToolPath(dialog.ToolPath);
                }
            }
        }

        private void LoadCompareToolPath()
        {
            FileInfo settingsFile = new FileInfo(Environment.ExpandEnvironmentVariables(settingsFilePath));
            if (settingsFile.Exists)
            {
                using (FileStream fileStream = settingsFile.OpenRead())
                {
                    TextReader reader = new StreamReader(fileStream);
                    var storedCompareToolPath = reader.ReadLine();
                    if (!String.IsNullOrWhiteSpace(storedCompareToolPath))
                    {
                        compareToolPath = storedCompareToolPath;
                    }
                }
            }
        }

        private void StoreCompareToolPath(string newToolPath)
        {
            FileInfo file = new FileInfo(Environment.ExpandEnvironmentVariables(settingsFilePath));
            if (!file.Directory.Exists)
                file.Directory.Create();

            FileStream fileStream = null;
            try
            {
                fileStream = file.Create();
                using (TextWriter writer = new StreamWriter(fileStream))
                {
                    fileStream = null;
                    writer.WriteLine(newToolPath);
                }
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Dispose();
            }
        }

    }
}
