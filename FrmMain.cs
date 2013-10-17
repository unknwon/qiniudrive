#region 文档说明
/* ******************************************************************************************************
 * 文档作者：无闻
 * 创建日期：2013 年 10 月 16 日
 * 文档用途：七牛云盘主窗口
 * ------------------------------------------------------------------------------------------------------
 * 修改记录：
 * ------------------------------------------------------------------------------------------------------
 * 参考文献：
 * ******************************************************************************************************/
#endregion

#region 命名空间引用
using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

using CharmCommonMethod;
using CharmControlLibrary;
#endregion

namespace QiNiuDrive
{
    // 七牛云盘主窗口
    public partial class FrmMain : Form
    {
        #region 常量
        private const string APP_NAME = "七牛云盘";             // 软件名称
        private const string APP_VER = "v0.0.1";                // 软件版本
        private const string APP_CFG_PATH = "Config\\app.ini";  // 软件配置路径
        private const int TITLE_HEIGHT = 30;                    // 标题栏高度
        #endregion

        #region 字段
        #region 主窗口
        // * 私有字段 *
        private string[] mMenuNames;                // 菜单项名称
        private int mMenuSelectedIndex = 3;             // 菜单栏现行选中项索引
        private string mStatusText = string.Empty;  // 状态文本

        // * 用户控件 *
        private List<CharmControl> mCharmControls;          // Charm 控件集合
        private readonly ToolTip mToolTip = new ToolTip();  // 工具提示文本控件
        private NotifyIcon mNotifyIcon;                     // 托盘图标
        #endregion

        #region 关于
        private List<CharmControl> mAboutCharmControls; // 关于面板 Charm 控件集合
        // 
        #endregion
        #endregion

        #region 窗体事件
        // 窗体构造方法
        public FrmMain()
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

        // 窗体即将关闭事件
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 释放系统资源
            mNotifyIcon.Dispose();
        }

        // 窗体鼠标单击事件
        private void frmMain_MouseClick(object sender, MouseEventArgs e)
        {
            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 3: // 关于
                    CharmControl.MouseClickEvent(e, mAboutCharmControls);
                    break;
            }
            #endregion

