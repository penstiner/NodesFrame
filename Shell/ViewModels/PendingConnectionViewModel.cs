using System;
using System.Windows.Input;
using Prism.Commands;

namespace Shell.ViewModels
{
    /// <summary>
    /// Ԥ�����ӣ�PendingConnection������ͼģ�ͣ������ڿ�ʼ/�������ʱ���ñ༭�������ӷ�����
    /// </summary>
    public class PendingConnectionViewModel
    {
        private readonly DocumentViewModel _document;
        private Shell.Models.ConnectorViewModel _source;

        public PendingConnectionViewModel(DocumentViewModel document)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            StartCommand = new DelegateCommand<Shell.Models.ConnectorViewModel>(s => _source = s);
            FinishCommand = new DelegateCommand<Shell.Models.ConnectorViewModel>(t =>
            {
                if (t != null && _source != null)
                    _document.Connect(_source, t);
            });
        }

        public ICommand StartCommand { get; }
        public ICommand FinishCommand { get; }
    }
}
