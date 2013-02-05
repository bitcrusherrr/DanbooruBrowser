using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace dbz.UIComponents.CustomControls
{
    public class LabelTextBox : TextBox
    {
        private bool _isWatermarked;
        private Binding _textBinding;

        public string Label
        {
            get 
            { 
                return (string)GetValue(LabelProperty); 
            }
            set 
            { 
                SetValue(LabelProperty, value); 
            }
        }

        public static readonly DependencyProperty LabelProperty =
           DependencyProperty.Register("Label", typeof(string), typeof(LabelTextBox), new UIPropertyMetadata("Label"));

        public LabelTextBox() : base()
        {
            Loaded += (obj, args) => SetLabel();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text) && !IsFocused)
                SetLabel();

            base.OnTextChanged(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            HideLabel();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            SetLabel();
            base.OnLostFocus(e);
        }

        private void HideLabel()
        {
            if (_isWatermarked)
            {
                _isWatermarked = false;
                ClearValue(ForegroundProperty);

                Text = "";

                if (_textBinding != null) 
                    SetBinding(TextProperty, _textBinding);

                FontStyle = FontStyles.Normal;

                Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void SetLabel()
        {
            if (string.IsNullOrEmpty(Text) && Label != "Label")
            {
                _isWatermarked = true;

                //save the existing binding so it can be restored
                _textBinding = BindingOperations.GetBinding(this, TextProperty);

                //Update the data that's bound to the textbox, otherwise there could be a case where textbox will revert to the "empty" state,
                //but text is still there and target value isn't updated.
                var sourceText = GetBindingExpression(TextBox.TextProperty);
                if (sourceText != null)
                    sourceText.UpdateSource();

                //blank out the existing binding so we can throw in our Watermark
                BindingOperations.ClearBinding(this, TextProperty);

                Text = Label;

                FontStyle = FontStyles.Italic;

                Foreground = new SolidColorBrush(Colors.Gray);
            }
        }
    }
}
