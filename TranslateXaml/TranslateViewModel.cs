using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;

namespace TranslateXaml
{
    public class TranslateViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand BrowseCommand { get; set; }
        public ICommand CreateTranslatedXamlCommand { get; set; }

        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
            }
        }
        public int ProcessCount
        {
            get { return processCount; }
            set
            {
                processCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessCount)));
            }
        }
        public int ProcessDone
        {
            get { return processDone; }
            set
            {
                processDone = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessDone)));
            }
        }

        private string path;
        private int processCount;
        private int processDone;
        private bool isProcessing;

        private string dstFilePath;
        public TranslateViewModel()
        {
            BrowseCommand = new DelegateCommand(Browse);
            CreateTranslatedXamlCommand = new DelegateCommand(CreateTranslatedXaml);
            InitProgress();
            isProcessing = false;
        }

        public void Browse()
        {
            var dialog = new CommonOpenFileDialog("翻訳するファイル（xaml）を選択");
            dialog.InitialDirectory = Path;
            var ret = dialog.ShowDialog();
            if (ret == CommonFileDialogResult.Ok)
            {
                Path = dialog.FileName;
            }
        }

        public void CreateTranslatedXaml()
        {
            if(isProcessing == true)
            {
                return;
            }
            isProcessing = true;

            InitProgress();

            var translate = new Translate();

            translate.OnFileLoaded = OnFileLoaded;
            translate.OnProgress = OnProgress;
            translate.OnCompleted = OnCompleted;
            translate.OnError = OnError;

            var srcDir = System.IO.Path.GetDirectoryName(Path);
            var dstFileName = string.Format("{0}2{1}.{2}", Translate.From, Translate.To, System.IO.Path.GetFileName(Path));
            dstFilePath = System.IO.Path.Combine(srcDir, dstFileName);

            translate.RunAsync(Path, dstFilePath);
        }

        private void OnFileLoaded(int count)
        {
            ProcessCount += count;
        }

        private async void OnProgress(int index)
        {
            ProcessDone++;
            await Task.Delay(10);
        }

        private void OnCompleted()
        {
            ProcessDone++;

            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("完了しました");
                Process.Start("explorer", System.IO.Path.GetDirectoryName(dstFilePath));
            });

            isProcessing = false;
        }

        private void OnError(string msg, Exception exp)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string errMsg = string.Format("{0}\n{1}", msg, exp);
                MessageBox.Show(errMsg, "error");
            });
        }

        private void InitProgress()
        {
            ProcessCount = 1;
            ProcessDone = 0;
        }
    }
}
