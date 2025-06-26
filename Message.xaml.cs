using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

// 消息类型枚举
public enum EXMessageType
{
    KEYDOWN,    // 按键按下
    KEYUP,      // 按键释放
    MOUSEMOVE,  // 鼠标移动
    LBUTTONDOWN,// 左键按下
    LBUTTONUP,  // 左键释放
    RBUTTONDOWN,// 右键按下
    RBUTTONUP,  // 右键释放
    MBUTTONDOWN,// 中键按下
    MBUTTONUP,  // 中键释放
    WHEEL,      // 鼠标滚轮
    TIMER,      // 定时器
    CLOSE       // 窗口关闭
}

// 消息基类
public class EXMessage
{
    public EXMessageType MessageType { get; internal set; }
    public IntPtr HWnd { get; internal set; }
    public int Time { get; internal set; }
}

// 键盘消息
public class EXKeyMessage : EXMessage
{
    public Keys Key { get; internal set; }
    public bool Alt { get; internal set; }
    public bool Control { get; internal set; }
    public bool Shift { get; internal set; }
}

// 鼠标消息
public class EXMouseMessage : EXMessage
{
    public int X { get; internal set; }
    public int Y { get; internal set; }
    public int WheelDelta { get; internal set; }
    public MouseButtons Button { get; internal set; }
    public int Clicks { get; internal set; }
}

// 定时器消息
public class EXTimerMessage : EXMessage
{
    public int TimerID { get; internal set; }
}

// EasyX风格图形窗口
public class EXGraphics : Form
{
    private static EXGraphics _instance;
    private static Queue<EXMessage> _messageQueue = new Queue<EXMessage>();
    private static ManualResetEvent _messageAvailable = new ManualResetEvent(false);
    private static Thread _messageThread;
    private static bool _running = true;
    private static Dictionary<int, Timer> _timers = new Dictionary<int, Timer>();

