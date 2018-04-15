using System;
using System.Activities.XamlIntegration;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Xaml;
using ICSharpCode.AvalonEdit.Document;

namespace wmsMLC.DCL.WorkflowDesigner.ViewModels.Xaml
{
    public class XamlViewModel : INotifyPropertyChanged, IXamlViewModel
    {
        private readonly DispatcherTimer _modelTimer = new DispatcherTimer();
        private readonly DispatcherTimer _textTimer = new DispatcherTimer();
        private readonly IDesignerViewModel _designerViewModel;

        public XamlViewModel(IDesignerViewModel designerViewModel)
        {
            _designerViewModel = designerViewModel;
            _designerViewModel.SurfaceChanged += SurfaceChanged;

            Document = new TextDocument();
            Document.TextChanged += Document_TextChanged;

            _modelTimer.Interval = TimeSpan.FromSeconds(1);
            _modelTimer.Tick += (sender, e) => UpdateModel();

            _textTimer.Interval = TimeSpan.FromSeconds(1);
            _textTimer.Tick += (sender, e) => UpdateText();

            ErrorText = string.Empty;
            ErrorVisibility = Visibility.Visible;
            TextBoxVisibility = Visibility.Visible;
        }

        void Document_TextChanged(object sender, EventArgs e)
        {
            _modelTimer.Stop();
            _modelTimer.Start();
        }

        public TextDocument Document
        {
            get;
            set;
        }

        public Visibility TextBoxVisibility
        {
            get;
            set;
        }

        public string ErrorText
        {
            get;
            private set;
        }

        public Visibility ErrorVisibility
        {
            get;
            private set;
        }

        private void SurfaceChanged()
        {
            if (_designerViewModel.CurrentSurface == null) return;

            if (_designerViewModel.CurrentSurface is IDesignerSurface)
            {
                var designer = ((IDesignerSurface)_designerViewModel.CurrentSurface).Designer;

                TextBoxVisibility = Visibility.Visible;
                OnPropertyChanged("TextBoxVisibility");

                designer.ModelChanged -= designer_ModelChanged;
                designer.ModelChanged += designer_ModelChanged;

                ModelChanged();
            }
            else if (_designerViewModel.CurrentSurface is ILoadErrorDesignerSurface)
            {
                TextBoxVisibility = Visibility.Visible;
                Document.Text = ((ILoadErrorDesignerSurface)_designerViewModel.CurrentSurface).Xaml;

                OnPropertyChanged("TextBoxVisibility");
            }
        }

        void designer_ModelChanged(object sender, EventArgs e)
        {
            ModelChanged();
        }

        private void UpdateText()
        {
            _textTimer.Stop();

            if (!(_designerViewModel.CurrentSurface is IDesignerSurface)) return;
            var designer = ((IDesignerSurface) _designerViewModel.CurrentSurface).Designer;

            if (designer == null)
                return;

            //STUB: не делаем Flush (течет память)
            //designer.Flush();            
            if (designer.Text == null)
                return;

            Document.TextChanged -= Document_TextChanged;
            Document.Text = designer.Text;
            Document.TextChanged += Document_TextChanged;
        }

        private void UpdateModel()
        {
            _modelTimer.Stop();

            ErrorVisibility = Visibility.Collapsed;
            ErrorText = string.Empty;
            OnPropertyChanged("ErrorVisibility");
            OnPropertyChanged("ErrorText");
            try
            {
                var designerSurface = _designerViewModel.CurrentSurface as IDesignerSurface;
                if (designerSurface != null)
                {
                    if (designerSurface.Designer.Text == Document.Text) return;



                    var text = Document.Text.Trim();

                    /*while (text.StartsWith(" "))
                    {
                        text = text.Substring(1);
                    }*/

                    var root = text.StartsWith("<Activity ")
                        ? XamlServices.Load(
                            ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(new StringReader(text))))
                        : ActivityXamlServices.Load(new StringReader(text));

                    if (root == null) throw new ApplicationException("Nothing to load");
                    _designerViewModel.ReloadDesigner(root);
                }
            }
            catch (Exception ex)
            {
                ErrorVisibility = Visibility.Visible;
                ErrorText = ex.Message;
                OnPropertyChanged("ErrorVisibility");
                OnPropertyChanged("ErrorText");
            }
        }

        private void ModelChanged()
        {
            _textTimer.Stop();
            _textTimer.Start();
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
