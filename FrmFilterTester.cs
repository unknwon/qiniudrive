#region 文档说明
/* ******************************************************************************************************
 * 文档作者：无闻
 * 创建日期：2013 年 10 月 20 日
 * 文档用途：七牛云盘过滤规则测试窗口
 * ------------------------------------------------------------------------------------------------------
 * 修改记录：
 * ------------------------------------------------------------------------------------------------------
 * 参考文献：
 * ******************************************************************************************************/
#endregion

#region 命名空间引用
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

using CharmCommonMethod;
using CharmControlLibrary;
#endregion

namespace QiNiuDrive
{
    // 七牛云盘过滤规则测试窗口
    public partial class FrmFilterTester : Form
    {
        #region 常量
        private const int TITLE_HEIGHT = 30;                    // 标题栏高度
        #endregion

        #region 字段
        // * 用户控件 *
        private List<Control> mControls;                    // 控件集合
        // 0-测试目录文本框
        private List<CharmControl> mCharmControls;          // Charm 控件集合
        private readonly ToolTip mToolTip = new ToolTip();  // 工具提示文本控件
        #endregion

        #region 窗体事件
        // 窗体构造方法
        public FrmFilterTester()
        {
            // 初始化组件
            InitializeComponent();
            // 设置双缓冲模式
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | //不擦除背景 ,减少闪烁
                               ControlStyles.OptimizedDoubleBuffer | //双缓冲
                               ControlStyles.UserPaint, //使用自定义的重绘事件,减少闪烁
                               true);
            this.UpdateStyles();
            // 初始化设置
            InitializeSetting();
        }

        // 窗体鼠标单击事件
        private void FrmFilterTester_MouseClick(object sender, MouseEventArgs e)
        {
            // 调用事件
            CharmControl.MouseClickEvent(e, mCharmControls);
        }

        // 窗体鼠标按下事件
        private void FrmFilterTester_MouseDown(object sender, MouseEventArgs e)
        {
            // 调用事件
            if (!CharmControl.MouseDownEvent(e, mCharmControls, this))
                APIOperation.MoveNoBorderForm(this, e);
        }

        // 窗体鼠标移动事件
        private void FrmFilterTester_MouseMove(object sender, MouseEventArgs e)
        {
            // 调用事件
            CharmControl.MouseMoveEvent(e, mCharmControls, this, mToolTip);
        }

        // 窗体鼠标弹起事件
        private void FrmFilterTester_MouseUp(object sender, MouseEventArgs e)
        {
            // 调用事件
            CharmControl.MouseUpEvent(e, mCharmControls, this);
        }
        #endregion

        #region  重载事件
        // 控件重绘事件
        protected override void OnPaint(PaintEventArgs e)
        {
            // 获取绘制对象
            Graphics g = e.Graphics;

            // 测试目录
            g.DrawString("模拟同步目录：", this.Font, Brushes.Black, 20, TITLE_HEIGHT + 15);

            // 绘制控件
            CharmControl.PaintEvent(e.Graphics, mCharmControls);

            // 重置重绘索引
            //mMultipleSettingRedrawIndex = 0;
        }
        #endregion

        #region 控件事件
        // 关闭按钮被单击事件
        private void btnClose_MouseClick(object sender, MouseEventArgs e)
        {
            this.Hide();
        }

        // 浏览路径按钮被单击事件
        private void btnViewPath_MouseClick(object sender, MouseEventArgs e)
        {
            string testDir = ((CharmTextBox)mControls[0]).Text;
            // 创建并实例化文件浏览对话框
            FolderBrowserDialog folderBrowserFialog = new FolderBrowserDialog
            {
                Description = "请选择测试目录",
                SelectedPath = testDir
            };

            // 显示对话框并判断用户是否指定新的目录
            if (folderBrowserFialog.ShowDialog() == DialogResult.OK)
            {
                testDir = folderBrowserFialog.SelectedPath; // 用户指定新的目录
                ((CharmTextBox)mControls[0]).Text = testDir;
                IniOperation.WriteValue(FrmMain.APP_CFG_PATH, "cache", "test_filter_dir", testDir);
            }
        }

        // 测试规则按钮被单击事件
        private void btnTest_MouseClick(object sender, MouseEventArgs e)
        {
            // 初始化
            lsvFiles.Items.Clear();
            List<Filter> filterList = new List<Filter>();

            // 加载过滤器列表
            FrmMain.LoadFilterList(filterList);

            // 开始过滤
            LoadLocalFiles(filterList, "");
        }
        #endregion