            // 调用事件
            CharmControl.MouseClickEvent(e, mCharmControls);
        }

        // 窗体鼠标按下事件
        private void frmMain_MouseDown(object sender, MouseEventArgs e)
        {
            // 只响应鼠标左键
            if (e.Button == MouseButtons.Left)
            {
                #region 菜单栏事件处理
                // 轮询菜单项
                for (int i = 0; i < mMenuNames.Length; i++)
                {
                    // 判断是否为现行选中项且未被激活
                    if (mMenuSelectedIndex == i || e.X <= 1 || e.X >= 91 ||
                        e.Y <= TITLE_HEIGHT + 1 + i * 26 || e.Y >= TITLE_HEIGHT + 5 + (i + 1) * 26)
                        continue;

                    mMenuSelectedIndex = i;     // 设置菜单栏现行选中项索引
                    ShowPanels();               // 设置控件可见性

                    // 重绘菜单栏区域
                    this.Invalidate();
                    //this.Invalidate(new Rectangle(1, 23, 90, mMenuNames.Length * 26));      
                    return;
                }
                #endregion
            }

            // 指示是否捕捉到事件
            bool isCaptureEvent = false;

            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 3: // 关于
                    isCaptureEvent = CharmControl.MouseDownEvent(e, mAboutCharmControls, this);
                    break;
            }
            #endregion

            // 判断之前是否已捕捉到事件
            if (isCaptureEvent) return;

            // 调用事件
            if (!CharmControl.MouseDownEvent(e, mCharmControls, this))
                APIOperation.MoveNoBorderForm(this, e);
        }

        // 窗体鼠标移动事件
        private void frmMain_MouseMove(object sender, MouseEventArgs e)
        {
            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 3: // 关于
                    CharmControl.MouseMoveEvent(e, mAboutCharmControls, this, mToolTip);
                    break;
            }
            #endregion

            // 调用事件
            CharmControl.MouseMoveEvent(e, mCharmControls, this, mToolTip);
        }

        // 窗体鼠标弹起事件
        private void frmMain_MouseUp(object sender, MouseEventArgs e)
        {
            #region 主面板事件
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 3: // 关于
                    CharmControl.MouseUpEvent(e, mAboutCharmControls, this);
                    break;
            }
            #endregion

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

            #region 绘制菜单栏
            // 轮询菜单项
            for (int i = 0; i < mMenuNames.Length; i++)
            {
                // 判断是否为现行选中项
                if (mMenuSelectedIndex == i)
                {
                    g.DrawImage(Properties.Resources.tab_button_setting_chosen, new Point(1, TITLE_HEIGHT + 1 + i * 26));
                    g.DrawString(mMenuNames[i], this.Font, Brushes.White, new Point(20, TITLE_HEIGHT + 5 + i * 26));
                }
                else
                    g.DrawString(mMenuNames[i], this.Font, Brushes.Black, new Point(20, TITLE_HEIGHT + 5 + i * 26));
            }
            #endregion

            #region 绘制主面板
            // 根据菜单现行选中项索引判断绘制哪个面板
            switch (mMenuSelectedIndex)
            {
                case 3: // 关于
                    DrawAboutPanel(g);
                    CharmControl.PaintEvent(e.Graphics, mAboutCharmControls);
                    break;
            }
            #endregion

            // 绘制控件
            CharmControl.PaintEvent(e.Graphics, mCharmControls);

            // 重置重绘索引
            //mMultipleSettingRedrawIndex = 0;
            //mDownloadSettingRedrawIndex = 0;
        }
        #endregion

        #region 控件事件
        #region 主窗口
        // 关闭按钮被单击事件
        private void btnClose_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 关于
        // GitHub 链接标签鼠标单击事件
        private static void lblGithub_MouseClick(object sender, MouseEventArgs e)
        {
            Process.Start("https://github.com/Unknwon/qiniudrive");
        }

        // 新浪链接标签鼠标单击事件
        private static void lblSina_MouseClick(object sender, MouseEventArgs e)
        {
            Process.Start("http://weibo.com/Obahua");
        }
        #endregion
        #endregion

        #region 方法
        // 初始化设置
        private void InitializeSetting()
        {
            // 设置窗体属性
            this.Icon = Properties.Resources.icon;
            this.Text = APP_NAME;

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

            // 创建控件集合
            mCharmControls = new List<CharmControl> { btnClose };

            // 关联控件事件
            btnClose.MouseClick += btnClose_MouseClick;
            #endregion

            #region 创建主体区域
            // 创建关于面板
            CreateAboutPanel();
            #endregion

            #region 创建托盘图标
            // 创建托盘图标
            mNotifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.icon,
                Text = APP_NAME + "\n" + "未启动同步",
                Visible = true
            };

            // * 关联控件事件 *
            //mNotifyIcon.MouseClick += new MouseEventHandler(notifyIcon_MouseClick);
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
                const int menuWidth = 90;

                // 绘制标题栏背景
                SolidBrush sb = new SolidBrush(Color.DodgerBlue);
                g.FillRectangle(sb, 0, 0, this.Width, TITLE_HEIGHT);
                // 绘制窗口图标
                g.DrawImage(Properties.Resources.logo, new Rectangle(6, 5, 20, 20));
                // 绘制窗口标题
                g.DrawString("设置中心 - 七牛云盘",
                    new Font("微软雅黑", 10, FontStyle.Bold), Brushes.White, new Point(30, 6));
                // 绘制竖线
                g.DrawLine(Pens.DarkGray, menuWidth + 1, TITLE_HEIGHT, 91, this.Height);
                // 绘制菜单区
                sb.Color = Color.FromArgb(100, Color.LightGray);
                g.FillRectangle(sb, 1, TITLE_HEIGHT, 90, this.Height);
                // 绘制主面板区
                sb.Color = Color.FromArgb(255, Color.WhiteSmoke);
                g.FillRectangle(sb, menuWidth + 2, TITLE_HEIGHT, this.Width - menuWidth - 4, this.Height - TITLE_HEIGHT - bottomHeight);
                // 绘制横线
                g.DrawLine(Pens.DarkGray, menuWidth + 2, this.Height - bottomHeight, this.Width - 2, this.Height - bottomHeight);
                //// 绘制按钮区
                sb.Color = Color.FromArgb(70, Color.LightGray);
                g.FillRectangle(sb, menuWidth + 2, this.Height - bottomHeight, this.Width - 2, bottomHeight);
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
            // 获取菜单项名称
            mMenuNames = IniOperation.ReadValue(APP_CFG_PATH, "setting", "menu_names").Split('|');
        }

        // 显示面板
        private void ShowPanels()
        {
            // 设置面板相关控件的可见性
        }

        #region 创建面板方法
        // 创建关于面板
        private void CreateAboutPanel()
        {

            // 创建 GitHub 链接标签
            CharmLinkLabel lblGithub = new CharmLinkLabel
            {
                Location = new Point(185, 145),
                Font = new Font("微软雅黑", 10),
                ForeColor = Color.Blue,
                Text = "github.com/Unknwon/qiniudrive"
            };

            // 创建新浪微博链接标签
            CharmLinkLabel lblSina = new CharmLinkLabel
            {
                Location = new Point(330, 223),
                Font = new Font("微软雅黑", 10),
                ForeColor = Color.Blue,
                Text = "@无闻Unknown"
            };

            // 关联控件事件
            lblGithub.MouseClick += lblGithub_MouseClick;
            lblSina.MouseClick += lblSina_MouseClick;

            // 创建关于面板控件集合
            mAboutCharmControls = new List<CharmControl> { lblGithub, lblSina };
        }
        #endregion

        #region 绘制面板方法
        // 绘制关于面板
        private void DrawAboutPanel(Graphics g)
        {
            Font font = new Font("微软雅黑", 10, FontStyle.Bold);
            // 基本说明
            string intro = "本程序是基于七牛开放 API 构建的第三方同步程序\n" +
                                "七牛云存储官方不对本程序的任何行为负责。";
            g.DrawString(intro, font, Brushes.Black, 185, 60);
            // 开源说明
            intro = "七牛云盘由 C# 编写并已开源在 GitHub 上：";
            g.DrawString(intro, font, Brushes.Black, 185, 120);
            // 版权说明
            g.DrawImage(Properties.Resources.unknown, 190, 200, 64, 64);
            //font = new Font("微软雅黑", 10);
            intro = "程序作者：无闻\n" +
                    "新浪微博：\n" +
                    "个人博客：暂未开通";
            g.DrawString(intro, font, Brushes.Black, 265, 205);
            intro = "版权所有 @ 2013 无闻";
            g.DrawString(intro, font, Brushes.Black, 265, 285);
            // 绘制七牛 LOGO
            g.DrawImage(Properties.Resources.qiniu_logo, 190, 330, 290, 45);
        }
        #endregion
        #endregion
    }
}
