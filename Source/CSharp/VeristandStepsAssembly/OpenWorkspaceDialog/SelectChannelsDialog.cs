﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NationalInstruments.TestStand.Interop.API;
using NationalInstruments.VeriStand.ClientAPI;
using NationalInstruments.VeriStand.SystemDefinitionAPI;
using NationalInstruments.VeriStand.SystemStorageUI;
using NationalInstruments.VeriStand.SystemStorage;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xaml;

namespace OpenWorkspaceDialog
{
    public partial class SelectChannelsDialog : Form
    {
        //Veristand Objects Initialize
        SystemDefinition currentSysDef;
        public List<string> channelNamesList = new List<string>();
        public double[] channelValuesArray;
        public List<string> channelNamesListOld = new List<string>();
        public double[] channelValuesArrayOld;
        public enum ChannelType { paramChannel, faultChannel, writableChannel, readableChannel };
        public ChannelType channelType;
        public string sysDefPath;
        public BaseNodeType[] baseNodeArray;
        public BaseNode baseNodeElement;
        SetMultipleChannelsDialogDGV CallingFormGlobal;

        public SelectChannelsDialog(SetMultipleChannelsDialogDGV CallingForm)
        {
            InitializeComponent();

            //Set up Variable objects
            CallingFormGlobal = CallingForm;
            sysDefPath = CallingFormGlobal.sysDefPath;
            channelNamesList = CallingFormGlobal.channelNamesList;
            channelValuesArray = CallingFormGlobal.channelValuesArray;
            channelNamesListOld = CallingFormGlobal.channelNamesListOld;
            channelValuesArrayOld = CallingFormGlobal.channelValuesArrayOld;
            channelType = (ChannelType)CallingFormGlobal.channelType;
            baseNodeArray = CallingFormGlobal.baseNodeArray;
            baseNodeElement = CallingFormGlobal.baseNodeElement;

            this.loggingChannelSelection.ShowCheckBox = true;

            //If the file at path FileGlobals.Veristand.SystemDefinitionPath exists and the extension is ".nivssdf" use that System Definition file to initialize the TreeAliasBrowserWF.
            if (System.IO.File.Exists(StringUtilities.unparseFilePathString(sysDefPath)) && System.IO.Path.GetExtension(StringUtilities.unparseFilePathString(sysDefPath)) == ".nivssdf")
            {
                //File exists with correct extension so try and populate the tree
                InitializeListBox(sysDefPath);
            }
            //If FileGlobals.Veristand.SystemDefinitionPath is empty or the file does not exist at path FileGlobals.Veristand.SystemDefinitionPath.
            else //(sysDefPath == "" || !System.IO.File.Exists(StringUtilities.unparseFilePathString(sysDefPath)))
            {
                //Do nothing
            }
        }
        private void InitializeListBox(string _sysDefPath)
        {
            currentSysDef = new SystemDefinition(StringUtilities.unparseFilePathString(_sysDefPath));

            switch (channelType)
            {
                case ChannelType.faultChannel:
                    loggingChannelSelection.DisplayFilter = ISystemDefinitionBrowserFilterType.K_CHAN_FAULT;
                    loggingChannelSelection.StartNode = currentSysDef.Root.BaseNodeType;
                    break;
                case ChannelType.readableChannel:
                    loggingChannelSelection.DisplayFilter = ISystemDefinitionBrowserFilterType.K_CHAN_R;
                    loggingChannelSelection.StartNode = currentSysDef.Root.BaseNodeType;
                    break;
                case ChannelType.writableChannel:
                    loggingChannelSelection.DisplayFilter = ISystemDefinitionBrowserFilterType.K_CHAN_W;
                    loggingChannelSelection.StartNode = currentSysDef.Root.BaseNodeType;
                    break;
                case ChannelType.paramChannel:
                    loggingChannelSelection.DisplayFilter = ISystemDefinitionBrowserFilterType.K_ALL;
                    loggingChannelSelection.StartNode = currentSysDef.Root.BaseNodeType;
                    break;
            }
            //Check to see if Step.Veristand.ChannelPath[] not empty. 
            //We should not attempt to initialize the TreeALiasBrowserWF with an empty set of ChannelPaths.
            if (channelNamesListOld.Count > 0)
            {
                Root currentSysDefRoot = currentSysDef.Root;
                currentSysDefRoot.FindNodeByPath(channelNamesListOld.ElementAt(0), out baseNodeElement);
                if (baseNodeElement != null)
                {
                    if (baseNodeArray.Length == channelNamesListOld.Count)
                    {
                        for (int i = 0; i < channelNamesListOld.Count; i++)
                        {
                            currentSysDefRoot.FindNodeByPath(channelNamesListOld.ElementAt(i), out baseNodeElement);
                            baseNodeArray[i] = baseNodeElement.BaseNodeType;
                        }
                    }
                    else
                    {
                        Array.Resize(ref baseNodeArray, channelNamesListOld.Count);
                        for (int i = 0; i < channelNamesListOld.Count; i++)
                        {
                            currentSysDefRoot.FindNodeByPath(channelNamesListOld.ElementAt(i), out baseNodeElement);
                            baseNodeArray[i] = baseNodeElement.BaseNodeType;
                        }
                    }
                    this.loggingChannelSelection.SetCheckBoxSelections(baseNodeArray);
                }
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            try
            {
                BaseNodeType[] selections =
                    this.loggingChannelSelection.GetCheckBoxSelections(false)
                        .ToArray();
                channelNamesList.Clear();
                if (selections.Length > 0)
                {
                    channelNamesList.AddRange(from selection in selections where selection is NationalInstruments.VeriStand.SystemStorage.ChannelType || selection is AliasType select selection.NodePath);
                    CallingFormGlobal.channelNamesList = channelNamesList;
                    Array.Resize(ref CallingFormGlobal.channelValuesArray, CallingFormGlobal.channelNamesList.Count);
                    baseNodeArray = selections;
                }

                CallingFormGlobal.sysDefPath = sysDefPath;

                }
                catch (System.NullReferenceException ex)
                {
                    //do nothing
                }
                catch (System.IndexOutOfRangeException ex)
                {
                    //do nothing
                }
            this.Dispose();

        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SysDefFileBrowse_Click(object sender, EventArgs e)
        {
            OpenSysDefBrowseFileDialog();
        }
        private void openSysDefFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            OpenFileDialog openFile = (OpenFileDialog)sender;
            sysDefPath = openFile.FileName;
            sysDefPath = StringUtilities.parseFilePathString(sysDefPath);
            InitializeListBox(sysDefPath);
        }
    }
}
