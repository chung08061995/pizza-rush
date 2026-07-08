using System;
using System.Collections.Generic;

namespace DraftUtils
{
    /// <summary>
    /// Interface đại diện cho một lệnh thực thi trong Command Queue.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Thực thi lệnh.
        /// </summary>
        void Execute();
    }

    /// <summary>
    /// Bộ quản lý hàng đợi các lệnh, thực thi tuần tự từng lệnh một.
    /// Hỗ trợ cả tác vụ đồng bộ và bất đồng bộ.
    /// </summary>
    public class CommandQueue
    {
        private readonly FormattedLogger _logger = new(FormattedLogger.CreateFormatForType(typeof(CommandQueue)));
        private readonly Queue<ICommand> _commands = new();
        private ICommand _currentCommand;

        /// <summary>
        /// Số lượng lệnh còn lại trong hàng đợi.
        /// </summary>
        public int Count => _commands.Count;

        /// <summary>
        /// Thêm một Command tự định nghĩa vào hàng đợi.
        /// </summary>
        /// <param name="command">Lệnh cần thêm.</param>
        public void Enqueue(ICommand command)
        {
            if (command == null)
            {
                return;
            }
            _commands.Enqueue(command);
            _logger.Log("Enqueue command: {0}. Queue size: {1}", command.GetType().Name, Count);
        }

        /// <summary>
        /// Thêm một tác vụ đồng bộ vào hàng đợi (sẽ thực thi và hoàn thành ngay lập tức).
        /// </summary>
        /// <param name="action">Action chứa logic thực thi đồng bộ.</param>
        public void Enqueue(Action action)
        {
            Enqueue(new SyncActionCommand(action));
        }

        /// <summary>
        /// Thực hiện lệnh tiếp theo trong hàng đợi nếu hàng đợi đang rảnh.
        /// </summary>
        public void TryExecute()
        {
            if (_currentCommand != null)
            {
                return;
            }
            ExecuteNext();
        }

        /// <summary>
        /// Giải phóng lệnh hiện tại và thực hiện lệnh tiếp theo trong hàng đợi.
        /// </summary>
        public void ExecuteNext()
        {
            _currentCommand = null;
            if (_commands.Count == 0)
            {
                _logger.Log("Queue is empty.");
                return;
            }
            _currentCommand = _commands.Dequeue();
            _logger.Log("Start Command: {0}", _currentCommand.GetType().Name);
            _currentCommand.Execute();
        }

        /// <summary>
        /// Xóa sạch tất cả các lệnh đang chờ trong hàng đợi và dừng thực thi.
        /// </summary>
        public void Clear()
        {
            _logger.Log("Clear all commands.");
            _commands.Clear();
            _currentCommand = null;
        }
    }

    /// <summary>
    /// Wrapper Command cho các tác vụ đồng bộ, sẽ hoàn thành ngay khi chạy xong khối lệnh.
    /// </summary>
    public class SyncActionCommand : ICommand
    {
        private readonly Action _action;

        public SyncActionCommand(Action action)
        {
            _action = action;
        }

        public void Execute()
        {
            _action?.Invoke();
        }
    }
}
