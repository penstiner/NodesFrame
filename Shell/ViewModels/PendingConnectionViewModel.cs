using System;
using System.Windows.Input;
using Prism.Commands;

namespace Shell.ViewModels
{
    /// <summary>
    /// 预备连接（PendingConnection）的视图模型，负责在开始/完成连接时调用编辑器的连接方法。
    /// </summary>
    public class PendingConnectionViewModel
    {
        private readonly MainWindowViewModel _editor;
        private Shell.Models.ConnectorViewModel _source;

        public PendingConnectionViewModel(MainWindowViewModel editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            StartCommand = new DelegateCommand<Shell.Models.ConnectorViewModel>(s => _source = s);
            FinishCommand = new DelegateCommand<Shell.Models.ConnectorViewModel>(t =>
            {
                if (t != null && _source != null)
                    _editor.Connect(_source, t);
            });
        }

        public ICommand StartCommand { get; }
        public ICommand FinishCommand { get; }
    }
}
