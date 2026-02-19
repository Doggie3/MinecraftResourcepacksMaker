using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MinecraftResourcepacksMaker
{
    /// <summary>
    /// ListView列表项的数据模型
    /// 实现INotifyPropertyChanged接口，支持属性变更通知（数据绑定必备）
    /// </summary>
    public class BlockListItem : INotifyPropertyChanged
    {
        // 私有字段：存储列表项要显示的文本内容
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

        /// <summary>
        /// 列表项中按钮的点击命令
        /// WPF推荐使用ICommand接口实现按钮点击逻辑，而非直接绑定Click事件
        /// 优点：解耦UI和业务逻辑，支持命令参数传递
        /// </summary>
        public ICommand ButtonClickCommand { get; set; }

        /// <summary>
        /// 属性变更事件（INotifyPropertyChanged接口要求）
        /// UI绑定此事件，当属性变化时自动刷新显示
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更通知的方法
        /// [CallerMemberName]特性：自动获取调用此方法的属性名，无需手动传入
        /// </summary>
        /// <param name="propertyName">发生变化的属性名称</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // 空值判断：防止事件未订阅时出现空引用异常
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}