    private EXGraphics(string title, int width, int height)
    {
        Text = title;
        ClientSize = new Size(width, height);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        // 启用双缓冲，减少闪烁
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer, true);
    }

    // 初始化图形窗口
    public static void InitGraph(string title, int width, int height)
    {
        if (_instance != null)
            throw new InvalidOperationException("图形窗口已初始化");

        _instance = new EXGraphics(title, width, height);

        // 启动消息处理线程
        _messageThread = new Thread(MessageLoop);
        _messageThread.IsBackground = true;
        _messageThread.Start();
    }

    // 消息循环
    private static void MessageLoop()
    {
        Application.Run(_instance);
        _running = false;
        _messageAvailable.Set();
    }

    // 获取下一条消息（阻塞方式）
    public static EXMessage GetMessage()
    {
        while (_running)
        {
            lock (_messageQueue)
            {
                if (_messageQueue.Count > 0)
                    return _messageQueue.Dequeue();
            }

            _messageAvailable.Reset();
            _messageAvailable.WaitOne();
        }

        return null;
    }

    // 检查是否有消息（非阻塞方式）
    public static bool PeekMessage(out EXMessage message)
    {
        message = null;

        lock (_messageQueue)
        {
            if (_messageQueue.Count > 0)
            {
                message = _messageQueue.Dequeue();
                return true;
            }
        }

        return false;
    }

    // 注册定时器
    public static void SetTimer(int timerID, int interval)
    {
        if (_timers.ContainsKey(timerID))
            KillTimer(timerID);

        var timer = new Timer(state =>
        {
            lock (_messageQueue)
            {
                _messageQueue.Enqueue(new EXTimerMessage
                {
                    MessageType = EXMessageType.TIMER,
                    HWnd = _instance.Handle,
                    Time = Environment.TickCount,
                    TimerID = timerID
                });
                _messageAvailable.Set();
            }
        }, null, interval, interval);

        _timers[timerID] = timer;
    }

    // 销毁定时器
    public static void KillTimer(int timerID)
    {
        if (_timers.TryGetValue(timerID, out Timer timer))
        {
            timer.Dispose();
            _timers.Remove(timerID);
        }
    }

    // 清屏
    public static void ClearScreen(Color color)
    {
        if (_instance == null)
            throw new InvalidOperationException("图形窗口未初始化");

        _instance.Invalidate();
    }

    // 绘制文本
    public static void DrawText(string text, int x, int y, Font font, Color color)
    {
        if (_instance == null)
            throw new InvalidOperationException("图形窗口未初始化");

        _instance.Invoke(new Action(() =>
        {
            using (var g = _instance.CreateGraphics())
            {
                using (var brush = new SolidBrush(color))
                {
                    g.DrawString(text, font, brush, x, y);
                }
            }
        }));
    }

    // 绘制矩形
    public static void DrawRectangle(int x, int y, int width, int height, Color color, int thickness = 1)
    {
        if (_instance == null)
            throw new InvalidOperationException("图形窗口未初始化");

        _instance.Invoke(new Action(() =>
        {
            using (var g = _instance.CreateGraphics())
            {
                using (var pen = new Pen(color, thickness))
                {
                    g.DrawRectangle(pen, x, y, width, height);
                }
            }
        }));
    }

    // 填充矩形
    public static void FillRectangle(int x, int y, int width, int height, Color color)
    {
        if (_instance == null)
            throw new InvalidOperationException("图形窗口未初始化");

        _instance.Invoke(new Action(() =>
        {
            using (var g = _instance.CreateGraphics())
            {
                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, x, y, width, height);
                }
            }
        }));
    }

    // 绘制圆形
    public static void DrawCircle(int x, int y, int radius, Color color, int thickness = 1)
    {
        if (_instance == null)
            throw new InvalidOperationException("图形窗口未初始化");

        _instance.Invoke(new Action(() =>
        {
            using (var g = _instance.CreateGraphics())
            {
                using (var pen = new Pen(color, thickness))
                {
                    g.DrawEllipse(pen, x - radius, y - radius, radius * 2, radius * 2);
                }
            }
        }));
    }

    // 填充圆形
    public static void FillCircle(int x, int y, int radius, Color color)
    {
        if (_instance == null)
            throw new InvalidOperationException("图形窗口未初始化");

        _instance.Invoke(new Action(() =>
        {
            using (var g = _instance.CreateGraphics())
            {
                using (var brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
                }
            }
        }));
    }

    // 关闭图形窗口
    public static void CloseGraph()
    {
        if (_instance != null && !_instance.IsDisposed)
        {
            _instance.Invoke(new Action(() => _instance.Close()));
            _instance = null;
        }

        // 清理定时器
        foreach (var timer in _timers.Values)
            timer.Dispose();

        _timers.Clear();
    }

    // 窗口绘制事件
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // 在这里可以添加默认绘制代码
    }

    // 键盘按下事件
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        lock (_messageQueue)
        {
            _messageQueue.Enqueue(new EXKeyMessage
            {
                MessageType = EXMessageType.KEYDOWN,
                HWnd = Handle,
                Time = Environment.TickCount,
                Key = e.KeyCode,
                Alt = e.Alt,
                Control = e.Control,
                Shift = e.Shift
            });
            _messageAvailable.Set();
        }
    }

    // 键盘释放事件
    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        lock (_messageQueue)
        {
            _messageQueue.Enqueue(new EXKeyMessage
            {
                MessageType = EXMessageType.KEYUP,
                HWnd = Handle,
                Time = Environment.TickCount,
                Key = e.KeyCode,
                Alt = e.Alt,
                Control = e.Control,
                Shift = e.Shift
            });
            _messageAvailable.Set();
        }
    }

    // 鼠标移动事件
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        lock (_messageQueue)
        {
            _messageQueue.Enqueue(new EXMouseMessage
            {
                MessageType = EXMessageType.MOUSEMOVE,
                HWnd = Handle,
                Time = Environment.TickCount,
                X = e.X,
                Y = e.Y,
                Button = e.Button,
                Clicks = e.Clicks,
                WheelDelta = e.Delta
            });
            _messageAvailable.Set();
        }
    }

    // 鼠标按下事件
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        EXMessageType messageType = e.Button switch
        {
            MouseButtons.Left => EXMessageType.LBUTTONDOWN,
            MouseButtons.Right => EXMessageType.RBUTTONDOWN,
            MouseButtons.Middle => EXMessageType.MBUTTONDOWN,
            _ => EXMessageType.MOUSEMOVE
        };

        lock (_messageQueue)
        {
            _messageQueue.Enqueue(new EXMouseMessage
            {
                MessageType = messageType,
                HWnd = Handle,
                Time = Environment.TickCount,
                X = e.X,
                Y = e.Y,
                Button = e.Button,
                Clicks = e.Clicks,
                WheelDelta = e.Delta
            });
            _messageAvailable.Set();
        }
    }

    // 鼠标释放事件
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        EXMessageType messageType = e.Button switch
        {
            MouseButtons.Left => EXMessageType.LBUTTONUP,
            MouseButtons.Right => EXMessageType.RBUTTONUP,
            MouseButtons.Middle => EXMessageType.MBUTTONUP,
            _ => EXMessageType.MOUSEMOVE
        };

        lock (_messageQueue)
        {
            _messageQueue.Enqueue(new EXMouseMessage
            {
                MessageType = messageType,
                HWnd = Handle,
                Time = Environment.TickCount,
                X = e.X,
                Y = e.Y,
                Button = e.Button,
                Clicks = e.Clicks,
                WheelDelta = e.Delta
            });
            _messageAvailable.Set();
        }
    }

    // 鼠标滚轮事件
    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        lock (_messageQueue)
        {
            _messageQueue.Enqueue(new EXMouseMessage
            {
                MessageType = EXMessageType.WHEEL,
                HWnd = Handle,
                Time = Environment.TickCount,
                X = e.X,
                Y = e.Y,
                Button = e.Button,
                Clicks = e.Clicks,
                WheelDelta = e.Delta
            });
            _messageAvailable.Set();
        }
    }

    // 窗口关闭事件
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        lock (_messageQueue)
        {
            _messageQueue.Enqueue(new EXMessage
            {
                MessageType = EXMessageType.CLOSE,
                HWnd = Handle,
                Time = Environment.TickCount
            });
            _messageAvailable.Set();
        }
    }
}