        #region 方法
        // 初始化设置
        private void InitializeSetting()
        {
            // 设置窗体属性
            this.Icon = Properties.Resources.icon;
            this.Text = "过滤规则测试";

            // 绘制窗体背景
            DrawFormBackground();

            #region 创建窗体组件
            // 创建关闭系统按钮
            CharmSysButton btnClose = new CharmSysButton
            {
                SysButtonType = SysButtonType.Close,
                ToolTipText = "关闭",
                Location = new Point(this.Width - 44, 1)
            };

            // 创建测试目录文本框
            CharmTextBox txtTestDir = new CharmTextBox
            {
                Location = new Point(110, 10 + TITLE_HEIGHT),
                Width = 330
            };
            // 创建浏览路径按钮
            CharmButton btnViewPath = new CharmButton
            {
                ButtonType = ButtonType.Classic_Size_08223,
                Text = "浏览路径",
                ForeColor = Color.DarkGreen,
                Location = new Point(455, 12 + TITLE_HEIGHT)
            };

            // 创建测试规则按钮
            CharmButton btnTest = new CharmButton
            {
                ButtonType = ButtonType.Classic_Size_12425,
                Text = "开始测试规则",
                ForeColor = Color.MediumSlateBlue,
                Location = new Point(420, 435),
            };

            // 将控件添加到集合中
            this.Controls.Add(txtTestDir);

            // 创建控件集合
            mControls = new List<Control> { txtTestDir };
            mCharmControls = new List<CharmControl> { btnClose, btnViewPath, btnTest };

            // 关联控件事件
            btnClose.MouseClick += btnClose_MouseClick;
            btnViewPath.MouseClick += btnViewPath_MouseClick;
            btnTest.MouseClick += btnTest_MouseClick;
            #endregion

            // 加载本地设置
            LoadLocalSetting();
        }

        // 绘制窗体背景
        private void DrawFormBackground()
        {
            // 加载窗体背景图像资源
            Bitmap imgBkg = new Bitmap(this.Width, this.Height);

            #region 绘制窗体内容
            using (Graphics g = Graphics.FromImage(imgBkg))
            {
                // 设置参数
                const int bottomHeight = 45;

                // 绘制标题栏背景
                SolidBrush sb = new SolidBrush(Color.DodgerBlue);
                g.FillRectangle(sb, 0, 0, this.Width, TITLE_HEIGHT);
                // 绘制窗口图标
                g.DrawImage(Properties.Resources.logo, new Rectangle(6, 5, 20, 20));
                // 绘制窗口标题
                g.DrawString("过滤规则测试 - 七牛云盘",
                    new Font("微软雅黑", 10, FontStyle.Bold), Brushes.White, new Point(30, 6));
                // 绘制主面板区
                sb.Color = Color.FromArgb(255, Color.WhiteSmoke);
                g.FillRectangle(sb, 1, TITLE_HEIGHT, this.Width - 2, this.Height - TITLE_HEIGHT - bottomHeight);
                // 绘制横线
                g.DrawLine(Pens.DarkGray, 1, this.Height - bottomHeight, this.Width - 2, this.Height - bottomHeight);
                // 绘制按钮区
                sb.Color = Color.FromArgb(70, Color.LightGray);
                g.FillRectangle(sb, 1, this.Height - bottomHeight, this.Width - 2, bottomHeight);
                // 绘制边框
                g.DrawRectangle(new Pen(Color.DimGray), 0, 0, this.Width - 1, this.Height - 1);
            }
            #endregion

            // 设置背景资源
            this.BackgroundImage = new Bitmap(imgBkg);

            // 释放系统资源
            imgBkg.Dispose();
        }

        // 加载本地设置
        private void LoadLocalSetting()
        {
            // 测试目录
            string testDir = IniOperation.ReadValue(FrmMain.APP_CFG_PATH, "cache", "test_filter_dir");
            if (testDir.Length > 0 && Directory.Exists(testDir))
            {
                ((CharmTextBox)mControls[0]).Text = testDir;
            }
        }

        // 加载本地文件（可用于递归操作）
        private void LoadLocalFiles(List<Filter> filterList, string dirPrefix)
        {
            // 检查文件
            DirectoryInfo di = new DirectoryInfo(((CharmTextBox)mControls[0]).Text + "\\" + dirPrefix);
            try
            {
                foreach (FileInfo fi in di.GetFiles())
                {
                    string fileName = (dirPrefix.TrimStart('\\') + "\\" + fi.Name).TrimStart('\\').Replace("\\", "/");

                    bool isFilter = false;
                    foreach (Filter f in filterList)
                    {
                        switch (f.Type)
                        {
                            case FilterType.Contain:
                                if (fileName.Contains(f.Name))
                                {
                                    lsvFiles.Items.Add(new ListViewItem(new[] { fileName, "是", "*" + f.Name + "*" }));
                                    isFilter = true;
                                }
                                break;
                            case FilterType.Prefix:
                                if (fileName.StartsWith(f.Name))
                                {
                                    lsvFiles.Items.Add(new ListViewItem(new[] { fileName, "是", f.Name + "*" }));
                                    isFilter = true;
                                }
                                break;
                            case FilterType.Suffix:
                                if (fileName.EndsWith(f.Name))
                                {
                                    lsvFiles.Items.Add(new ListViewItem(new[] { fileName, "是", "*" + f.Name }));
                                    isFilter = true;
                                }
                                break;
                            case FilterType.FullMatch:
                                if (fileName.Equals(f.Name))
                                {
                                    lsvFiles.Items.Add(new ListViewItem(new[] { fileName, "是", f.Name }));
                                    isFilter = true;
                                }
                                break;
                        }

                        if (isFilter)
                            break;
                    }

                    if (!isFilter)
                        lsvFiles.Items.Add(new ListViewItem(new[] { fileName, "否", "-" }));
                }

                // 检查目录
                foreach (DirectoryInfo subDi in di.GetDirectories())
                    LoadLocalFiles(filterList, dirPrefix + "\\" + subDi.Name);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch { }
        }
        #endregion
    }
}
