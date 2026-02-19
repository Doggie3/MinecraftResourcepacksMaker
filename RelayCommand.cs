using System;
using System.Windows.Input;

/// <summary>
/// 通用的命令实现类（RelayCommand/DelegateCommand）
/// 封装按钮点击等操作的逻辑，是WPF中ICommand接口的标准实现
/// </summary>
public class RelayCommand : ICommand
{
    // 存储命令执行的逻辑（无返回值，带object参数）
    private readonly Action<object> _execute;
    // 存储判断命令是否可执行的逻辑（可选，默认返回true）
    private readonly Func<object, bool> _canExecute;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="execute">命令执行的具体逻辑</param>
    /// <param name="canExecute">命令是否可执行的判断逻辑（可为null）</param>
    /// <exception cref="ArgumentNullException">当execute为null时抛出</exception>
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        // 空值检查：确保执行逻辑必须存在
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// 判断命令是否可以执行
    /// </summary>
    /// <param name="parameter">命令参数</param>
    /// <returns>true=可执行，false=不可执行（按钮会自动置灰）</returns>
    public bool CanExecute(object parameter)
    {
        // 如果未指定判断逻辑，默认返回true（命令始终可执行）
        return _canExecute?.Invoke(parameter) ?? true;
    }

    /// <summary>
    /// 执行命令的核心方法
    /// </summary>
    /// <param name="parameter">传递给命令的参数（如当前列表项的文本）</param>
    public void Execute(object parameter)
    {
        _execute(parameter);
    }

    /// <summary>
    /// 命令可执行状态变化时触发的事件
    /// 绑定到CommandManager的RequerySuggested事件，自动更新按钮状态
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}