// 示例使用
public class Program
{
    [STAThread]
    public static void Main()
    {
        // 初始化图形窗口
        EXGraphics.InitGraph("EasyX风格示例", 800, 600);

        // 设置定时器（ID为1，间隔500毫秒）
        EXGraphics.SetTimer(1, 500);

        // 绘制一些初始内容
        EXGraphics.FillRectangle(100, 100, 200, 150, Color.LightBlue);
        EXGraphics.DrawText("按ESC退出，点击鼠标或按键查看消息", 50, 50,
                           new Font("宋体", 12), Color.Black);

        // 消息循环
        bool running = true;
        while (running)
        {
            // 获取消息（阻塞方式）
            var msg = EXGraphics.GetMessage();

            switch (msg.MessageType)
            {
                case EXMessageType.KEYDOWN:
                    var keyMsg = (EXKeyMessage)msg;
                    Console.WriteLine($"按键按下: {keyMsg.Key}");

                    // 按ESC退出
                    if (keyMsg.Key == Keys.Escape)
                        running = false;

                    // 按空格键清屏
                    if (keyMsg.Key == Keys.Space)
                        EXGraphics.ClearScreen(Color.White);
                    break;

                case EXMessageType.LBUTTONDOWN:
                    var mouseMsg = (EXMouseMessage)msg;
                    Console.WriteLine($"左键点击: ({mouseMsg.X}, {mouseMsg.Y})");

                    // 在鼠标点击位置画圆
                    EXGraphics.FillCircle(mouseMsg.X, mouseMsg.Y, 20, Color.Red);
                    break;

                case EXMessageType.TIMER:
                    var timerMsg = (EXTimerMessage)msg;
                    Console.WriteLine($"定时器触发: ID={timerMsg.TimerID}");

                    // 每秒改变背景颜色
                    Random rand = new Random();
                    EXGraphics.ClearScreen(Color.FromArgb(rand.Next(256),
                                                         rand.Next(256),
                                                         rand.Next(256)));
                    break;

                case EXMessageType.CLOSE:
                    Console.WriteLine("窗口关闭");
                    running = false;
                    break;
            }
        }

        // 清理资源
        EXGraphics.KillTimer(1);
        EXGraphics.CloseGraph();
    }
}
