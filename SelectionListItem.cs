using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MinecraftResourcepacksMaker
{
    internal class SelectionListItem
    {
        private string _displayText;

        /// <summary>
        /// 公开属性：列表项显示的文本内容
        /// 绑定到UI的TextBlock控件，属性值变化时自动更新UI
        /// </summary>
        public string DisplayText
        {
            get => _displayText; // 简化的get访问器
            set
            {
                _displayText = value; // 赋值给私有字段
                OnPropertyChanged(); // 触发属性变更通知
            }
        }
        private string _rawPath;
        public string RawPath
        {
            get => _rawPath; // 简化的get访问器
            set
            {
                _rawPath = value; // 赋值给私有字段
                OnPropertyChanged(); // 触发属性变更通知
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // 空值判断：防止事件未订阅时出现空引用异常
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
